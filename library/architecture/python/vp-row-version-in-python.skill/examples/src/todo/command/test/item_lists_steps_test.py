import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command import AddItemCommand, AddItemHandler, AddItemResult, ListItemsCommand, ListItemsHandler, ListItemsResult
from todo.command.pipeline import Kind, KindError, kind_of
from todo.domain.entity import Item, KeptItem

scenarios("../features/item_lists.feature")

KEPT_VERSION = "8"
"""The version the fake answers for an item it keeps or saves."""


@dataclass
class FakeItemLists:
    """The item lists faked: what they hold, what they were given, the item
    they were asked to save, or the kind they answer."""

    held: dict[str, list[KeptItem]] = field(default_factory=dict)
    given: list[tuple[str, str]] = field(default_factory=list)
    kind: Kind | None = None
    save_kind: Kind | None = None
    saved: KeptItem | None = None
    asked: bool = False

    async def add(self, list_id: str, item: Item) -> KeptItem:
        self.asked = True
        if self.kind is not None:
            raise KindError(self.kind, "answered by the scenario")
        self.given.append((list_id, item.title))
        return KeptItem(id=len(self.given), version=KEPT_VERSION, item=item)

    async def items(self, list_id: str) -> list[KeptItem]:
        self.asked = True
        if self.kind is not None:
            raise KindError(self.kind, "answered by the scenario")
        return self.held[list_id]

    async def item(self, list_id: str, item_id: int) -> KeptItem:
        self.asked = True
        for kept in self.held.get(list_id, []):
            if kept.id == item_id:
                # A copy, as a service reads a new entity each time.
                return KeptItem(id=kept.id, version=kept.version, item=_copy(kept.item))
        raise KindError(Kind.NOT_FOUND, f"no item {item_id}")

    async def save(self, list_id: str, kept: KeptItem) -> KeptItem:
        # What the command gave, before the answer's new version.
        self.saved = KeptItem(id=kept.id, version=kept.version, item=kept.item)
        if self.save_kind is not None:
            raise KindError(self.save_kind, "answered by the scenario")
        return KeptItem(id=kept.id, version=KEPT_VERSION, item=kept.item)


def _copy(item: Item) -> Item:
    copy = Item(item.title)
    if item.done:
        copy.complete()
    return copy


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
    world.lists.held[list_id] = [KeptItem(id=i + 1, version="7", item=Item(title)) for i, title in enumerate(titles.split("|"))]


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


@then("the added item should carry the id and the version the item lists answered")
def the_added_item_should_carry(world: World) -> None:
    assert world.added is not None, world.error
    assert (world.added.item.id, world.added.item.version) == (1, KEPT_VERSION)


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
