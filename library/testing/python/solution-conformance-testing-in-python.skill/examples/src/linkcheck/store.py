import json
import threading
from pathlib import Path

from linkcheck.checker import Result
from linkcheck.mapping import from_record, to_record


class StoreCorrupted(Exception):
    """The history file holds a line that is not a record."""


class HistoryStore:
    """Keeps every check result as one JSON line of a file."""

    def __init__(self, path: Path):
        self._path = Path(path)
        self._lock = threading.Lock()

    def append(self, result: Result) -> None:
        line = json.dumps(to_record(result))
        with self._lock, self._path.open("a", encoding="utf-8") as history:
            history.write(line + "\n")

    def load(self) -> list[Result]:
        if not self._path.exists():
            return []
        results = []
        for line in self._path.read_text(encoding="utf-8").splitlines():
            try:
                results.append(from_record(json.loads(line)))
            except ValueError as error:
                raise StoreCorrupted(str(error)) from error
        return results
