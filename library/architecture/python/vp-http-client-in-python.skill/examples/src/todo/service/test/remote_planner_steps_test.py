import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.domain.entity import Item, Kind, kind_of
from todo.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from todo.infrastructure.client.todo_api import Item as AnsweredItem
from todo.service import RemotePlanner

scenarios("../features/remote_planner.feature")


@dataclass
class FakeTodoApi:
    """The adapter faked: the items it answers or the technical outcome it raises."""

    items: list[AnsweredItem] = field(default_factory=list)
    error: Exception | None = None

    async def plan_items(self, titles: list[str]) -> list[AnsweredItem]:
        if self.error is not None:
            raise self.error
        return self.items


@dataclass
class World:
    api: FakeTodoApi = field(default_factory=FakeTodoApi)
    items: list[Item] | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the todo API plans "{titles}"'))
def the_todo_api_plans(world: World, titles: str) -> None:
    world.api.items = [AnsweredItem(title=title, done=False) for title in titles.split("|")]


@given('the todo API plans ""')
def the_todo_api_plans_an_empty_title(world: World) -> None:
    world.api.items = [AnsweredItem(title="", done=False)]


@given("the todo API is unavailable")
def the_todo_api_is_unavailable(world: World) -> None:
    world.api.error = ApiUnavailableError("todo-api", "connection refused")


@given(parsers.parse('the todo API answers {status:d} with the code {code:d} and the message "{message}"'))
def the_todo_api_answers(world: World, status: int, code: int, message: str) -> None:
    world.api.error = ApiStatusError("todo-api", StatusCode(code), message, status)


@when(parsers.parse('I plan "{titles}" remotely'))
def i_plan_remotely(world: World, titles: str) -> None:
    try:
        world.items = asyncio.run(RemotePlanner(world.api).plan(titles.split("|")))
    except Exception as error:
        world.error = error
    print(f"when: plan {titles} -> {world.items}, error {world.error!r}")


@then(parsers.parse('the planned items should be the open items "{titles}"'))
def the_planned_items_should_be(world: World, titles: str) -> None:
    assert world.items is not None, world.error
    assert [(item.title, item.done) for item in world.items] == [(title, False) for title in titles.split("|")]


@then(parsers.parse('planning should fail with the kind "{kind}"'))
def planning_should_fail_with_the_kind(world: World, kind: str) -> None:
    assert kind_of(world.error) is Kind(kind), world.error


@then(parsers.parse('planning should fail with the kind "{kind}" mentioning "{text}"'))
def planning_should_fail_mentioning(world: World, kind: str, text: str) -> None:
    assert kind_of(world.error) is Kind(kind) and text in str(world.error), world.error


@then("planning should fail with no kind")
def planning_should_fail_with_no_kind(world: World) -> None:
    assert world.error is not None and kind_of(world.error) is None, world.error
