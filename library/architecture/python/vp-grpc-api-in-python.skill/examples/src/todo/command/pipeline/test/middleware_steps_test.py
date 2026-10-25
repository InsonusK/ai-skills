import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.pipeline import HandlerFunc, Kind, KindError, Middleware, Next, decorate

scenarios("../features/middleware.feature")


@dataclass
class World:
    middlewares: list[Middleware] = field(default_factory=list)
    calls: list[str] = field(default_factory=list)
    """"{middleware} in", "handler {command}", "{middleware} out", in order"""
    seen: dict[str, str] = field(default_factory=dict)
    """what each middleware was called with"""
    result: str = ""
    error: KindError | None = None

    def recording(self, middleware: str, does: str) -> Middleware:
        """A middleware that writes "{middleware} in" and "{middleware} out" to
        the calls around call_next, and what it was called with to seen; does
        says whether it passes the command on or rejects it."""

        async def record(name: str, command: object, call_next: Next) -> object:
            self.calls.append(f"{middleware} in")
            self.seen[middleware] = f"{command} named {name}"
            if does == "rejects":
                raise KindError(Kind.PERMISSION_DENIED, f"rejected by {middleware}")
            try:
                return await call_next(command)
            finally:
                self.calls.append(f"{middleware} out")

        return record


@pytest.fixture
def world() -> World:
    return World()


@given("a handler that succeeds, wrapped by the middleware:")
def a_handler_wrapped_by(world: World, datatable: list[list[str]]) -> None:
    world.middlewares = [world.recording(middleware, does) for middleware, does in datatable[1:]]


@when(parsers.parse('the wrapped handler handles the command "{command}" as "{name}"'))
def the_wrapped_handler_handles(world: World, command: str, name: str) -> None:
    async def plan(command: str) -> str:
        world.calls.append(f"handler {command}")
        return f"planned {command}"

    try:
        world.result = asyncio.run(decorate(name, HandlerFunc(plan), world.middlewares).handle(command))
    except KindError as error:
        world.error = error
    print(f"when: handle {command!r} as {name} -> result {world.result!r}, error {world.error!r}, calls {world.calls}")


@then(parsers.parse('the calls should be "{calls}"'))
def the_calls_should_be(world: World, calls: str) -> None:
    assert ", ".join(world.calls) == calls


@then(parsers.parse('the result should be "{result}"'))
def the_result_should_be(world: World, result: str) -> None:
    assert world.result == result


@then(parsers.parse('the error should be "{message}"'))
def the_error_should_be(world: World, message: str) -> None:
    assert world.error is not None and world.error.message == message


@then(parsers.parse('the middleware "{middleware}" should have seen the command "{command}" named "{name}"'))
def the_middleware_should_have_seen(world: World, middleware: str, command: str, name: str) -> None:
    assert world.seen[middleware] == f"{command} named {name}"
