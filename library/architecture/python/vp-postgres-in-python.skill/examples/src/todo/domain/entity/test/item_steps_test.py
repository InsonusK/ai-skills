from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.domain.entity import AlreadyDoneError, EmptyTitleError, Item, KindError, TitleTooLongError

scenarios("../features/item.feature")

# The outcome names of the scenarios, by the error of the rule.
_OUTCOMES: dict[type[KindError], str] = {EmptyTitleError: "empty title", TitleTooLongError: "title too long"}


@dataclass
class World:
    item: Item | None = None
    outcome: str = ""
    error: KindError | None = None


@pytest.fixture
def world() -> World:
    return World()


def _create(world: World, title: str) -> None:
    try:
        world.item, world.outcome = Item(title), "created"
    except KindError as error:
        world.item, world.outcome = None, _OUTCOMES[type(error)]
    print(f"when: Item({title!r}) -> {world.outcome}")


@when(parsers.re(r'I create an item titled "(?P<title>[^"]*)"'))
def i_create_an_item(world: World, title: str) -> None:
    _create(world, title)


@when(parsers.parse("I create an item with a title of {length:d} characters"))
def i_create_an_item_of_length(world: World, length: int) -> None:
    _create(world, "a" * length)


@then(parsers.parse('the item outcome should be "{outcome}"'))
def the_item_outcome_should_be(world: World, outcome: str) -> None:
    assert world.outcome == outcome


@then(parsers.re(r'the item title should be "(?P<title>[^"]*)"'))
def the_item_title_should_be(world: World, title: str) -> None:
    assert (world.item.title if world.item else "") == title


@given(parsers.parse('an open item titled "{title}"'))
def an_open_item(world: World, title: str) -> None:
    world.item = Item(title)


@given("the item is completed")
def the_item_is_completed(world: World) -> None:
    assert world.item is not None
    world.item.complete()


@when("I complete the item")
def i_complete_the_item(world: World) -> None:
    assert world.item is not None
    try:
        world.item.complete()
    except KindError as error:
        world.error = error
    print(f"when: complete -> done {world.item.done}, error {world.error}")


@then("the item should be done")
def the_item_should_be_done(world: World) -> None:
    assert world.item is not None and world.item.done and world.error is None


@then(parsers.parse('completing should fail with "{reason}"'))
def completing_should_fail(world: World, reason: str) -> None:
    assert isinstance(world.error, AlreadyDoneError) and reason in str(world.error)
