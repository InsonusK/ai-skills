# Source: skill vp-http-api-in-python

"""The HTTP entry point's own settings. They are read here, not in
{package}.config, so they come and go together with this package."""

import os
from dataclasses import dataclass


@dataclass(frozen=True)
class HttpConfig:
    listen_port: int = 8080
    """HTTP_LISTEN_PORT, the port the server listens on; default 8080."""

    base_path: str = ""
    """HTTP_BASE_PATH, the path prefix the service is published under: empty,
    or with a leading "/" and no trailing one. Routes stay mounted at "/";
    only URLs handed to a client carry it."""

    docs_enabled: bool = False
    """HTTP_API_DOCS_ENABLED: serve the OpenAPI spec and Swagger UI."""


_BOOLS = {"true": True, "1": True, "false": False, "0": False}


def load_config() -> HttpConfig:
    """Reads the settings; an unusable value raises ValueError naming its variable."""
    port = 8080
    if raw := os.environ.get("HTTP_LISTEN_PORT", ""):
        if not raw.isdigit() or int(raw) > 65535:
            raise ValueError(f"http config: HTTP_LISTEN_PORT {raw!r} is not a port")
        port = int(raw)
    docs = False
    if raw := os.environ.get("HTTP_API_DOCS_ENABLED", ""):
        if raw.lower() not in _BOOLS:
            raise ValueError(f"http config: HTTP_API_DOCS_ENABLED {raw!r} is not a bool")
        docs = _BOOLS[raw.lower()]
    return HttpConfig(listen_port=port, base_path=normalize_base_path(os.environ.get("HTTP_BASE_PATH", "")), docs_enabled=docs)


def normalize_base_path(path: str) -> str:
    """A path prefix in its one form: "" for none, otherwise a leading "/" and
    no trailing one ("orders/" and "/orders" both become "/orders")."""
    path = path.strip().strip("/")
    return "/" + path if path else ""
