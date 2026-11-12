"""test.v1.EchoService: the service of these tests only, so the fixed code is
tested without any service or command of the application - its fake command,
the service built over it, and a client call of its one method."""

import asyncio
from dataclasses import dataclass, field
from typing import Any

import grpc
from google.protobuf.wrappers_pb2 import StringValue

from todo.api.grpc.pipeline import Service, call
from todo.command.pipeline import Handler
from todo.log import request_id

ECHO_SERVICE = "test.v1.EchoService"
ECHO_METHOD = f"/{ECHO_SERVICE}/Echo"


@dataclass
class EchoCommand:
    """The fake command of the echo method - a command of these tests only,
    so the fixed code is tested without any command of the application. It
    answers or raises what the scenario set, and records what it received."""

    answer: str = ""
    error: BaseException | None = None
    delay: float = 0
    received: list[str] = field(default_factory=list)
    request_ids: list[str | None] = field(default_factory=list)

    async def handle(self, command: str) -> str:
        self.received.append(command)
        self.request_ids.append(request_id())
        await asyncio.sleep(self.delay)
        if self.error is not None:
            raise self.error
        return self.answer


def echo_service(handler: Handler[str, str]) -> Service:
    """test.v1.EchoService with its one method Echo, built the way a service
    module of endpoint/ builds a generated one: the method calls call() with
    its decode and encode. Its messages are google.protobuf.StringValue, so
    the tests need no proto file of their own."""

    def decode(request: StringValue) -> str:
        if not request.value:
            raise ValueError("text is required")
        return request.value

    async def echo(request: StringValue, context: grpc.aio.ServicerContext[Any, Any]) -> StringValue:
        return await call(handler, request, decode, lambda text: StringValue(value=text))

    method = grpc.unary_unary_rpc_method_handler(
        echo, request_deserializer=StringValue.FromString, response_serializer=StringValue.SerializeToString
    )
    generic = grpc.method_handlers_generic_handler(ECHO_SERVICE, {"Echo": method})
    return Service(name=ECHO_SERVICE, add_to=lambda server: server.add_generic_rpc_handlers((generic,)))


async def call_echo(port: int, text: str, metadata: tuple[tuple[str, str], ...] = ()) -> tuple[str, grpc.aio.Metadata | None]:
    """Calls Echo over TCP; returns the reply and the initial metadata, or
    raises the call's grpc.aio.AioRpcError."""
    async with grpc.aio.insecure_channel(f"127.0.0.1:{port}") as channel:
        echo = channel.unary_unary(ECHO_METHOD, request_serializer=StringValue.SerializeToString, response_deserializer=StringValue.FromString)
        answer = echo(StringValue(value=text), metadata=metadata)
        reply: StringValue = await answer
        return reply.value, await answer.initial_metadata()
