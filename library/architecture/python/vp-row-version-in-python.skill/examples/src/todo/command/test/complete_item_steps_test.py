import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command import CompleteItemCommand, CompleteItemHandler, CompleteItemResult
from todo.command.pipeline import Kind, kind_of
from todo.command.test.item_lists_steps_test import FakeItemLists
from todo.domain.entity import Item, KeptItem

scenarios("../features/complete_item.feature")


@dataclass
class World:
    lists: FakeItemLists = field(default_factory=FakeItemLists)
    completed: CompleteItemResult | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the item lists hold the item {item_id:d} "{title}" at the version "{version}"'))
def the_item_lists_hold_the_item(world: World, item_id: int, title: str, version: str) -> None:
    world.lists.held.setdefault("home", []).append(KeptItem(id=item_id, version=version, item=Item(title)))


@given(parsers.parse("the item {item_id:d} is done already"))
def the_item_is_done_already(world: World, item_id: int) -> None:
    next(kept for kept in world.lists.held["home"] if kept.id == item_id).item.complete()


@given(parsers.parse('the item lists answer the kind "{kind}" to a save'))
def the_item_lists_answer_to_a_save(world: World, kind: str) -> None:
    world.lists.save_kind = Kind(kind)


@when(parsers.re(r'I send CompleteItem with the list "(?P<list_id>[^"]*)", the item (?P<item_id>\d+), and the version "(?P<version>[^"]*)"'))
def i_send_complete_item(world: World, list_id: str, item_id: str, version: str) -> None:
    command = CompleteItemCommand(list_id=list_id, item_id=int(item_id), version=version)
    try:
        world.completed = asyncio.run(CompleteItemHandler(world.lists).handle(command))
    except Exception as error:
        world.error = error
    print(f"when: CompleteItem {command} -> {world.completed}, error {world.error!r}")


@then(parsers.parse('the item lists should have saved the item {item_id:d} done, under the version "{version}"'))
def the_item_lists_should_have_saved(world: World, item_id: int, version: str) -> None:
    saved = world.lists.saved
    assert saved is not None and (saved.id, saved.item.done, saved.version) == (item_id, True, version), saved


@then(parsers.parse('the completed item should be done, with the version "{version}"'))
def the_completed_item_should_be_done(world: World, version: str) -> None:
    assert world.completed is not None, world.error
    assert (world.completed.item.done, world.completed.item.version) == (True, version)


@then("the item lists should not have saved")
def the_item_lists_should_not_have_saved(world: World) -> None:
    assert world.lists.saved is None


@then("the item lists should not have been asked")
def the_item_lists_should_not_have_been_asked(world: World) -> None:
    assert not world.lists.asked


@then(parsers.parse('the command should fail with the kind "{kind}"'))
def the_command_should_fail_with_the_kind(world: World, kind: str) -> None:
    assert kind_of(world.error) == kind, world.error
