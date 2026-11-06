"""The steps of import-items.feature: ImportItems over a fake AddItem that
records the titles it added and rejects the one a scenario names."""

import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command import AddItemCommand, AddItemResult, ItemView
from todo.command.import_items import ImportItemsCommand, ImportItemsHandler, ImportItemsResult
from todo.command.pipeline import Kind, KindError, kind_of

scenarios("../features/import-items.feature")


@dataclass
class FakeAddItem:
    rejected: str | None = None
    added: list[str] = field(default_factory=list)

    async def handle(self, command: AddItemCommand) -> AddItemResult:
        if command.title == self.rejected:
            raise KindError(Kind.CONFLICT, f"{command.title} is taken")
        self.added.append(command.title)
        return AddItemResult(item=ItemView(title=command.title, done=False))


@dataclass
class World:
    add_item: FakeAddItem = field(default_factory=FakeAddItem)
    result: ImportItemsResult | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the title "{title}" is rejected'))
def the_title_is_rejected(world: World, title: str) -> None:
    world.add_item.rejected = title


@when(parsers.parse('I send ImportItems to the list "{list_id}" with the titles "{titles}"'))
def i_send_import_items(world: World, list_id: str, titles: str) -> None:
    try:
        world.result = asyncio.run(ImportItemsHandler(world.add_item).handle(ImportItemsCommand(list_id=list_id, titles=tuple(titles.split("|")))))
    except Exception as error:
        world.error = error
    print(f"when: ImportItems({list_id!r}, {titles!r}) -> {world.result}, error {world.error!r}")


@then(parsers.parse('AddItem should have run for "{titles}"'))
def add_item_should_have_run_for(world: World, titles: str) -> None:
    assert "|".join(world.add_item.added) == titles


@then(parsers.parse('the import should answer the items "{titles}"'))
def the_import_should_answer(world: World, titles: str) -> None:
    assert world.result is not None, world.error
    assert "|".join(item.title for item in world.result.items) == titles


@then("the import should fail with the kind of the rejection")
def the_import_should_fail(world: World) -> None:
    assert kind_of(world.error) == Kind.CONFLICT, world.error
