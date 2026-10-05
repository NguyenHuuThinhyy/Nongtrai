"""Opt-in real HTTP/model follow-up check; no mock and no credentials in output."""
import argparse
import json
import os
from pathlib import Path

import httpx

parser = argparse.ArgumentParser()
parser.add_argument("--api", required=True)
parser.add_argument("--output", required=True)
args = parser.parse_args()
records = []
output = Path(args.output)
output.parent.mkdir(parents=True, exist_ok=True)
with httpx.Client(base_url=args.api, timeout=100,
                  headers={"X-Farm-Key": os.environ["FARM_EVAL_KEY"]}) as client:
    question = "Cung hỏng sửa bao nhiêu xu?"
    first = client.post("/v1/chat", json={"question": question})
    records.append({"question": question, "status": first.status_code, "response": first.json()})
    output.write_text(json.dumps(records, ensure_ascii=False, indent=2), encoding="utf-8")
    first.raise_for_status()
    assert first.json()["generated"] and "20 xu" in first.json()["answer"]
    history = [{"role": "user", "content": question},
               {"role": "assistant", "content": first.json()["answer"]}]
    follow = client.post("/v1/chat", json={"question": "Giá bao nhiêu?", "history": history})
    records.append({"question": "Giá bao nhiêu?", "status": follow.status_code, "response": follow.json()})
    output.write_text(json.dumps(records, ensure_ascii=False, indent=2), encoding="utf-8")
    follow.raise_for_status()
    assert follow.json()["generated"] and "20 xu" in follow.json()["answer"]
    assert "Cung và mũi tên" in follow.json()["sources"]
    outside = client.post("/v1/chat", json={"question": "Thời tiết ở Sao Hỏa?"})
    records.append({"question": "Thời tiết ở Sao Hỏa?", "status": outside.status_code, "response": outside.json()})
    output.write_text(json.dumps(records, ensure_ascii=False, indent=2), encoding="utf-8")
    outside.raise_for_status()
    assert not outside.json()["sources"] and "Chưa có thông tin" in outside.json()["answer"]
print("PASS real model follow-up and out-of-scope fallback")
