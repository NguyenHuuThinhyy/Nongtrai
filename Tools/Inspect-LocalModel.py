"""Read GGUF metadata without importing or running a model. Copyright TriForge."""
import json
import struct
import sys
from pathlib import Path

with Path(sys.argv[1]).open("rb") as f:
    def read(fmt):
        return struct.unpack("<" + fmt, f.read(struct.calcsize("<" + fmt)))[0]
    def string():
        return f.read(read("Q")).decode("utf-8")
    formats = {0:"B",1:"b",2:"H",3:"h",4:"I",5:"i",6:"f",7:"?",10:"Q",11:"q",12:"d"}
    def value(kind, keep=True):
        if kind == 8:
            size = read("Q")
            if keep: return f.read(size).decode("utf-8")
            f.seek(size, 1)
        elif kind == 9:
            element, size = read("I"), read("Q")
            for _ in range(size): value(element, False)
            return {"count":size}
        else:
            return read(formats[kind])
    assert f.read(4) == b"GGUF", "Invalid model header"
    version, tensors, entries = read("I"), read("Q"), read("Q")
    metadata = {}
    for _ in range(entries):
        key, kind = string(), read("I")
        result = value(kind, not key.startswith("tokenizer.ggml."))
        if not key.startswith("tokenizer.ggml."): metadata[key] = result
    print(json.dumps({"version":version,"tensors":tensors,"metadata":metadata},ensure_ascii=False,indent=2))
