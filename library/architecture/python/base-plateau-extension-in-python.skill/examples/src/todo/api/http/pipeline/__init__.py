# Source: skill vp-http-api-in-python

"""The fixed part of the HTTP entry point, copied from templates and never
edited: settings (config.py), the server (server.py), the exception handlers
answering the errors the framework knows of as google.rpc.Status (errors.py),
and the docs routes (docs.py). The middleware live one package each in
todo.api.http.middleware. Like every entry point it knows
only the command layer - never domain, service, or infrastructure."""

from todo.api.http.pipeline.config import HttpConfig, load_config, normalize_base_path
from todo.api.http.pipeline.server import CallNext, Middleware, Server, run

__all__ = [
    "CallNext",
    "HttpConfig",
    "Middleware",
    "Server",
    "load_config",
    "normalize_base_path",
    "run",
]
