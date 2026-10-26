import asyncio
import io
import logging
import shlex
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.cli import Endpoints, run
from todo.api.cli.endpoint import ItemsHandler
from todo.command import ItemView, PlanItemsCommand, PlanItemsResult

scenarios("../features/items.feature")


@dataclass
class FakePlanItems:
    """PlanItems faked: what it returns, and the commands it received."""

    titles: list[str] = field(default_factory=list)
    received: list[PlanItemsCommand] = field(default_factory=list)

    async def handle(self, command: PlanItemsCommand) -> PlanItemsResult:
        self.received.append(command)
        return PlanItemsResult(items=[ItemView(title=title, done=False) for title in self.titles])


@dataclass
class World:
    plan_items: FakePlanItems = field(default_factory=FakePlanItems)
    stdout: str = ""


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('PlanItems returns the items "{titles}"'))
def plan_items_returns(world: World, titles: str) -> None:
    world.plan_items.titles = titles.split("|")


@when(parsers.parse('I run "{args}"'))
def i_run(world: World, args: str) -> None:
    stdout, stderr = io.StringIO(), io.StringIO()
    endpoints = Endpoints(items=ItemsHandler(plan_items=world.plan_items))
    asyncio.run(run(logging.Logger("test"), "todo", "1.2.3", shlex.split(args), endpoints, stdout, stderr))
    world.stdout = stdout.getvalue()
    print(f"when: todo {args} -> {world.stdout!r}, stderr {stderr.getvalue()!r}")


@then(parsers.re(r"stdout should be the line '(?P<line>[^']*)'"))
def stdout_should_be_the_line(world: World, line: str) -> None:
    assert world.stdout == line + "\n"


@then(parsers.parse('stdout should be the lines "{lines}"'))
def stdout_should_be_the_lines(world: World, lines: str) -> None:
    assert world.stdout == "\n".join(lines.split("|")) + "\n"


@then(parsers.parse('PlanItems should have received the titles "{titles}"'))
def plan_items_should_have_received(world: World, titles: str) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=titles.split("|"))]
