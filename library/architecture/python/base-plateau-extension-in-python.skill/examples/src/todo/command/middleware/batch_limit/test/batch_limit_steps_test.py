import asyncio
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.middleware.batch_limit import BatchLimitMiddleware
from todo.command.pipeline import HandlerFunc, Kind, KindError, decorate

scenarios("../features/batch_limit.feature")


# The commands of these tests: one marked as a batch, one with no mark, so the
# middleware is tested without any command of the application.
@dataclass(frozen=True)
class BatchCommand:
    size: int

    def batch_size(self) -> int:
        return self.size


@dataclass(frozen=True)
class PlainCommand:
    pass


@dataclass
class World:
    limit: int = 0
    handled: bool = False
    error: KindError | None = None

    def handle(self, command: object) -> None:
        """Runs command, named "Echo", through the batch limit to a handler
        that only records that it ran."""

        async def record(command: object) -> str:
            self.handled = True
            return "done"

        try:
            asyncio.run(decorate("Echo", HandlerFunc(record), [BatchLimitMiddleware(self.limit)]).handle(command))
        except KindError as error:
            self.error = error
        print(f"when: {command!r} -> handled {self.handled}, error {self.error!r}")


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse("the batch limit {limit:d}"))
def the_batch_limit(world: World, limit: int) -> None:
    world.limit = limit


@when(parsers.parse("a batch of {size:d} items goes through the batch limit"))
def a_batch(world: World, size: int) -> None:
    world.handle(BatchCommand(size=size))


@when("a command without a mark goes through the batch limit")
def an_unmarked_command(world: World) -> None:
    world.handle(PlainCommand())


@then("the handler should have run")
def the_handler_should_have_run(world: World) -> None:
    assert world.handled and world.error is None


@then("the handler should not have run")
def the_handler_should_not_have_run(world: World) -> None:
    assert not world.handled


@then(parsers.parse('the call should be rejected as resource exhausted with "{message}"'))
def the_call_should_be_rejected(world: World, message: str) -> None:
    assert world.error is not None
    assert (world.error.kind, world.error.message) == (Kind.RESOURCE_EXHAUSTED, message)
