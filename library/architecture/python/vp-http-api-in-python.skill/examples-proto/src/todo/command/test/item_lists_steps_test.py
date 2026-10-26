import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command import AddItemCommand, AddItemHandler, AddItemResult, ListItemsCommand, ListItemsHandler, ListItemsResult
from todo.command.pipeline import Kind, KindError, kind_of
from todo.domain.entity import Item

scenarios("../features/item_lists.feature")


@dataclass
class FakeItemLists:
    """The item lists faked: what they hold, what they were given, or the kind they answer."""

    held: dict[str, list[Item]] = field(default_factory=dict)
    given: list[tuple[str, str]] = field(default_factory=list)
    kind: Kind | None = None
    asked: bool = False

    async def add(self, list_id: str, item: Item) -> None:
        self.asked = True
        if self.kind is not None:
            raise KindError(self.kind, "answered by the scenario")
        self.given.append((list_id, item.title))

    async def items(self, list_id: str) -> list[Item]:
        self.asked = True
        if self.kind is not None:
            raise KindError(self.kind, "answered by the scenario")
        return self.held[list_id]


@dataclass
class World:
    lists: FakeItemLists = field(default_factory=FakeItemLists)
    added: AddItemResult | None = None
    listed: ListItemsResult | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the item lists answer the kind "{kind}"'))
def the_item_lists_answer_the_kind(world: World, kind: str) -> None:
    world.lists.kind = Kind(kind)


@given(parsers.parse('the item lists hold "{titles}" for the list "{list_id}"'))
def the_item_lists_hold(world: World, titles: str, list_id: str) -> None:
    world.lists.held[list_id] = [Item(title) for title in titles.split("|")]


@when(parsers.re(r'I send AddItem with the list "(?P<list_id>[^"]*)" and the title "(?P<title>[^"]*)"'))
def i_send_add_item(world: World, list_id: str, title: str) -> None:
    try:
        world.added = asyncio.run(AddItemHandler(world.lists).handle(AddItemCommand(list_id=list_id, title=title)))
    except Exception as error:
        world.error = error
    print(f"when: AddItem {list_id!r} {title!r} -> {world.added}, error {world.error!r}")


@when(parsers.parse('I send ListItems with the list "{list_id}"'))
def i_send_list_items(world: World, list_id: str) -> None:
    try:
        world.listed = asyncio.run(ListItemsHandler(world.lists).handle(ListItemsCommand(list_id=list_id)))
    except Exception as error:
        world.error = error
    print(f"when: ListItems {list_id!r} -> {world.listed}, error {world.error!r}")


@then(parsers.parse('the added item should be the open item "{title}"'))
def the_added_item_should_be(world: World, title: str) -> None:
    assert world.added is not None, world.error
    assert (world.added.item.title, world.added.item.done) == (title, False)


@then(parsers.parse('the item lists should have been given "{title}" for the list "{list_id}"'))
def the_item_lists_should_have_been_given(world: World, title: str, list_id: str) -> None:
    assert world.lists.given == [(list_id, title)]


@then("the item lists should not have been asked")
def the_item_lists_should_not_have_been_asked(world: World) -> None:
    assert not world.lists.asked


@then(parsers.parse('the command should fail with the kind "{kind}"'))
def the_command_should_fail_with_the_kind(world: World, kind: str) -> None:
    assert kind_of(world.error) == kind, world.error


@then(parsers.parse('the listed items should be "{titles}"'))
def the_listed_items_should_be(world: World, titles: str) -> None:
    assert world.listed is not None, world.error
    assert [item.title for item in world.listed.items] == titles.split("|")
