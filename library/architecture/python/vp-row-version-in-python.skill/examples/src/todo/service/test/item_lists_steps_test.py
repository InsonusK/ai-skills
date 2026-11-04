import asyncio
from collections.abc import Awaitable, Iterator
from dataclasses import dataclass, field
from typing import TypeVar

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.domain.entity import Item, KeptItem
from todo.infrastructure import postgres
from todo.infrastructure.postgres import DuplicateKeyError, UnavailableError, migrations, postgrestest
from todo.infrastructure.postgres.tables import ItemRow
from todo.service import ItemLists
from todo.shared.errors import kind_of

scenarios("../features/item_lists.feature")

T = TypeVar("T")


@dataclass
class World:
    """One scenario: the service over a migrated database of its own, or over
    a repository that answers the failure a step chose."""

    loop: asyncio.AbstractEventLoop = field(default_factory=asyncio.new_event_loop)
    scenario: postgrestest.ScenarioDatabase | None = None
    database: postgres.Database | None = None
    lists: ItemLists | None = None
    items: list[KeptItem] = field(default_factory=list)
    # kept is the item the last add, read, or save answered.
    kept: KeptItem | None = None
    error: Exception | None = None

    def run(self, call: Awaitable[T]) -> T:
        return self.loop.run_until_complete(call)

    def fail_with(self, error: Exception) -> None:
        self.lists = ItemLists(postgrestest.Failing[ItemRow](error))


@pytest.fixture
def world() -> Iterator[World]:
    world = World()
    world.scenario = world.run(postgrestest.create())
    world.database = postgres.Database(world.scenario.config)
    world.run(postgres.migrate(world.database, migrations.LOCATION))
    world.lists = ItemLists(postgres.Repository(world.database, ItemRow))
    yield world
    world.run(world.database.close())
    world.run(world.scenario.drop())
    world.loop.close()


def lists(world: World) -> ItemLists:
    assert world.lists is not None
    return world.lists


@given("the store answers a duplicate key")
def the_store_answers_a_duplicate_key(world: World) -> None:
    world.fail_with(DuplicateKeyError("insert into list_items: duplicate key"))


@given("the store answers that the database is unavailable")
def the_store_answers_unavailable(world: World) -> None:
    world.fail_with(UnavailableError("select list_items: database unavailable"))


@given(parsers.parse('the store fails with "{message}"'))
def the_store_fails(world: World, message: str) -> None:
    world.fail_with(RuntimeError(message))


@given(parsers.parse('I add "{title}" to the list "{list_id}"'))
@when(parsers.parse('I add "{title}" to the list "{list_id}"'))
def i_add(world: World, title: str, list_id: str) -> None:
    try:
        world.kept = world.run(lists(world).add(list_id, Item(title)))
    except Exception as error:
        world.error = error
    print(f"when: add {title!r} to {list_id!r} -> {world.kept}, error {world.error!r}")


@then("the added item should carry an id and a version")
def the_added_item_should_carry(world: World) -> None:
    assert world.kept is not None and world.kept.id and world.kept.version, (world.kept, world.error)


def kept(world: World) -> KeptItem:
    assert world.kept is not None, world.error
    return world.kept


@given("I read that item")
def i_read_that_item(world: World) -> None:
    world.kept = world.run(lists(world).item("home", kept(world).id))


@when(parsers.parse('I read the item {item_id:d} of the list "{list_id}"'))
def i_read_the_item(world: World, item_id: int, list_id: str) -> None:
    try:
        world.kept = world.run(lists(world).item(list_id, item_id))
    except Exception as error:
        world.error = error


@given("another caller completes that item meanwhile")
def another_caller_completes(world: World) -> None:
    other = world.run(lists(world).item("home", kept(world).id))
    other.item.complete()
    world.run(lists(world).save("home", other))


@when("I complete it and save it under the version I read")
def i_complete_and_save(world: World) -> None:
    read = kept(world)
    read.item.complete()
    read_with = read.version
    try:
        world.kept = world.run(lists(world).save("home", read))
    except Exception as error:
        world.error = error
        return
    assert world.kept.version != read_with, f"the saved item kept the version {read_with!r} it was read with"


@then("the item read back should be done, at the version the save answered")
def the_item_read_back(world: World) -> None:
    saved = kept(world)
    read = world.run(lists(world).item("home", saved.id))
    assert read.item.done and read.version == saved.version, (read, saved)


@when(parsers.parse('I read the list "{list_id}"'))
def i_read(world: World, list_id: str) -> None:
    try:
        world.items = world.run(lists(world).items(list_id))
    except Exception as error:
        world.error = error
    print(f"when: read {list_id!r} -> {[kept.item.title for kept in world.items]}, error {world.error!r}")


@then(parsers.parse('the items should be "{titles}"'))
def the_items_should_be(world: World, titles: str) -> None:
    assert [kept.item.title for kept in world.items] == titles.split("|"), world.error


@then(parsers.parse('the service should fail with the kind "{kind}" mentioning "{text}"'))
def the_service_should_fail_with_the_kind(world: World, kind: str, text: str) -> None:
    assert kind_of(world.error) == kind and text in str(world.error), world.error


@then("the service should fail with no kind")
def the_service_should_fail_with_no_kind(world: World) -> None:
    assert world.error is not None and kind_of(world.error) is None, world.error
