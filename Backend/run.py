"""Optional native diagnostic runner; submission Docker startup uses compose.yaml."""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
import uvicorn
if __name__ == "__main__": uvicorn.run("app.main:app", host="127.0.0.1", port=8000)
