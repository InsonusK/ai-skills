from dataclasses import dataclass
from urllib.parse import urlsplit


@dataclass(frozen=True)
class Result:
    is_valid: bool
    normalized: str = ""
    error_code: str = ""


def check(raw_url: str) -> Result:
    trimmed = raw_url.strip()
    parts = urlsplit(trimmed)
    scheme = parts.scheme.lower()
    if scheme not in ("http", "https"):
        return Result(False, error_code="UNSUPPORTED_SCHEME")
    if not parts.netloc:
        return Result(False, error_code="MISSING_HOST")
    return Result(True, normalized=f"{scheme}://{parts.netloc.lower()}{parts.path}")
