import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.middleware.unit_of_work import UnitOfWorkMiddleware
from todo.command.pipeline import Kind, kind_of
from todo.shared.errors import cause_of

scenarios("../features/middleware.feature")


@dataclass
class RecordingUnit:
    """A unit of work that records what the middleware asked of it."""

    asked: list[str]
    commit_error: Exception | None

    async def commit(self) -> None:
        self.asked.append("commit")
        if self.commit_error is not None:
            raise self.commit_error

    async def rollback(self) -> None:
        self.asked.append("rollback")


@dataclass
class RecordingUnits:
    begun: bool = False
    """The context already carries a unit: the command is nested."""
    commit_error: Exception | None = None
    asked: list[str] = field(default_factory=list)

    def begin(self) -> RecordingUnit | None:
        self.asked.append("begin")
        return None if self.begun else RecordingUnit(self.asked, self.commit_error)


@dataclass
class World:
    units: RecordingUnits = field(default_factory=RecordingUnits)
    failure: BaseException | None = None
    result: object = None
    error: BaseException | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the handler fails with "{message}"'))
def the_handler_fails(world: World, message: str) -> None:
    world.failure = RuntimeError(message)


@given("the handler is cancelled")
def the_handler_is_cancelled(world: World) -> None:
    world.failure = asyncio.CancelledError()


@given("a unit of work already begun by the outer command")
def a_unit_already_begun(world: World) -> None:
    world.units.begun = True


@given(parsers.parse('the commit fails with "{message}"'))
def the_commit_fails(world: World, message: str) -> None:
    world.units.commit_error = OSError(message)


@when("a command runs through the unit of work middleware")
def a_command_runs(world: World) -> None:
    async def handler(command: object) -> object:
        if world.failure is not None:
            raise world.failure
        return "the result"

    try:
        world.result = asyncio.run(UnitOfWorkMiddleware(world.units)("Import", "the command", handler))
    except BaseException as error:
        world.error = error
    print(f"when: the middleware -> {world.result!r}, {world.error!r}, asked {world.units.asked}")


@then(parsers.parse('the unit of work should have been asked for "{asked}"'))
def asked_for(world: World, asked: str) -> None:
    assert "|".join(world.units.asked) == asked


@then("the command should answer its result")
def answers_its_result(world: World) -> None:
    assert world.error is None and world.result == "the result", world.error


@then(parsers.parse('the command should fail with "{message}"'))
def fails_with(world: World, message: str) -> None:
    assert world.error is not None and str(world.error) == message, world.error


@then("the cancellation should have reached the caller")
def the_cancellation_reached(world: World) -> None:
    assert isinstance(world.error, asyncio.CancelledError), world.error


@then('the command should fail with the kind "unavailable"')
def fails_unavailable(world: World) -> None:
    assert kind_of(world.error) == Kind.UNAVAILABLE, world.error


@then(parsers.parse("the cause of the command's error should mention \"{text}\""))
def cause_mentions(world: World, text: str) -> None:
    cause = cause_of(world.error)
    assert cause is not None and text in cause, f"the cause of {world.error!r} is {cause!r}"


@then(parsers.parse("the command's message should not mention \"{text}\""))
def message_does_not_mention(world: World, text: str) -> None:
    assert text not in str(world.error), world.error
