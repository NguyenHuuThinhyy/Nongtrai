"""Local companion API. Save files and model cache remain on the user's PC."""
import sys
import os
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
import uvicorn
if __name__ == "__main__":
    host = "0.0.0.0" if "--lan" in sys.argv else "127.0.0.1"
    os.environ["FARM_BIND_HOST"] = host
    uvicorn.run("app.main:app", host=host, port=8000)
