"""The ListItems command."""

from dataclasses import dataclass
from typing import Protocol

from todo.command.plan_items import ItemView
from todo.domain.entity import Item


@dataclass(frozen=True)
class ListItemsCommand:
    """Asks for the items of the list list_id."""

    list_id: str


@dataclass(frozen=True)
class ListItemsResult:
    """The items in the order they were added."""

    items: list[ItemView]


class ItemReader(Protocol):
    """What ListItemsHandler needs of the item lists; service.ItemLists has it."""

    async def items(self, list_id: str) -> list[Item]: ...


class ListItemsHandler:
    """Handles ListItemsCommand: an unknown list is the service's not found."""

    def __init__(self, lists: ItemReader) -> None:
        self._lists = lists

    async def handle(self, command: ListItemsCommand) -> ListItemsResult:
        items = await self._lists.items(command.list_id)
        return ListItemsResult(items=[ItemView(title=item.title, done=item.done) for item in items])
