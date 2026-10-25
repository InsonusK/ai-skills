# Source: skill vp-http-api-in-python

"""The fixed part of the HTTP entry point, copied from templates and never
edited: settings (config.py), the server (server.py), every error answered as
google.rpc.Status (errors.py), the docs routes (docs.py), and one module per
wrapper around every request (logging.py). Like every entry point it knows
only the command layer - never domain, service, or infrastructure."""

from todo.api.http.pipeline.config import HttpConfig, load_config, normalize_base_path
from todo.api.http.pipeline.errors import status_response
from todo.api.http.pipeline.logging import REQUEST_ID_HEADER
from todo.api.http.pipeline.server import CallNext, Middleware, Server, run

__all__ = [
    "REQUEST_ID_HEADER",
    "CallNext",
    "HttpConfig",
    "Middleware",
    "Server",
    "load_config",
    "normalize_base_path",
    "run",
    "status_response",
]
