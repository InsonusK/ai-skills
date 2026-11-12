import asyncio
import io
from collections.abc import Coroutine
from typing import Any
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api import startup
from todo.api.core.entry import exit_code
from todo.command import PlanItemsCommand, PlanItemsHandler
from todo.command.pipeline import HandlerFunc

scenarios("../features/startup.feature")


@dataclass
class World:
    out: io.StringIO = field(default_factory=io.StringIO)
    exit_code: int | None = None
    input: list[str] = field(default_factory=list)


@pytest.fixture
def world(monkeypatch: pytest.MonkeyPatch) -> World:
    monkeypatch.delenv("STARTUP_TITLES", raising=False)
    return World()


def _start(world: World, run: Coroutine[Any, Any, None]) -> None:
    error: BaseException | None = None
    try:
        asyncio.run(run)
    except Exception as raised:
        error = raised
    world.exit_code = exit_code(error)
    print(f"when: started -> output {world.out.getvalue()!r}, exit {world.exit_code}")


@when(parsers.parse('the process starts with PlanItems for the titles "{titles}"'))
def the_process_starts_with_plan_items(world: World, titles: str) -> None:
    _start(world, startup.run(PlanItemsHandler(), PlanItemsCommand(titles=titles.split("|")), world.out))


@when(parsers.parse('the process starts with a command whose handler fails with "{message}"'))
def the_process_starts_with_a_failing_handler(world: World, message: str) -> None:
    async def fail(command: str) -> str:
        raise RuntimeError(message)

    _start(world, startup.run(HandlerFunc(fail), "command", world.out))


@then(parsers.parse("the output should be the JSON line '{line}'"))
def the_output_should_be(world: World, line: str) -> None:
    assert world.out.getvalue() == line + "\n"


@then("the output should be empty")
def the_output_should_be_empty(world: World) -> None:
    assert world.out.getvalue() == ""


@then(parsers.parse("the exit code should be {code:d}"))
def the_exit_code_should_be(world: World, code: int) -> None:
    assert world.exit_code == code


@given(parsers.parse('the environment variable "{name}" is "{value}"'))
def the_environment_variable_is(world: World, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    # world first: it clears the variables the scenario sets
    monkeypatch.setenv(name, value)


@when(parsers.parse('the startup input is read from "{name}"'))
def the_startup_input_is_read(world: World, name: str) -> None:
    world.input = startup.env_list(name)
    print(f"when: env_list({name}) -> {world.input}")


@then(parsers.parse('the startup input should be "{values}"'))
def the_startup_input_should_be(world: World, values: str) -> None:
    assert world.input == values.split("|")


@then("the startup input should be empty")
def the_startup_input_should_be_empty(world: World) -> None:
    assert world.input == []
