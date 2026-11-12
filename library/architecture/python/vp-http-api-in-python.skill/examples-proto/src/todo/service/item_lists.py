"""The ItemLists service."""

from typing import Protocol

from todo.domain.entity import Item
from todo.infrastructure.memory import DuplicateKeyError, NoRowsError, Row
from todo.service.errors import ItemAlreadyListedError, ListNotFoundError


class ListStore(Protocol):
    """What ItemLists needs of its infrastructure; memory.Lists has it."""

    async def insert(self, list_id: str, row: Row) -> None: ...

    async def select(self, list_id: str) -> list[Row]: ...


class ItemLists:
    """Keeps items in lists. It turns the store's technical outcomes into the
    application's error kinds: a duplicate key is a conflict, no rows is not
    found; any other failure of the store keeps no kind."""

    def __init__(self, store: ListStore) -> None:
        self._store = store

    async def add(self, list_id: str, item: Item) -> None:
        """Keeps item in the list."""
        try:
            await self._store.insert(list_id, Row(title=item.title, done=item.done))
        except DuplicateKeyError as error:
            raise ItemAlreadyListedError(list_id, item.title) from error

    async def items(self, list_id: str) -> list[Item]:
        """The items of the list in the order they were added."""
        try:
            rows = await self._store.select(list_id)
        except NoRowsError as error:
            raise ListNotFoundError(list_id) from error
        items: list[Item] = []
        for row in rows:
            item = Item(row.title)
            if row.done:
                item.complete()
            items.append(item)
        return items
