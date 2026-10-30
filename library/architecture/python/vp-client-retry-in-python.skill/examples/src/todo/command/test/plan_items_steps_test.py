import asyncio
from dataclasses import dataclass

import pytest
from pytest_bdd import parsers, scenarios, then, when

from todo.command import PlanItemsCommand, PlanItemsHandler, PlanItemsResult
from todo.command.pipeline import Kind, kind_of

scenarios("../features/plan_items.feature")


@dataclass
class World:
    result: PlanItemsResult | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@when(parsers.parse('I send PlanItems with the titles "{titles}"'))
def i_send_plan_items(world: World, titles: str) -> None:
    try:
        world.result = asyncio.run(PlanItemsHandler().handle(PlanItemsCommand(titles=titles.split("|"))))
    except Exception as error:
        world.error = error
    print(f"when: PlanItems {titles} -> {world.result}, error {world.error!r}")


@then(parsers.parse('the result should list the open items "{titles}"'))
def the_result_should_list(world: World, titles: str) -> None:
    assert world.result is not None, world.error
    assert [(view.title, view.done) for view in world.result.items] == [(title, False) for title in titles.split("|")]


@then("the command should be rejected as invalid")
def the_command_should_be_rejected(world: World) -> None:
    assert kind_of(world.error) is Kind.INVALID, world.error
