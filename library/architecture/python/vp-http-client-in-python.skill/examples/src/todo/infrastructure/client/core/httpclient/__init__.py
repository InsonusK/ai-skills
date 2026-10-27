# Source: skill vp-http-client-in-python

"""The fixed part of every client of another service's HTTP API: the {API}
settings (config.py), and HttpClient.call, which sends one request and reads
its answer with the timeout, the call log, the request id, the outcomes, and
the mock (client.py). An adapter of one API (infrastructure/client/{api})
writes its operations as calls of it."""

from todo.infrastructure.client.core.httpclient.client import REQUEST_ID_HEADER, Emulator, HttpClient
from todo.infrastructure.client.core.httpclient.config import DEFAULT_TIMEOUT, MOCK, HttpConfig, load_config

__all__ = ["DEFAULT_TIMEOUT", "MOCK", "REQUEST_ID_HEADER", "Emulator", "HttpClient", "HttpConfig", "load_config"]
