"""Start the desktop assistant, pair the game, and own only our child processes.

Copyright TriForge. Invoked by the user's CHAY_GAME_CO_TRO_LY.bat.
"""
import argparse
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import sys
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parent
API = "http://127.0.0.1:8000"
OLLAMA = "http://127.0.0.1:11434"


def request(url, data=None, key="", timeout=5):
    headers = {"Content-Type": "application/json"}
    if key:
        headers["X-Farm-Key"] = key
    body = None if data is None else json.dumps(data).encode("utf-8")
    # LAN/loopback connections must not be sent through a system web proxy.
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    with opener.open(urllib.request.Request(url, body, headers), timeout=timeout) as response:
        return json.load(response)


def read_settings(path):
    values = {}
    if path.exists():
        for line in path.read_text(encoding="utf-8-sig").splitlines():
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                name, value = line.split("=", 1)
                values[name.strip()] = value.strip().strip('"').strip("'")
    return values


def prepare_settings(backend=ROOT):
    path = backend / ".env"
    values = read_settings(path)
    if not values.get("FARM_PAIRING_KEY") or values["FARM_PAIRING_KEY"] == "change-this-pairing-code":
        values["FARM_PAIRING_KEY"] = secrets.token_hex(24)
        values.setdefault("OLLAMA_MODEL", "qwen3:1.7b")
        # Preserve all existing cloud settings; never print their values.
        path.write_text("\n".join(f"{k}={v}" for k, v in values.items()) + "\n", encoding="utf-8")
    return values


def pair_game(key, preferences=None):
    if preferences is None:
        preferences = Path(os.environ["USERPROFILE"]) / "AppData/LocalLow/Nong Trai Studio/Nong Trai - First Harvest"
    preferences.mkdir(parents=True, exist_ok=True)
    target = preferences / "farm-connection.json"
    value = {"url": API, "key": key}
    if target.exists():
        old = target.read_text(encoding="utf-8-sig")
        try:
            if json.loads(old) == value:
                return
        except ValueError:
            pass
        backup = preferences / ("farm-connection.before-assistant-" + time.strftime("%Y%m%d-%H%M%S") + ".json")
        if not backup.exists():
            backup.write_text(old, encoding="utf-8")
    temporary = target.with_suffix(".tmp")
    temporary.write_text(json.dumps(value), encoding="utf-8")
    temporary.replace(target)


def port_busy(port):
    with socket.socket() as sock:
        sock.settimeout(.5)
        return sock.connect_ex(("127.0.0.1", port)) == 0


def wait_ready(url, child, seconds=45):
    deadline = time.monotonic() + seconds
    while time.monotonic() < deadline:
        if child.poll() is not None:
            raise RuntimeError("Dich vu dung som. Xem log trong Backend/.runtime.")
        try:
            return request(url)
        except (OSError, ValueError):
            time.sleep(.5)
    raise RuntimeError("Dich vu khoi dong qua lau. Xem log trong Backend/.runtime.")


def find_game(project):
    for relative in ("Builds/Windows-LocalAI/NongTrai.exe", "NongTrai.exe"):
        candidate = project / relative
        if candidate.is_file():
            return candidate
    raise RuntimeError("Thieu game. Giai nen day du goi Windows, giu EXE, Data, DLL va Backend.")


def start(args):
    runtime = ROOT / ".runtime"
    runtime.mkdir(exist_ok=True)
    values = prepare_settings()
    key = values["FARM_PAIRING_KEY"]
    model = json.loads((ROOT / "MODEL-MANIFEST.json").read_text(encoding="utf-8-sig"))
    env = os.environ.copy()
    env.update(values)
    env.update(OLLAMA_HOST="127.0.0.1:11434", OLLAMA_URL=OLLAMA,
               OLLAMA_MODEL=model["name"], OLLAMA_NUM_PARALLEL="1", OLLAMA_MAX_LOADED_MODELS="1")
    if args.models:
        env["OLLAMA_MODELS"] = str(Path(args.models).resolve())
    children, logs = [], []

    def spawn(command, label):
        log = (runtime / (label + ".log")).open("a", encoding="utf-8")
        logs.append(log)
        child = subprocess.Popen(command, cwd=ROOT, env=env, stdout=log, stderr=subprocess.STDOUT,
                                 creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0)
        children.append(child)
        return child

    try:
        print("[1/4] Khoi dong Ollama...", flush=True)
        if not port_busy(11434):
            server = spawn([args.ollama, "serve"], "ollama")
            wait_ready(OLLAMA + "/api/tags", server)
        tags = request(OLLAMA + "/api/tags").get("models", [])
        selected = next((m for m in tags if m.get("name") == model["name"]), None)
        if selected is None:
            print("Tai model Qwen3 lan dau (~1.36 GB). Cho tai xong; cac lan sau dung ban da luu.", flush=True)
            result = subprocess.run([args.ollama, "pull", model["name"]], env=env)
            if result.returncode:
                raise RuntimeError("Tai model that bai. Kiem tra Internet roi chay lai.")
            selected = next((m for m in request(OLLAMA + "/api/tags").get("models", []) if m.get("name") == model["name"]), None)
        if not selected or selected.get("digest", "").removeprefix("sha256:") != model["manifest_digest"]:
            raise RuntimeError("Model khong khop phien ban da kiem tra. Xem Backend/MODEL-MANIFEST.json.")

        print("[2/4] Khoi dong backend va kiem tra ma ket noi...", flush=True)
        if not port_busy(8000):
            command = [sys.executable, str(ROOT / "run.py")]
            if args.lan:
                command.append("--lan")
            api = spawn(command, "api")
            wait_ready(API + "/health", api)
        try:
            if not request(API + "/v1/pair", key=key).get("paired"):
                raise ValueError("Not paired")
        except (OSError, ValueError) as error:
            raise RuntimeError("Cong 8000 dang dung boi dich vu khac hoac ma khac. Dong backend cu roi chay lai; khong tu tat tien trinh cua ban.") from error
        health = request(API + "/health")
        if health.get("service") != "nongtrai":
            raise RuntimeError("Backend cu hoac khong dung game. Dong backend cu roi chay lai.")
        if args.lan and health.get("listen_host") != "0.0.0.0":
            raise RuntimeError("Backend dang chi mo cho PC. Dong launcher PC roi chay BAT_BACKEND_CHO_DIEN_THOAI.bat.")
        if not health.get("model_ready"):
            raise RuntimeError("Backend chua thay model. Xem Backend/.runtime/api.log.")
        if args.verify_model:
            print("[3/4] Kiem tra AI bang mot cau hoi that (co the mat 1-2 phut)...", flush=True)
            answer = request(API + "/v1/chat", {"question": "Cung hong sua bao nhieu xu?"}, key, timeout=100)
            if not answer.get("generated") or "20 xu" not in answer.get("answer", ""):
                raise RuntimeError("Model chua tra loi dung cau kiem tra. Xem log, chua bao san sang.")
        else:
            print("[3/4] Che do nhe: AI chi nap vao RAM khi ban gui cau hoi.", flush=True)
        pair_game(key)
        (runtime / "connection.txt").write_text(
            "PC URL: " + API + "\nFARM_PAIRING_KEY: " + key +
            "\nAndroid: dung IP LAN cua PC va khoi dong BAT_BACKEND_CHO_DIEN_THOAI.bat.\n", encoding="utf-8")
        print("[4/4] BACKEND SAN SANG. Da tu ket noi game. Nhan C de hoi; AR khong can backend.\n", flush=True)
        if args.no_game:
            print("Giu cua so nay khi choi. Ctrl+C de tat cac dich vu do lan chay nay mo.", flush=True)
            while True:
                time.sleep(1)
                if any(child.poll() is not None for child in children):
                    raise RuntimeError("Dich vu da dung. Xem log roi chay lai.")
        else:
            game = find_game(ROOT.parent)
            game_args = [str(game)] + (["-farmMute"] if args.mute else [])
            with subprocess.Popen(game_args, cwd=game.parent) as player:
                player.wait()
    finally:
        # Reused user services are never included in children.
        for child in reversed(children):
            if child.poll() is None:
                if os.name == "nt":
                    subprocess.run(["taskkill", "/PID", str(child.pid), "/T", "/F"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                else:
                    child.terminate()
                try:
                    child.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    child.kill()
        for log in logs:
            log.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--ollama", required=True)
    parser.add_argument("--models")
    parser.add_argument("--no-game", action="store_true")
    parser.add_argument("--lan", action="store_true")
    parser.add_argument("--mute", action="store_true")
    parser.add_argument("--verify-model", action="store_true")
    try:
        start(parser.parse_args())
    except KeyboardInterrupt:
        print("Da dong tro ly.")
    except Exception as error:
        print("CHUA SAN SANG: " + str(error), file=sys.stderr)
        sys.exit(1)
