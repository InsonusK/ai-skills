import logging
from typing import Protocol

from google.protobuf.message import Message
from pytest_bdd import given, parsers, scenarios

from todo.api.grpc.middleware import CallNext, IncomingCall, Interceptor
from todo.command.pipeline import Kind, KindError

scenarios("../features/interceptor.feature")


class World(Protocol):
    """What these steps use of conftest's World."""

    interceptors: list[Interceptor]

    def logger(self) -> logging.Logger: ...


@given(parsers.parse('the interceptors "{outer}" and "{inner}", each logging a record with its name'))
def the_interceptors_logging(world: World, outer: str, inner: str) -> None:
    # In this order, each writing the record "interceptor {name}" before it
    # passes the call on.
    for name in (outer, inner):

        async def log_name(call: IncomingCall, call_next: CallNext, name: str = name) -> Message:
            world.logger().info(f"interceptor {name}")
            return await call_next(call)

        world.interceptors.append(log_name)


@given(parsers.parse('an interceptor that rejects every call as unauthenticated with "{message}"'))
def a_rejecting_interceptor(world: World, message: str) -> None:
    # As an authentication interceptor rejects a call without credentials.
    async def reject(call: IncomingCall, call_next: CallNext) -> Message:
        raise KindError(Kind.UNAUTHENTICATED, message)

    world.interceptors.append(reject)
