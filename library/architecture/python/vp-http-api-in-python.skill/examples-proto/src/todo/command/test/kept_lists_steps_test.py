import asyncio
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.kept_lists import KeptLists
from todo.domain.entity import Item
from todo.shared.errors import Kind, kind_of

scenarios("../features/kept-lists-in-python.feature")


@dataclass
class Kept:
    lists: KeptLists = field(default_factory=KeptLists)
    error: Exception | None = None


@pytest.fixture
def kept() -> Kept:
    return Kept()


@given(parsers.parse('I add "{title}" to the kept list "{list_id}"'))
@when(parsers.parse('I add "{title}" to the kept list "{list_id}"'))
def add(kept: Kept, title: str, list_id: str) -> None:
    try:
        asyncio.run(kept.lists.add(list_id, Item(title)))
        kept.error = None
    except Exception as error:  # noqa: BLE001 - the scenario reads the failure
        kept.error = error


@when(parsers.parse('I read the kept list "{list_id}"'))
def read(kept: Kept, list_id: str) -> None:
    try:
        asyncio.run(kept.lists.items(list_id))
    except Exception as error:  # noqa: BLE001 - the scenario reads the failure
        kept.error = error


@then(parsers.parse('the kept list "{list_id}" should hold "{titles}"'))
def holds(kept: Kept, list_id: str, titles: str) -> None:
    items = asyncio.run(kept.lists.items(list_id))
    assert [item.title for item in items] == titles.split("|")


@then(parsers.parse("keeping should fail with the kind \"{kind}\" mentioning '{text}'"))
def fails(kept: Kept, kind: str, text: str) -> None:
    assert kind_of(kept.error) == Kind(kind)
    assert text in str(kept.error)
