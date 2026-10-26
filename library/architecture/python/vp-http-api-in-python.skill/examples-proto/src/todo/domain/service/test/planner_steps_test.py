from dataclasses import dataclass, field

import pytest
from pytest_bdd import parsers, scenarios, then, when

from todo.domain.entity import DuplicateTitleError, EmptyTitleError, Item, KindError, NoItemsError
from todo.domain.service import Planner

scenarios("../features/planner.feature")

_REASONS: dict[type[KindError], str] = {
    DuplicateTitleError: "duplicate title",
    EmptyTitleError: "empty title",
    NoItemsError: "no items",
}


@dataclass
class World:
    items: list[Item] = field(default_factory=list)
    error: KindError | None = None


@pytest.fixture
def world() -> World:
    return World()


def _plan(world: World, titles: list[str]) -> None:
    try:
        world.items = Planner().plan(titles)
    except KindError as error:
        world.error = error
    print(f"when: plan {titles} -> {[item.title for item in world.items]}, error {world.error!r}")


@when(parsers.parse('I plan the titles "{titles}"'))
def i_plan_the_titles(world: World, titles: str) -> None:
    _plan(world, titles.split("|"))


@when("I plan no titles")
def i_plan_no_titles(world: World) -> None:
    _plan(world, [])


@then(parsers.parse('planning should succeed with the titles "{titles}"'))
def planning_should_succeed(world: World, titles: str) -> None:
    assert world.error is None
    assert [item.title for item in world.items] == titles.split("|")
    assert not any(item.done for item in world.items)


@then(parsers.parse('planning should fail with "{reason}"'))
def planning_should_fail(world: World, reason: str) -> None:
    assert world.error is not None and _REASONS[type(world.error)] == reason
