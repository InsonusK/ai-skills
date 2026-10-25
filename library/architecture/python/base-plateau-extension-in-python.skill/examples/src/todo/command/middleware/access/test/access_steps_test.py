import asyncio
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.middleware.access import AccessMiddleware, Identity, caller
from todo.command.pipeline import HandlerFunc, Kind, KindError, Middleware, decorate

scenarios("../features/access.feature")


# The commands of these tests: one with each mark, one with none, so the
# middleware are tested without any command of the application.
@dataclass(frozen=True)
class RestrictedCommand:
    role: str

    def required_role(self) -> str:
        return self.role


@dataclass(frozen=True)
class PlainCommand:
    pass


@dataclass
class World:
    identity: Identity | None = None
    handled: bool = False
    error: KindError | None = None

    def handle(self, middleware: Middleware, command: object) -> None:
        """Runs command, named "Echo", through the middleware to a handler that
        only records that it ran - with the caller in the context when the
        scenario has one."""

        async def record(command: object) -> str:
            self.handled = True
            return "done"

        async def call() -> None:
            handler = decorate("Echo", HandlerFunc(record), [middleware])
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


@when(parsers.parse('a command restricted to "{role}" goes through the access check'))
def a_restricted_command(world: World, role: str) -> None:
    world.handle(AccessMiddleware(), RestrictedCommand(role=role))


@when("a command without a mark goes through the access check")
def an_unmarked_command_checked(world: World) -> None:
    world.handle(AccessMiddleware(), PlainCommand())


@then("the handler should have run")
def the_handler_should_have_run(world: World) -> None:
    assert world.handled and world.error is None


@then("the handler should not have run")
def the_handler_should_not_have_run(world: World) -> None:
    assert not world.handled


_KINDS = {
    "unauthenticated": Kind.UNAUTHENTICATED,
    "permission denied": Kind.PERMISSION_DENIED,
}


@then(
    parsers.re(
        r'the call should be rejected as (?P<kind>unauthenticated|permission denied) with "(?P<message>[^"]+)"'
    )
)
def the_call_should_be_rejected(world: World, kind: str, message: str) -> None:
    assert world.error is not None
    assert (world.error.kind, world.error.message) == (_KINDS[kind], message)
