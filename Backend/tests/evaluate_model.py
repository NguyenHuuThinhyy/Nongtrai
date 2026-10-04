"""Record real Ollama responses; content correctness still requires review.

Run after closing Unity Editor on an 8 GB PC. Never substitutes a mock model.
"""
import argparse
import json
import os
import statistics
import sys
import time
from datetime import datetime, timezone
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
os.environ["FARM_PAIRING_KEY"] = "local-evaluation-only"
from fastapi.testclient import TestClient
from app.main import app

parser = argparse.ArgumentParser()
parser.add_argument("--output", required=True)
parser.add_argument("--limit", type=int, default=30)
args = parser.parse_args()
cases = json.loads(Path(__file__).with_name("chat-evaluation.json").read_text(encoding="utf-8"))[:args.limit]
output = Path(args.output)
output.parent.mkdir(parents=True, exist_ok=True)
report = {"time_utc": datetime.now(timezone.utc).isoformat(), "runtime": "native Windows Ollama CPU + FastAPI TestClient (not Docker)", "model": "qwen3:1.7b", "results": []}
with TestClient(app) as client:
    health = client.get("/health").json()
    report["health"] = health
    if not health["model_ready"]:
        raise SystemExit("Real model is not downloaded or Ollama is not serving")
    for index, case in enumerate(cases):
        start = time.perf_counter()
        response = client.post("/v1/chat", headers={"X-Farm-Key": "local-evaluation-only"}, json={"question": case["question"]})
        seconds = round(time.perf_counter() - start, 3)
        report["results"].append(dict(case, seconds=seconds, http_status=response.status_code, response=response.json(), content_review=None))
        output.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"{index+1}/{len(cases)} HTTP {response.status_code} {seconds}s", flush=True)
times = [r["seconds"] for r in report["results"][1:] if r["http_status"] == 200]
report["summary"] = {"requests": len(cases), "generated": sum(r["response"].get("generated", False) for r in report["results"]), "warm_mean_seconds": round(statistics.mean(times), 3) if times else None, "warm_over_30_seconds": sum(t > 30 for t in times), "content_accuracy": "pending review against expected/source; no automatic correctness claim"}
output.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps(report["summary"], ensure_ascii=False), flush=True)
