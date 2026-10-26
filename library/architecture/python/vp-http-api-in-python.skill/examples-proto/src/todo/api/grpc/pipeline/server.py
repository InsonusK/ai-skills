# Source: skill vp-grpc-api-in-python

"""The gRPC server: the contract's services, every call run through the call
log, the answer of every failure, and the application's middleware, server
reflection behind its switch, served by grpc.aio until it is told to stop."""

import asyncio
import inspect
import logging
from collections.abc import Callable, Sequence
from dataclasses import dataclass
from typing import Any

import grpc
from google.protobuf.message import Message
from grpc_reflection.v1alpha import reflection

from todo.api.grpc.middleware import CallNext, IncomingCall, Interceptor
from todo.api.grpc.middleware.answer_failures import AnswerFailuresMiddleware
from todo.api.grpc.middleware.call_log import CallLogMiddleware
from todo.api.grpc.pipeline.config import GrpcConfig, load_config
from todo.log import attrs

SHUTDOWN_TIMEOUT_SECONDS = 10
"""How long calls in flight may run on after the server is told to stop;
then they are cancelled."""


@dataclass(frozen=True)
class Service:
    """One service of the published contract as the server registers it."""

    name: str
    """The full name of the service, e.g. "todo.v1.ItemsService" - what
    reflection lists."""

    add_to: Callable[[grpc.aio.Server], None]
    """Registers the service's methods on a server - the generated
    add_{Service}Servicer_to_server with the service's servicer."""


class Server:
    """Serves the contract's services and, when enabled, server reflection.
    It serves no health service: the probes of every service, whatever its
    entry points, go to the same HTTP health endpoints (skill
    vp-health-in-python).
    """

    def __init__(
        self,
        config: GrpcConfig,
        services: Sequence[Service],
        logger: logging.Logger,
        interceptors: Sequence[Interceptor] = (),
    ) -> None:
        self._config = config
        self._services = services
        self._logger = logger
        self._interceptors = interceptors

    def build(self) -> grpc.aio.Server:
        """The server with the contract's services and, when enabled,
        reflection. Every unary call goes through the fixed middleware - the
        call log, then the answer of every failure - and then through the
        application's, the first the outermost, so a call a middleware
        rejects, or one that crashed, is logged and answered like any other.
        """
        chain: list[Interceptor] = [CallLogMiddleware(self._logger), AnswerFailuresMiddleware(self._logger), *self._interceptors]
        server = grpc.aio.server(
            interceptors=[_Chain(chain)],
            # grpc sets SO_REUSEPORT by default: a second process would share
            # the port silently instead of failing to start.
            options=[("grpc.so_reuseport", 0)],
        )
        for service in self._services:
            service.add_to(server)
        if self._config.reflection_enabled:
            reflection.enable_server_reflection([service.name for service in self._services] + [reflection.SERVICE_NAME], server)
        return server

    async def start(self) -> tuple[grpc.aio.Server, int]:
        """Builds the server, listens on the configured port, and starts
        serving; returns the server and the port it listens on - the one
        picked for a configured 0. Raises when the port cannot be listened on."""
        server = self.build()
        port = server.add_insecure_port(f"[::]:{self._config.listen_port}")
        await server.start()
        return server, port

    async def serve(self, stop: asyncio.Event) -> None:
        """Listens on the configured port until stop is set (SIGTERM), then
        stops accepting calls and waits up to SHUTDOWN_TIMEOUT_SECONDS for
        those in flight. Returns after a graceful stop, or raises the error
        that stopped the server - a port already taken among them."""
        _route_library_logs(self._logger)
        server, port = await self.start()
        self._logger.info("grpc server listening", extra=attrs(addr=f"[::]:{port}"))
        terminated = asyncio.create_task(server.wait_for_termination())
        stopping = asyncio.create_task(stop.wait())
        done, _ = await asyncio.wait({terminated, stopping}, return_when=asyncio.FIRST_COMPLETED)
        if terminated in done:
            stopping.cancel()
            raise RuntimeError("grpc server stopped by itself")
        await server.stop(SHUTDOWN_TIMEOUT_SECONDS)
        await terminated
        self._logger.info("grpc server stopped")


async def run(
    services: Sequence[Service], logger: logging.Logger, stop: asyncio.Event, interceptors: Sequence[Interceptor] = ()
) -> None:
    """The gRPC entry point's run: reads the settings, builds the server, and
    serves until stop is set. services are the contract's services -
    services() of the application's api/grpc package; interceptors wrap each
    of their calls, the first the outermost."""
    await Server(load_config(), services, logger, interceptors).serve(stop)


class _Chain(grpc.aio.ServerInterceptor):
    """The one grpc.aio interceptor of the server: it wraps the method of
    every unary call so the call runs through the chain of middleware, the
    first the outermost. A streaming method - reflection - and an unknown
    method, which grpc answers UNIMPLEMENTED itself, pass unchanged."""

    def __init__(self, chain: Sequence[Interceptor]) -> None:
        self._chain = chain

    async def intercept_service(
        self,
        continuation: Callable[[grpc.HandlerCallDetails], Any],
        handler_call_details: grpc.HandlerCallDetails,
    ) -> Any:
        handler = await continuation(handler_call_details)
        if handler is None or handler.unary_unary is None:
            return handler
        method: str = handler_call_details.method
        behavior = handler.unary_unary

        async def innermost(call: IncomingCall) -> Message:
            response = behavior(call.request, call.context)
            answered: Message = await response if inspect.isawaitable(response) else response
            return answered

        call_next: CallNext = innermost
        for interceptor in reversed(self._chain):
            call_next = _step(interceptor, call_next)

        async def chained(request: Message, context: grpc.aio.ServicerContext[Any, Any]) -> Message:
            return await call_next(IncomingCall(method=method, request=request, context=context))

        return grpc.unary_unary_rpc_method_handler(
            chained,
            request_deserializer=handler.request_deserializer,
            response_serializer=handler.response_serializer,
        )


def _step(interceptor: Interceptor, call_next: CallNext) -> CallNext:
    # One link of the chain: interceptor around everything after it.
    async def step(call: IncomingCall) -> Message:
        return await interceptor(call, call_next)

    return step


def _route_library_logs(logger: logging.Logger) -> None:
    # grpc logs through the standard library; its warnings and errors go to
    # the application's handler, so they are JSON lines like every other.
    library = logging.getLogger("grpc")
    library.handlers = list(logger.handlers)
    library.setLevel(logging.WARNING)
    library.propagate = False
