"""Small Vietnamese BM25 index; no additional embedding model is required."""
import json
import math
import re
import unicodedata
from collections import Counter
from pathlib import Path


def tokens(text: str) -> list[str]:
    normalized = unicodedata.normalize("NFD", text.lower().replace("đ", "d"))
    return re.findall(r"[a-z0-9]+", "".join(c for c in normalized if not unicodedata.combining(c)))


class Knowledge:
    def __init__(self, path: Path):
        self.sections = json.loads(path.read_text(encoding="utf-8"))["sections"]
        self.docs = [Counter(tokens(s["title"] + " " + s["text"] + " " + s.get("keywords", ""))) for s in self.sections]
        self.lengths = [sum(d.values()) for d in self.docs]
        self.average = sum(self.lengths) / max(1, len(self.docs))
        self.frequency = Counter(word for doc in self.docs for word in doc)

    def search(self, query: str, limit: int = 3) -> list[dict]:
        words = set(tokens(query))
        scores = []
        # Common Vietnamese question particles must not retrieve unrelated game facts.
        words -= set(tokens("là gì làm sao thế nào tôi bạn có không trong của cho và với được game hỏi cách"))
        for i, doc in enumerate(self.docs):
            score = 0.0
            for word in words:
                count = doc[word]
                if not count:
                    continue
                idf = math.log(1 + (len(self.docs) - self.frequency[word] + .5) / (self.frequency[word] + .5))
                score += idf * count * 2.5 / (count + 1.5 * (.25 + .75 * self.lengths[i] / self.average))
            if score > 0:
                scores.append((score, i))
        scores.sort(reverse=True)
        return [self.sections[i] for _, i in scores[:limit]]
