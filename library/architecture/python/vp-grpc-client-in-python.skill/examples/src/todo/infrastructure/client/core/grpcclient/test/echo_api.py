"""The test API of the fixed client code's scenarios: EchoService, an API of
these scenarios only, so the fixed code is tested without any API of the
application. Its one method takes and answers a google.protobuf.StringValue,
so it needs no generated code."""

import asyncio
from dataclasses import dataclass, field

import grpc
from google.protobuf.wrappers_pb2 import StringValue

from todo.infrastructure.client.core.grpcclient import REQUEST_ID_METADATA

ECHO_METHOD = "/test.v1.EchoService/Echo"


@dataclass(frozen=True)
class Received:
    """What the test API was sent."""

    text: str
    request_id: str | None


@dataclass
class EchoBehavior:
    """EchoService as a scenario sets it: it answers its text or fails with
    its status, after its delay, and records what it received."""

    text: str = ""
    failure: tuple[grpc.StatusCode, str] | None = None
    delay: float = 0.0
    received: list[Received] = field(default_factory=list)

    async def echo(self, request: StringValue, context: grpc.aio.ServicerContext) -> StringValue:  # type: ignore[type-arg]
        ids = [value for key, value in context.invocation_metadata() or () if key == REQUEST_ID_METADATA]
        self.received.append(Received(request.value, str(ids[0]) if ids else None))
        await asyncio.sleep(self.delay)
        if self.failure is not None:
            await context.abort(*self.failure)
        return StringValue(value=self.text)

    def add_to(self, server: grpc.aio.Server) -> None:
        """Adds EchoService to server, described by hand as generated code would."""
        handler = grpc.unary_unary_rpc_method_handler(
            self.echo, request_deserializer=StringValue.FromString, response_serializer=StringValue.SerializeToString
        )
        server.add_generic_rpc_handlers((grpc.method_handlers_generic_handler("test.v1.EchoService", {"Echo": handler}),))


async def serve(behavior: EchoBehavior) -> tuple[grpc.aio.Server, int]:
    """A gRPC server of the test API on a free port of 127.0.0.1."""
    server = grpc.aio.server()
    behavior.add_to(server)
    port = server.add_insecure_port("127.0.0.1:0")
    await server.start()
    return server, port
