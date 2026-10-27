import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command import PlanRemoteItemsCommand, PlanRemoteItemsHandler, PlanRemoteItemsResult
from todo.command.pipeline import Kind, KindError, kind_of
from todo.domain.entity import Item

scenarios("../features/plan_remote_items.feature")


@dataclass
class FakePlanner:
    """The remote planner faked: the titles it plans or the error it raises, and the titles it was given."""

    titles: list[str] = field(default_factory=list)
    error: Exception | None = None
    given: list[str] | None = None

    async def plan(self, titles: list[str]) -> list[Item]:
        self.given = titles
        if self.error is not None:
            raise self.error
        return [Item(title) for title in self.titles]


@dataclass
class World:
    planner: FakePlanner = field(default_factory=FakePlanner)
    result: PlanRemoteItemsResult | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the remote planner plans "{titles}"'))
def the_remote_planner_plans(world: World, titles: str) -> None:
    world.planner.titles = titles.split("|")


@given(parsers.parse('the remote planner fails with the kind "{kind}"'))
def the_remote_planner_fails(world: World, kind: str) -> None:
    world.planner.error = KindError(Kind(kind), "answered by the scenario")


@when(parsers.parse('I send PlanRemoteItems with the titles "{titles}"'))
def i_send_plan_remote_items(world: World, titles: str) -> None:
    try:
        world.result = asyncio.run(PlanRemoteItemsHandler(world.planner).handle(PlanRemoteItemsCommand(titles=titles.split("|"))))
    except Exception as error:
        world.error = error
    print(f"when: PlanRemoteItems {titles} -> {world.result}, error {world.error!r}")


@then(parsers.parse('the remote result should list the open items "{titles}"'))
def the_remote_result_should_list(world: World, titles: str) -> None:
    assert world.result is not None, world.error
    assert [(view.title, view.done) for view in world.result.items] == [(title, False) for title in titles.split("|")]


@then(parsers.parse('the remote planner should have been given "{titles}"'))
def the_remote_planner_was_given(world: World, titles: str) -> None:
    assert world.planner.given == titles.split("|")


@then(parsers.parse('the command should fail with the kind "{kind}"'))
def the_command_should_fail_with_the_kind(world: World, kind: str) -> None:
    assert kind_of(world.error) is Kind(kind), world.error
