import logging
from typing import Protocol

from fastapi import Request, Response
from pytest_bdd import given, parsers, scenarios

from todo.api.http.pipeline import CallNext, Middleware
from todo.command.pipeline import Kind, KindError

scenarios("../features/middleware.feature")


class World(Protocol):
    """What these steps use of conftest's World."""

    middlewares: list[Middleware]

    def logger(self) -> logging.Logger: ...


@given(parsers.parse('the middlewares "{outer}" and "{inner}", each logging a record with its name'))
def the_middlewares_logging(world: World, outer: str, inner: str) -> None:
    # In this order, each writing the record "middleware {name}" before it
    # passes the request on.
    for name in (outer, inner):

        async def log_name(request: Request, call_next: CallNext, name: str = name) -> Response:
            world.logger().info(f"middleware {name}")
            return await call_next(request)

        world.middlewares.append(log_name)


@given(parsers.parse('a middleware that rejects every request as unauthenticated with "{message}"'))
def a_rejecting_middleware(world: World, message: str) -> None:
    # As an authentication middleware rejects a request without credentials.
    async def reject(request: Request, call_next: CallNext) -> Response:
        raise KindError(Kind.UNAUTHENTICATED, message)

    world.middlewares.append(reject)
