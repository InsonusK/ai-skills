# Source: skill vp-http-api-in-python

"""The HTTP server: the contract's routes, the docs, every error answered as
google.rpc.Status, every request logged, served by uvicorn until it is told
to stop."""

import asyncio
import contextlib
import logging
import socket
from collections.abc import Iterator, Sequence

import uvicorn
from fastapi import APIRouter, FastAPI

from todo.api.http.pipeline.config import HttpConfig, load_config
from todo.api.http.pipeline.docs import add_docs
from todo.api.http.pipeline.errors import AnswerFailures, add_error_answers
from todo.api.http.pipeline.logging import RequestLog
from todo.log import attrs

SHUTDOWN_TIMEOUT_SECONDS = 10
"""How long requests in flight may run on after the server is told to stop."""


class Server:
    """Serves the contract's routes - one APIRouter per resource - and, when
    enabled, the OpenAPI spec and Swagger UI. The health endpoints are not
    here: the management entry point serves them on a port of its own (skill
    vp-health-in-python).
    """

    def __init__(self, config: HttpConfig, routers: Sequence[APIRouter], logger: logging.Logger) -> None:
        self._config = config
        self._routers = routers
        self._logger = logger

    def app(self) -> FastAPI:
        """The application: the contract routes, then, when enabled, the docs
        routes outside the contract; FastAPI's own docs are off - the contract
        is the committed file, not one generated from the code."""
        app = FastAPI(openapi_url=None, docs_url=None, redoc_url=None)
        for router in self._routers:
            app.include_router(router)
        if self._config.docs_enabled:
            add_docs(app, self._config.base_path)
        add_error_answers(app)
        app.add_middleware(AnswerFailures, logger=self._logger)
        app.add_middleware(RequestLog, logger=self._logger)  # added last: the outermost
        return app

    async def serve(self, stop: asyncio.Event) -> None:
        """Listens on the configured port until stop is set (SIGTERM), then stops
        accepting requests and waits up to SHUTDOWN_TIMEOUT_SECONDS for those in
        flight. Returns after a graceful stop, or raises the error that stopped
        the server - a port already taken among them."""
        listener = _listen(self._config.listen_port)  # an OSError here is the entry point's error
        _route_library_logs(self._logger)
        server = _Server(
            uvicorn.Config(
                self.app(),
                log_config=None,
                access_log=False,
                lifespan="off",
                timeout_graceful_shutdown=SHUTDOWN_TIMEOUT_SECONDS,
            )
        )
        serving = asyncio.create_task(server.serve(sockets=[listener]))
        stopping = asyncio.create_task(stop.wait())
        self._logger.info("http server listening", extra=attrs(addr=_address(listener)))
        done, _ = await asyncio.wait({serving, stopping}, return_when=asyncio.FIRST_COMPLETED)
        if serving in done:
            stopping.cancel()
            serving.result()  # raises what stopped the server
            raise RuntimeError("http server stopped by itself")
        server.should_exit = True
        await serving
        listener.close()
        self._logger.info("http server stopped")


async def run(routers: Sequence[APIRouter], logger: logging.Logger, stop: asyncio.Event) -> None:
    """The HTTP entry point's one call from the composition root: reads the
    settings, builds the server, and serves until stop is set. routers are
    the contract's resources - routes() of the application's api/http package."""
    await Server(load_config(), routers, logger).serve(stop)


class _Server(uvicorn.Server):
    # The process's signals belong to main.py, which sets the entry points'
    # stop event; uvicorn must not take them over.
    @contextlib.contextmanager
    def capture_signals(self) -> Iterator[None]:
        yield


def _listen(port: int) -> socket.socket:
    if socket.has_dualstack_ipv6():
        return socket.create_server(("", port), family=socket.AF_INET6, dualstack_ipv6=True)
    return socket.create_server(("", port))


def _address(listener: socket.socket) -> str:
    host, port = listener.getsockname()[:2]
    return f"[{host}]:{port}" if ":" in host else f"{host}:{port}"


def _route_library_logs(logger: logging.Logger) -> None:
    # uvicorn logs through the standard library; its warnings and errors go
    # to the application's handler, so they are JSON lines like every other.
    for name in ("uvicorn", "uvicorn.error", "uvicorn.access"):
        library = logging.getLogger(name)
        library.handlers = list(logger.handlers)
        library.setLevel(logging.WARNING)
        library.propagate = False
