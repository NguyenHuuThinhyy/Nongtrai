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


STOP_WORDS = set(tokens("là gì làm sao thế nào tôi bạn có không trong của cho và với được game hỏi cách ở khi thì rồi"))
COMMON_CONTROLS = set(tokens("pc android windows mobile phím nút nhấn bấm dùng mở ẩn giá bao nhiêu mấy mất tốn lâu giây nữa còn nó cái đó tổng thêm như hết lần một phải đầy rỗng"))


class Knowledge:
    def __init__(self, path: Path):
        self.sections = json.loads(path.read_text(encoding="utf-8"))["sections"]
        self.docs = [Counter(tokens(s["title"] + " " + s["text"] + " " + s.get("keywords", ""))) for s in self.sections]
        self.lengths = [sum(d.values()) for d in self.docs]
        self.average = sum(self.lengths) / max(1, len(self.docs))
        self.frequency = Counter(word for doc in self.docs for word in doc)
        # Explicit topics such as webcam/AR/chatbot identify a guide chapter;
        # platform names and general control words cannot identify a topic.
        self.topics = [set(tokens(s["title"] + " " + s.get("keywords", ""))) - STOP_WORDS - COMMON_CONTROLS for s in self.sections]
        self.topic_frequency = Counter(word for topic in self.topics for word in topic)

    def search(self, query: str, limit: int = 3) -> list[dict]:
        words = set(tokens(query))
        scores = []
        # Common Vietnamese question particles must not retrieve unrelated game facts.
        words -= STOP_WORDS
        for i, doc in enumerate(self.docs):
            # Multi-word questions need more than one coincidental word such
            # as 'thời' or an accent-normalized 'hỏa/hóa'. Single-item queries
            # still work; unsupported topics must not gain an unrelated source.
            if len(words.intersection(doc)) < min(2, len(words)):
                continue
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
        # Keep the existing relevance gate. Only narrow already eligible results
        # when a query names a unique indexed topic, avoiding model distraction
        # from shared words such as PC in the unrelated cloud chapter.
        anchored = [(score,i) for score,i in scores if any(self.topic_frequency[word]==1 and self.frequency[word]==1 for word in words.intersection(self.topics[i]))]
        if anchored:
            scores = anchored
        return [self.sections[i] for _, i in scores[:limit]]
