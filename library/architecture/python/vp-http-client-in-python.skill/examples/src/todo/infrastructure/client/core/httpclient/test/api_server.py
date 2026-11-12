"""The test API of the fixed client code's scenarios: an HTTP server of these
scenarios only, on a free port of 127.0.0.1, so the fixed code is tested
without any API of the application."""

import threading
import time
from dataclasses import dataclass, field
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

from todo.infrastructure.client.core.httpclient import REQUEST_ID_HEADER


@dataclass(frozen=True)
class Received:
    """What the test API was sent."""

    method: str
    path: str
    body: str
    request_id: str | None


@dataclass
class TestApi:
    """Answers every request with the status and body a scenario sets, after
    its delay, and records what it received."""

    __test__ = False  # not a test class for pytest

    status: int = 200
    body: str = ""
    delay: float = 0.0
    received: list[Received] = field(default_factory=list)
    _server: ThreadingHTTPServer | None = None

    def start(self) -> int:
        """Serves on a free port and returns it."""
        api = self

        class Handler(BaseHTTPRequestHandler):
            def do_POST(self) -> None:
                length = int(self.headers.get("Content-Length") or 0)
                body = self.rfile.read(length).decode()
                api.received.append(Received(self.command, self.path, body, self.headers.get(REQUEST_ID_HEADER)))
                time.sleep(api.delay)
                answer = api.body.encode()
                try:
                    self.send_response(api.status)
                    self.send_header("Content-Type", "application/json")
                    self.send_header("Content-Length", str(len(answer)))
                    self.end_headers()
                    self.wfile.write(answer)
                except OSError:
                    pass  # the client gave up waiting

            def log_message(self, format: str, *args: object) -> None:
                pass

        self._server = ThreadingHTTPServer(("127.0.0.1", 0), Handler)
        threading.Thread(target=self._server.serve_forever, daemon=True).start()
        return int(self._server.server_address[1])

    def stop(self) -> None:
        if self._server is not None:
            self._server.shutdown()
            self._server.server_close()


def free_port() -> int:
    """A port nothing listens on."""
    server = ThreadingHTTPServer(("127.0.0.1", 0), BaseHTTPRequestHandler)
    port = int(server.server_address[1])
    server.server_close()
    return port
