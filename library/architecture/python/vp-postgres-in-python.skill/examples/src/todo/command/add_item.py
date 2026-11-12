"""The AddItem command."""

from dataclasses import dataclass
from typing import Protocol

from todo.command.plan_items import ItemView
from todo.domain.entity import Item


@dataclass(frozen=True)
class AddItemCommand:
    """Asks to add an open item with title to the list list_id."""

    list_id: str
    title: str


@dataclass(frozen=True)
class AddItemResult:
    """The added item."""

    item: ItemView


class ItemAdder(Protocol):
    """What AddItemHandler needs of the item lists; service.ItemLists has it."""

    async def add(self, list_id: str, item: Item) -> None: ...


class AddItemHandler:
    """Handles AddItemCommand: an invalid title is the entity's invalid
    error, a title already in the list the service's conflict."""

    def __init__(self, lists: ItemAdder) -> None:
        self._lists = lists

    async def handle(self, command: AddItemCommand) -> AddItemResult:
        item = Item(command.title)
        await self._lists.add(command.list_id, item)
        return AddItemResult(item=ItemView(title=item.title, done=item.done))
