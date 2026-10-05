import json
import os
import sys
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
os.environ["FARM_PAIRING_KEY"] = "test-pairing-only"
from fastapi.testclient import TestClient
from app import main
from app.cloud import FarmCloud


class FakeMQTT:
    def __init__(self): self.sent = []
    def publish(self, *args, **kwargs): self.sent.append(args)


class CloudTests(unittest.TestCase):
    def setUp(self):
        self.now = 100.0
        self.cloud = FarmCloud(lambda: self.now)
        self.cloud.connected = True
        self.cloud.client = FakeMQTT()
        self.data = dict(session_id="session-a", moisture_percent=20, growth_percent=30, station_built=True, pump_active=True, simulation=True)

    def test_live_command_session_duplicate_and_stale_ack(self):
        self.assertFalse(self.cloud.receive_command(False))
        self.cloud.telemetry(self.data)
        self.assertTrue(self.cloud.receive_command(False))
        first = self.cloud.control("session-a")
        self.assertFalse(first["pump_enabled"])
        self.assertFalse(self.cloud.receive_command(False))
        self.assertEqual(first["revision"], self.cloud.revision)
        self.assertFalse(self.cloud.control("session-b")["has_command"])
        with self.assertRaises(ValueError): self.cloud.ack(dict(session_id="session-a", server_id="old-boot", revision=first["revision"]))
        self.cloud.ack(dict(session_id="session-a", server_id=first["server_id"], revision=first["revision"]))
        self.now += 66
        self.assertFalse(self.cloud.control("session-a")["cloud_available"])
        self.assertFalse(self.cloud.receive_command(True))

    def test_retained_message_reconnect_and_disconnect(self):
        self.cloud.telemetry(self.data)
        self.cloud._message(None, None, SimpleNamespace(retain=True, payload=b"OFF"))
        self.assertIsNone(self.cloud.command)
        self.cloud.receive_command(False)
        self.cloud._disconnected(None, None, None, None, None)
        self.assertTrue(self.cloud.control("session-a")["pump_enabled"])
        self.cloud.disconnect("session-a")
        self.assertEqual(self.cloud.session, "")

    def test_telemetry_rate_limit_and_exclusive_owner(self):
        for _ in range(100): self.cloud.telemetry(self.data)
        self.assertEqual(len(self.cloud.client.sent), 3)
        with self.assertRaises(ValueError): self.cloud.telemetry(dict(self.data, session_id="session-b"))
        self.now += 20
        self.cloud.telemetry(self.data)
        self.assertEqual(len(self.cloud.client.sent), 6)


class APITests(unittest.TestCase):
    def setUp(self):
        self.client = TestClient(main.app)
        self.headers = {"X-Farm-Key": "test-pairing-only"}
        self.fresh = FarmCloud()
        self.override = patch.object(main, "cloud", self.fresh)
        self.override.start()

    def tearDown(self): self.override.stop(); self.client.close()

    def test_pairing_and_validation(self):
        self.assertEqual(self.client.get("/v1/pair").status_code, 401)
        self.assertEqual(self.client.get("/v1/pair", headers=self.headers).json(), {"paired": True})
        self.assertEqual(self.client.post("/v1/chat", json={"question": "x"}).status_code, 401)
        self.assertEqual(self.client.post("/v1/chat", headers=self.headers, json={"question": "x" * 801}).status_code, 422)
        invalid = dict(session_id="session-a", moisture_percent=101, growth_percent=0, station_built=False, pump_active=False)
        self.assertEqual(self.client.post("/v1/telemetry", headers=self.headers, json=invalid).status_code, 422)
        invalid["moisture_percent"] = 20; invalid["simulation"] = False
        self.assertEqual(self.client.post("/v1/telemetry", headers=self.headers, json=invalid).status_code, 422)

    def test_cloud_not_configured_does_not_invent_success(self):
        body = dict(session_id="session-a", moisture_percent=50, growth_percent=10, station_built=True, pump_active=True)
        self.assertEqual(self.client.post("/v1/telemetry", headers=self.headers, json=body).status_code, 200)
        result = self.client.get("/v1/control?session_id=session-a", headers=self.headers).json()
        self.assertFalse(result["cloud_available"])
        self.assertTrue(result["pump_enabled"])
        self.assertEqual(self.client.delete("/v1/control?session_id=session-a", headers=self.headers).status_code, 200)

    def test_unknown_question_has_no_fake_model_answer(self):
        result = self.client.post("/v1/chat", headers=self.headers, json={"question": "quantum astrophysics"}).json()
        self.assertFalse(result["generated"])
        self.assertEqual(result["sources"], [])
        result = self.client.post("/v1/chat", headers=self.headers,
            json={"question": "Thời tiết ở Sao Hỏa?"}).json()
        self.assertFalse(result["generated"])
        self.assertEqual(result["sources"], [])
        result = self.client.post("/v1/chat", headers=self.headers,
            json={"question": "quantum astrophysics", "history": [{"role": "user", "content": "Cung hỏng sửa thế nào?"}]}).json()
        self.assertFalse(result["generated"])
        self.assertEqual(result["sources"], [])

    def test_model_request_contract_and_sources(self):
        observed = {}
        class FakeClient:
            def __init__(self, **kwargs): pass
            async def __aenter__(self): return self
            async def __aexit__(self, *args): pass
            async def post(self, url, json):
                observed.update(json)
                import httpx
                return httpx.Response(200, request=httpx.Request("POST", url), json={"message": {"content": '{"section_id":"bow"}'}})
        with patch.object(main.httpx, "AsyncClient", FakeClient):
            response = self.client.post("/v1/chat", headers=self.headers, json={"question": "Cung có giảm độ bền không?"})
        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.json()["generated"])
        self.assertIn("Cung và mũi tên", response.json()["sources"])
        self.assertIn("1 độ bền", response.json()["answer"])
        self.assertEqual(response.json()["generation_mode"], "extractive")
        self.assertIn("bow", observed["format"]["properties"]["section_id"]["enum"])
        self.assertLessEqual(len(observed["format"]["properties"]["section_id"]["enum"]),4)
        self.assertFalse(observed["think"])
        self.assertLessEqual(observed["options"]["num_predict"], 220)
        self.assertIn("HƯỚNG DẪN", observed["messages"][0]["content"])
        with patch.object(main.httpx, "AsyncClient", FakeClient):
            followup = self.client.post("/v1/chat", headers=self.headers, json={"question":"Giá bao nhiêu?","history":[{"role":"user","content":"Cung hỏng sửa thế nào?"}]})
        self.assertEqual(followup.status_code,200)
        self.assertIn("bow",observed["format"]["properties"]["section_id"]["enum"])
        self.assertIn("20 xu",followup.json()["answer"])
        self.assertIn("Cung hỏng sửa thế nào?", observed["messages"][-1]["content"])
        self.assertIn("Giá bao nhiêu?", observed["messages"][-1]["content"])
        self.assertEqual(observed["format"]["properties"]["section_id"]["enum"], ["", "bow"])
        with patch.object(main.httpx, "AsyncClient", FakeClient):
            self.client.post("/v1/chat", headers=self.headers, json={"question": "Tốn bao nhiêu?", "history": [
                {"role": "user", "content": "Cung hỏng sửa thế nào?"},
                {"role": "assistant", "content": "Sửa cung trong túi."},
                {"role": "user", "content": "Giá bao nhiêu?"}]})
        self.assertIn("Cung hỏng sửa thế nào?", observed["messages"][-1]["content"])

    def test_thirty_retrieval_questions_and_unity_manual_match(self):
        cases = json.loads(Path(__file__).with_name("chat-evaluation.json").read_text(encoding="utf-8"))
        for case in cases:
            with self.subTest(case["question"]):
                self.assertIn(case["section"], [s["id"] for s in main.knowledge.search(case["question"])])
        for question,section in [("AR PC dùng webcam thế nào?","ar"),("Phím nào mở chatbot và ẩn hướng dẫn?","chat")]:
            with self.subTest(question):
                self.assertIn(section,[s["id"] for s in main.knowledge.search(question)])
        unity = Path(__file__).resolve().parents[2] / "Assets/Farm/Resources/FarmTechnology/knowledge.json"
        self.assertEqual(unity.read_bytes(), Path(main.__file__).with_name("knowledge.json").read_bytes())


if __name__ == "__main__": unittest.main(verbosity=2)
