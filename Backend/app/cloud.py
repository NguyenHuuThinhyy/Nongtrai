"""Session-bound simulated farm telemetry and Adafruit IO control."""
import logging
import os
import threading
import time
import uuid
from collections import deque

import paho.mqtt.client as mqtt

log = logging.getLogger("farm.cloud")


class FarmCloud:
    def __init__(self, clock=time.monotonic):
        self.clock = clock
        self.lock = threading.RLock()
        self.client = None
        self.connected = False
        self.session = ""
        self.expires = 0.0
        self.revision = 0
        self.command = None
        self.last_publish = -100.0
        self.last_ack = None
        self.published = deque()
        self.boot = uuid.uuid4().hex
        self.prefix = os.getenv("ADAFRUIT_IO_PREFIX", "farm")
        self.username = os.getenv("ADAFRUIT_IO_USERNAME", "")

    def start(self):
        secret = os.getenv("ADAFRUIT_IO_KEY", "")
        if not self.username or not secret:
            log.info("Cloud disabled: configure Adafruit IO credentials to enable live MQTT")
            return
        client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id="farm-" + self.boot[:12], clean_session=True)
        client.username_pw_set(self.username, secret)
        client.tls_set()
        client.on_connect = self._connected
        client.on_disconnect = self._disconnected
        client.on_message = self._message
        client.reconnect_delay_set(min_delay=2, max_delay=30)
        self.client = client
        client.connect_async("io.adafruit.com", 8883, 60)
        client.loop_start()

    def stop(self):
        if self.client:
            self.client.disconnect()
            self.client.loop_stop()

    def _connected(self, client, userdata, flags, reason_code, properties):
        with self.lock:
            self.connected = not reason_code.is_failure
            self.command = None
            self.revision += 1
        if self.connected:
            client.subscribe(f"{self.username}/feeds/{self.prefix}-pump-command", qos=1)

    def _disconnected(self, client, userdata, flags, reason_code, properties):
        with self.lock:
            self.connected = False
            self.command = None
            self.revision += 1

    def _message(self, client, userdata, message):
        if message.retain:
            return
        try:
            value = message.payload.decode("utf-8").strip().upper()
        except UnicodeDecodeError:
            return
        if value not in {"ON", "OFF", "1", "0"}:
            return
        self.receive_command(value in {"ON", "1"})

    def receive_command(self, enabled: bool):
        with self.lock:
            if not self.connected or not self.session or self.clock() > self.expires:
                return False
            if self.command is enabled:
                return False
            self.command = enabled
            self.revision += 1
            return True

    def telemetry(self, data: dict):
        now = self.clock()
        with self.lock:
            if self.session and self.session != data["session_id"] and now < self.expires:
                raise ValueError("Another game session is connected; disconnect it first")
            if self.session != data["session_id"] or now >= self.expires:
                self.session = data["session_id"]
                self.command = None
                self.revision += 1
                self.last_publish = -100.0
            self.expires = now + 65
            if not self.connected or self.client is None or now - self.last_publish < 20:
                return
            while self.published and self.published[0] < now - 60:
                self.published.popleft()
            if len(self.published) > 17:
                return
            self.last_publish = now
            for feed, value in (("moisture", data["moisture_percent"]), ("growth", data["growth_percent"]), ("pump-state", "ON" if data["pump_active"] else "OFF")):
                self.client.publish(f"{self.username}/feeds/{self.prefix}-{feed}", str(value), qos=1, retain=False)
                self.published.append(now)

    def control(self, session: str) -> dict:
        with self.lock:
            active = self.connected and session == self.session and self.clock() < self.expires
            return {"session_id": session, "server_id": self.boot, "revision": self.revision,
                    "cloud_available": active, "has_command": active and self.command is not None,
                    "pump_enabled": self.command if active and self.command is not None else True}

    def ack(self, data: dict):
        with self.lock:
            if data["session_id"] != self.session or self.clock() >= self.expires:
                raise ValueError("Expired game session")
            if data["server_id"] != self.boot or data["revision"] != self.revision:
                raise ValueError("Stale control acknowledgement")
            self.last_ack = data

    def disconnect(self, session: str):
        with self.lock:
            if session == self.session:
                self.session = ""
                self.expires = 0
                self.command = None
                self.revision += 1
