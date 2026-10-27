# Source: skill vp-grpc-client-in-python

"""GrpcClient: the channel to one gRPC API, and the interceptor of its calls."""

import logging
import time
from collections.abc import Callable
from datetime import timedelta
from typing import Any

import grpc

from {package}.infrastructure.client.core.grpcclient.config import GrpcConfig
from {package}.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from {package}.log import attrs, request_id

REQUEST_ID_METADATA = "x-request-id"
"""Carries the request id of the call being served to the API."""

Emulator = Callable[[grpc.aio.Server], None]
"""The emulator of an API: adds its services, answered inside the process, to
a gRPC server - the mock of one adapter (infrastructure/client/{api}/mock)."""


class GrpcClient:
    """The channel to one gRPC API. The adapter builds the generated stub of
    each service over channel (todo_pb2_grpc.ItemsServiceStub(client.channel));
    every unary call it makes passes _CallInterceptor."""

    def __init__(self, channel: grpc.aio.Channel, emulator: grpc.aio.Server | None) -> None:
        self.channel = channel
        self._emulator = emulator

    @classmethod
    async def open(cls, config: GrpcConfig, logger: logging.Logger, emulator: Emulator) -> "GrpcClient":
        """Builds the channel to the API config names. It does not connect:
        the first call does, so a start does not fail while the API is down.
        Mocked, every call is answered by the services emulator adds to a gRPC
        server inside the process, on a port of the loopback only - the same
        channel, interceptor, log, and outcomes - and a WARN record says so."""
        interceptors = [_CallInterceptor(config, logger)]
        if config.mocked:
            server = grpc.aio.server()
            emulator(server)
            port = server.add_insecure_port("127.0.0.1:0")
            await server.start()
            logger.warning("api client mocked", extra=attrs(api=config.name))
            return cls(grpc.aio.insecure_channel(f"127.0.0.1:{port}", interceptors=interceptors), server)
        if config.tls:
            return cls(grpc.aio.secure_channel(config.target, grpc.ssl_channel_credentials(), interceptors=interceptors), None)
        return cls(grpc.aio.insecure_channel(config.target, interceptors=interceptors), None)

    async def aclose(self) -> None:
        """Closes the channel and stops the emulator of a mocked client."""
        await self.channel.close()
        if self._emulator is not None:
            await self._emulator.stop(None)


class _CallInterceptor(grpc.aio.UnaryUnaryClientInterceptor):
    """Runs around every unary call of the channel. It ends the call after
    the timeout at the latest, sends the request id of the call being served,
    and writes two records: "grpc client call sent" with the request message
    as the payload, and "grpc client call answered" with the status code, the
    duration, failed, and as the payload the response message or, for a failed
    call, the status received and the request again. A failed call raises
    ApiUnavailableError for UNAVAILABLE and DEADLINE_EXCEEDED, ApiStatusError
    for any other status. Streaming calls are not intercepted: the client
    variation point covers unary calls only."""

    def __init__(self, config: GrpcConfig, logger: logging.Logger) -> None:
        self._config = config
        self._logger = logger

    async def intercept_unary_unary(self, continuation: Callable[..., Any], details: grpc.aio.ClientCallDetails, request: Any) -> Any:
        method = details.method.decode() if isinstance(details.method, bytes) else str(details.method)
        timeout = self._config.timeout.total_seconds()
        if details.timeout is not None:
            timeout = min(timeout, details.timeout)  # the earlier of the caller's deadline and the timeout
        metadata = grpc.aio.Metadata(*(details.metadata or ()))
        if id := request_id():
            metadata.add(REQUEST_ID_METADATA, id)
        details = grpc.aio.ClientCallDetails(
            method=details.method,
            timeout=timeout,
            metadata=metadata,
            credentials=details.credentials,
            wait_for_ready=details.wait_for_ready,
        )

        self._logger.info("grpc client call sent", extra=attrs(api=self._config.name, method=method, payload={"request": request}))
        started = time.monotonic()
        code = grpc.StatusCode.UNKNOWN
        received: dict[str, Any] = {"request": request}
        try:
            call = await continuation(details, request)
            response = await call
            code, received = grpc.StatusCode.OK, {"response": response}
            return response
        except grpc.aio.AioRpcError as error:
            code = error.code()
            received = {"status": {"code": code.name, "message": error.details() or ""}, "request": request}
            if code in (grpc.StatusCode.UNAVAILABLE, grpc.StatusCode.DEADLINE_EXCEEDED):
                raise ApiUnavailableError(self._config.name, f"{method}: {code.name}: {error.details()}") from error
            raise ApiStatusError(self._config.name, StatusCode(code.value[0]), error.details() or "") from error
        finally:
            # A call that is no status of the API (a message the client
            # cannot write) is logged as UNKNOWN and passed on as it is.
            self._logger.info(
                "grpc client call answered",
                extra=attrs(
                    api=self._config.name,
                    method=method,
                    code=code.name,
                    duration=timedelta(seconds=time.monotonic() - started),
                    failed=code is not grpc.StatusCode.OK,
                    payload=received,
                ),
            )
