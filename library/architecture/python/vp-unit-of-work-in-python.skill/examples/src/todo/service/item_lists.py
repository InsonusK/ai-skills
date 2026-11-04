"""The ItemLists service."""

from typing import Protocol

from todo.domain.entity import Item
from todo.infrastructure.postgres import DuplicateKeyError, Selector, Specification, UnavailableError
from todo.infrastructure.postgres.tables import ItemPosition, ItemRow, ItemsOfList
from todo.service.errors import ItemAlreadyListedError, ListNotFoundError, ListsUnavailableError


class ItemRows(Protocol):
    """What ItemLists needs of the rows of list_items; postgres.Repository[ItemRow] has it."""

    async def insert(self, row: ItemRow) -> None: ...

    def select_by(self, specification: Specification[ItemRow]) -> Selector[ItemRow]: ...


class ItemLists:
    """Keeps items in lists, as rows of list_items. It turns the repository's
    outcomes into the application's error kinds: a duplicate key is a
    conflict, no rows is not found, an unavailable database is unavailable;
    any other failure keeps no kind. A kind's text reaches the caller, so the
    repository's failure stays its cause, in the log only."""

    def __init__(self, rows: ItemRows) -> None:
        self._rows = rows

    async def add(self, list_id: str, item: Item) -> None:
        """Keeps item in the list."""
        try:
            await self._rows.insert(ItemRow(list_id=list_id, title=item.title, done=item.done))
        except DuplicateKeyError as error:
            raise ItemAlreadyListedError(list_id, item.title) from error
        except UnavailableError as error:
            raise ListsUnavailableError() from error

    async def items(self, list_id: str) -> list[Item]:
        """The items of the list in the order they were added."""
        try:
            rows = await self._rows.select_by(ItemsOfList(list_id)).order_by(ItemPosition).all()
        except UnavailableError as error:
            raise ListsUnavailableError() from error
        if not rows:
            raise ListNotFoundError(list_id)
        items: list[Item] = []
        for row in rows:
            item = Item(row.title)
            if row.done:
                item.complete()
            items.append(item)
        return items
