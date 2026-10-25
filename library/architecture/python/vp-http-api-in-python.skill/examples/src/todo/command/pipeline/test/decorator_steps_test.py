import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.pipeline import Decorator, HandlerFunc, Kind, KindError, Next, decorate

scenarios("../features/decorator.feature")


@dataclass
class World:
    decorators: list[Decorator] = field(default_factory=list)
    calls: list[str] = field(default_factory=list)
    """"{decorator} in", "handler {command}", "{decorator} out", in order"""
    seen: dict[str, str] = field(default_factory=dict)
    """what each decorator was called with"""
    result: str = ""
    error: KindError | None = None

    def recording(self, decorator: str, does: str) -> Decorator:
        """A decorator that writes "{decorator} in" and "{decorator} out" to
        the calls around call_next, and what it was called with to seen; does
        says whether it passes the command on or rejects it."""

        async def record(name: str, command: object, call_next: Next) -> object:
            self.calls.append(f"{decorator} in")
            self.seen[decorator] = f"{command} named {name}"
            if does == "rejects":
                raise KindError(Kind.PERMISSION_DENIED, f"rejected by {decorator}")
            try:
                return await call_next(command)
            finally:
                self.calls.append(f"{decorator} out")

        return record


@pytest.fixture
def world() -> World:
    return World()


@given("a handler that succeeds, decorated by:")
def a_handler_decorated_by(world: World, datatable: list[list[str]]) -> None:
    world.decorators = [world.recording(decorator, does) for decorator, does in datatable[1:]]


@when(parsers.parse('the decorated handler handles the command "{command}" as "{name}"'))
def the_decorated_handler_handles(world: World, command: str, name: str) -> None:
    async def plan(command: str) -> str:
        world.calls.append(f"handler {command}")
        return f"planned {command}"

    try:
        world.result = asyncio.run(decorate(name, HandlerFunc(plan), world.decorators).handle(command))
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


@then(parsers.parse('the decorator "{decorator}" should have seen the command "{command}" named "{name}"'))
def the_decorator_should_have_seen(world: World, decorator: str, command: str, name: str) -> None:
    assert world.seen[decorator] == f"{command} named {name}"
