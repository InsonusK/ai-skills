# Source: skill vp-health-in-python

"""The management server: three GET endpoints over HTTP/1.1 on asyncio's own
streams, one request per connection, stopped gracefully on the entry's
event."""

import asyncio
import json
import logging
import socket
from collections.abc import Awaitable, Callable, Sequence
from dataclasses import dataclass
from http import HTTPStatus
from typing import Any

from todo.api.core.entry import Entry
from todo.api.management.config import ManagementConfig, load_config
from todo.log import attrs

SHUTDOWN_TIMEOUT_SECONDS = 10
"""How long requests in flight may run on after the server is told to stop."""

READ_TIMEOUT_SECONDS = 10
"""How long a client may take to send its request line and headers."""

MAX_HEADER_LINES = 100
"""More header lines than this end the connection: a probe sends a few."""


@dataclass(frozen=True)
class Check:
    """One dependency the service waits for before it takes traffic. The
    management entry point has none of its own: a service variation point (a
    database, a client) adds its check in the composition root."""

    name: str
    """How a failed check is reported, e.g. "database"."""

    check: Callable[[], Awaitable[None]]
    """Returns while the dependency can be used, raises when it cannot -
    e.g. the database's ping."""


type _Answer = tuple[HTTPStatus, dict[str, Any]]


class Server:
    """Answers the health endpoints."""

    def __init__(self, config: ManagementConfig, version: str, logger: logging.Logger, checks: Sequence[Check] = ()) -> None:
        self._config = config
        self._version = version
        self._logger = logger
        self._checks = checks

    async def start(self) -> tuple[asyncio.Server, int]:
        """Listens on the configured port, on every interface; returns the
        server and the port it listens on - the one picked for a configured
        0. Raises OSError when the port is taken."""
        listener = _listen(self._config.listen_port)
        server = await asyncio.start_server(self._serve_connection, sock=listener)
        port: int = listener.getsockname()[1]
        return server, port

    async def serve(self, stop: asyncio.Event) -> None:
        """Listens until stop is set (SIGTERM), then stops accepting
        connections and waits up to SHUTDOWN_TIMEOUT_SECONDS for the requests
        in flight before it cuts them off. Returns after the stop, or raises
        the error that kept the server from listening."""
        server, port = await self.start()
        self._logger.info("management server listening", extra=attrs(addr=f":{port}"))
        await stop.wait()
        server.close()
        try:
            await asyncio.wait_for(server.wait_closed(), SHUTDOWN_TIMEOUT_SECONDS)
        except TimeoutError:
            server.abort_clients()
            await server.wait_closed()
        self._logger.info("management server stopped")

    async def answer(self, method: str, path: str) -> _Answer:
        """The status and the JSON body of one request."""
        endpoints: dict[str, Callable[[], Awaitable[_Answer]]] = {
            "/health/live": self._live,
            "/health/ready": self._ready,
            "/health": self._health,
        }
        endpoint = endpoints.get(path.split("?", 1)[0])
        if endpoint is None:
            return HTTPStatus.NOT_FOUND, {"status": "not found"}
        if method not in ("GET", "HEAD"):
            return HTTPStatus.METHOD_NOT_ALLOWED, {"status": "method not allowed"}
        return await endpoint()

    async def _live(self) -> _Answer:
        # Whether the process answers at all; it checks no dependency, so an
        # outage of a dependency never restarts the service.
        return HTTPStatus.OK, {"status": "ok"}

    async def _ready(self) -> _Answer:
        # 200 when every readiness check passes, 503 otherwise, naming the
        # checks that failed.
        failed: list[str] = []
        for check in self._checks:
            try:
                await check.check()
            except Exception as error:
                self._logger.warning("readiness check failed", exc_info=error, extra=attrs(check=check.name))
                failed.append(check.name)
        if failed:
            return HTTPStatus.SERVICE_UNAVAILABLE, {"status": "unavailable", "failed": failed}
        return HTTPStatus.OK, {"status": "ok"}

    async def _health(self) -> _Answer:
        # For people and clients, never for a probe: the status and the
        # version of the running application.
        return HTTPStatus.OK, {"status": "ok", "version": self._version}

    async def _serve_connection(self, reader: asyncio.StreamReader, writer: asyncio.StreamWriter) -> None:
        # One request: the request line and the headers, which are read and
        # dropped - a probe sends no body - then the answer, and the
        # connection is closed.
        try:
            request_line = await asyncio.wait_for(reader.readline(), READ_TIMEOUT_SECONDS)
            for _ in range(MAX_HEADER_LINES):
                if await asyncio.wait_for(reader.readline(), READ_TIMEOUT_SECONDS) in (b"\r\n", b"\n", b""):
                    break
            else:
                return
            parts = request_line.decode("latin-1").split()
            if len(parts) != 3 or not parts[2].startswith("HTTP/"):
                status, body = HTTPStatus.BAD_REQUEST, {"status": "bad request"}
            else:
                status, body = await self.answer(parts[0], parts[1])
            payload = json.dumps(body, separators=(",", ":")).encode() + b"\n"
            head = (
                f"HTTP/1.1 {status.value} {status.phrase}\r\n"
                "Content-Type: application/json\r\n"
                f"Content-Length: {len(payload)}\r\n"
                "Connection: close\r\n\r\n"
            ).encode()
            writer.write(head if parts and parts[0] == "HEAD" else head + payload)
            await writer.drain()
        except (TimeoutError, ConnectionError, ValueError):
            pass  # a client that sent nothing usable, or went away: nothing to answer
        finally:
            writer.close()


def _listen(port: int) -> socket.socket:
    # One socket for IPv6 and IPv4 where the host has both, so a port of 0
    # picks one port for both.
    if socket.has_dualstack_ipv6():
        return socket.create_server(("", port), family=socket.AF_INET6, dualstack_ipv6=True)
    return socket.create_server(("", port))


async def run(logger: logging.Logger, version: str, stop: asyncio.Event, checks: Sequence[Check] = ()) -> None:
    """The management entry point's run: reads the settings, builds the
    server, and serves until stop is set. checks are the readiness checks the
    service variation points add."""
    await Server(load_config(), version, logger, checks).serve(stop)


def entry(logger: logging.Logger, version: str, checks: Sequence[Check] = ()) -> Entry:
    """The management entry point as the composition root lists it for
    run_all: run under the name "management". version is the application's,
    reported by /health; checks are the readiness checks the service
    variation points add."""
    return Entry("management", lambda stop: run(logger, version, stop, checks))
