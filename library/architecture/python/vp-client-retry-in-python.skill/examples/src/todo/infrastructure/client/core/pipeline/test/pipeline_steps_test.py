import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.core.pipeline import Attempt, Call, Middleware, run

scenarios("../features/pipeline.feature")


@dataclass
class World:
    middlewares: list[Middleware] = field(default_factory=list)
    steps: list[str] = field(default_factory=list)


@pytest.fixture
def world() -> World:
    return World()


def _recording(world: World, name: str) -> Middleware:
    """A middleware that records its steps; "twice" awaits call_next twice."""

    async def middleware(call: Call, call_next: Attempt) -> None:
        world.steps.append(f"{name} before")
        await call_next()
        if name == "twice":
            await call_next()
        world.steps.append(f"{name} after")

    return middleware


@given(parsers.parse('the client middleware "{names}"'))
def the_client_middleware(world: World, names: str) -> None:
    world.middlewares.extend(_recording(world, name) for name in names.split("|"))


@when(parsers.parse('a call of "{operation}" is run'))
def a_call_is_run(world: World, operation: str) -> None:
    async def attempt() -> None:
        world.steps.append("attempt")

    asyncio.run(run(Call(api="test-api", operation=operation, retryable=False), world.middlewares, attempt))


@then(parsers.parse('the steps should be, in order, "{steps}"'))
def the_steps_should_be(world: World, steps: str) -> None:
    assert "|".join(world.steps) == steps
