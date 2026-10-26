import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.domain.entity import Item, kind_of
from todo.infrastructure.memory import DuplicateKeyError, NoRowsError, Row
from todo.service import ItemLists

scenarios("../features/item_lists.feature")


@dataclass
class FakeStore:
    """The store faked: rows by list, or the error the scenario set."""

    rows: dict[str, list[Row]] = field(default_factory=dict)
    error: Exception | None = None

    async def insert(self, list_id: str, row: Row) -> None:
        if self.error is not None:
            raise self.error
        self.rows.setdefault(list_id, []).append(row)

    async def select(self, list_id: str) -> list[Row]:
        if self.error is not None:
            raise self.error
        return self.rows[list_id]


@dataclass
class World:
    store: FakeStore = field(default_factory=FakeStore)
    items: list[Item] = field(default_factory=list)
    error: Exception | None = None

    def service(self) -> ItemLists:
        return ItemLists(self.store)


@pytest.fixture
def world() -> World:
    return World()


@given("the store answers a duplicate key")
def the_store_answers_a_duplicate_key(world: World) -> None:
    world.store.error = DuplicateKeyError("duplicate key")


@given("the store answers no rows")
def the_store_answers_no_rows(world: World) -> None:
    world.store.error = NoRowsError("no rows")


@given(parsers.parse('the store fails with "{message}"'))
def the_store_fails(world: World, message: str) -> None:
    world.store.error = OSError(message)


@when(parsers.parse('I add "{title}" to the list "{list_id}"'))
def i_add(world: World, title: str, list_id: str) -> None:
    try:
        asyncio.run(world.service().add(list_id, Item(title)))
    except Exception as error:
        world.error = error
    print(f"when: add {title!r} to {list_id!r} -> error {world.error!r}")


@when(parsers.parse('I read the list "{list_id}"'))
def i_read(world: World, list_id: str) -> None:
    try:
        world.items = asyncio.run(world.service().items(list_id))
    except Exception as error:
        world.error = error
    print(f"when: read {list_id!r} -> {[item.title for item in world.items]}, error {world.error!r}")


@then(parsers.parse('the items should be "{titles}"'))
def the_items_should_be(world: World, titles: str) -> None:
    assert [item.title for item in world.items] == titles.split("|"), world.error


@then(parsers.parse('the service should fail with the kind "{kind}" mentioning "{text}"'))
def the_service_should_fail_with_the_kind(world: World, kind: str, text: str) -> None:
    assert kind_of(world.error) == kind and text in str(world.error), world.error


@then("the service should fail with no kind")
def the_service_should_fail_with_no_kind(world: World) -> None:
    assert world.error is not None and kind_of(world.error) is None, world.error
