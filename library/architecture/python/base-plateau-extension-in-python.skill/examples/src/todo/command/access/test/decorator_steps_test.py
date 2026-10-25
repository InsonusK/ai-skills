import asyncio
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.access import Identity, caller, with_access_check, with_batch_limit
from todo.command.pipeline import Decorator, HandlerFunc, Kind, KindError, decorate

scenarios("../features/access_check.feature", "../features/batch_limit.feature")


# The commands of these tests: one with each mark, one with none, so the
# decorators are tested without any command of the application.
@dataclass(frozen=True)
class RestrictedCommand:
    role: str

    def required_role(self) -> str:
        return self.role


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
    identity: Identity | None = None
    limit: int = 0
    handled: bool = False
    error: KindError | None = None

    def handle(self, decorator: Decorator, command: object) -> None:
        """Runs command, named "Echo", through the decorator to a handler that
        only records that it ran - with the caller in the context when the
        scenario has one."""

        async def record(command: object) -> str:
            self.handled = True
            return "done"

        async def call() -> None:
            handler = decorate("Echo", HandlerFunc(record), [decorator])
            if self.identity is None:
                await handler.handle(command)
                return
            with caller(self.identity):
                await handler.handle(command)

        try:
            asyncio.run(call())
        except KindError as error:
            self.error = error
        print(f"when: {command!r} -> handled {self.handled}, error {self.error!r}")


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.re(r'the caller "(?P<user>[^"]+)" with the roles "(?P<roles>[^"]*)"'))
def the_caller(world: World, user: str, roles: str) -> None:
    world.identity = Identity(user=user, roles=tuple(roles.split(",")) if roles else ())


@given("no caller")
def no_caller(world: World) -> None:
    world.identity = None


@given(parsers.parse("the batch limit {limit:d}"))
def the_batch_limit(world: World, limit: int) -> None:
    world.limit = limit


@when(parsers.parse('a command restricted to "{role}" goes through the access check'))
def a_restricted_command(world: World, role: str) -> None:
    world.handle(with_access_check(), RestrictedCommand(role=role))


@when("a command without a mark goes through the access check")
def an_unmarked_command_checked(world: World) -> None:
    world.handle(with_access_check(), PlainCommand())


@when(parsers.parse("a batch of {size:d} items goes through the batch limit"))
def a_batch(world: World, size: int) -> None:
    world.handle(with_batch_limit(world.limit), BatchCommand(size=size))


@when("a command without a mark goes through the batch limit")
def an_unmarked_command_limited(world: World) -> None:
    world.handle(with_batch_limit(world.limit), PlainCommand())


@then("the handler should have run")
def the_handler_should_have_run(world: World) -> None:
    assert world.handled and world.error is None


@then("the handler should not have run")
def the_handler_should_not_have_run(world: World) -> None:
    assert not world.handled


_KINDS = {
    "unauthenticated": Kind.UNAUTHENTICATED,
    "permission denied": Kind.PERMISSION_DENIED,
    "resource exhausted": Kind.RESOURCE_EXHAUSTED,
}


@then(
    parsers.re(
        r'the call should be rejected as (?P<kind>unauthenticated|permission denied|resource exhausted) with "(?P<message>[^"]+)"'
    )
)
def the_call_should_be_rejected(world: World, kind: str, message: str) -> None:
    assert world.error is not None
    assert (world.error.kind, world.error.message) == (_KINDS[kind], message)
