"""The ItemLists service."""

from typing import Protocol

from todo.domain.entity import Item, KeptItem
from todo.infrastructure.postgres import DuplicateKeyError, RowChangedError, Selector, Specification, UnavailableError
from todo.infrastructure.postgres.tables import ItemOfList, ItemPosition, ItemRow, ItemsOfList
from todo.service.errors import (
    ItemAlreadyListedError,
    ItemChangedError,
    ItemNotFoundError,
    ListNotFoundError,
    ListsUnavailableError,
)


class ItemRows(Protocol):
    """What ItemLists needs of the rows of list_items; postgres.Repository[ItemRow] has it."""

    async def insert(self, row: ItemRow) -> None: ...

    async def update(self, row: ItemRow) -> None: ...

    def select_by(self, specification: Specification[ItemRow]) -> Selector[ItemRow]: ...


class ItemLists:
    """Keeps items in lists, as rows of list_items. It turns the repository's
    outcomes into the application's error kinds: a duplicate key is a
    conflict, no rows is not found, a row changed since the caller read it is
    a concurrent change, an unavailable database is unavailable; any other
    failure keeps no kind. A kind's text reaches the caller, so the
    repository's failure stays its cause, in the log only. Every item goes up
    with its id and its version."""

    def __init__(self, rows: ItemRows) -> None:
        self._rows = rows

    async def add(self, list_id: str, item: Item) -> KeptItem:
        """Keeps item in the list and answers it with its id and its first
        version, so the caller can change it next."""
        row = ItemRow(list_id=list_id, title=item.title, done=item.done)
        try:
            await self._rows.insert(row)
        except DuplicateKeyError as error:
            raise ItemAlreadyListedError(list_id, item.title) from error
        except UnavailableError as error:
            raise ListsUnavailableError() from error
        return _kept(row)

    async def items(self, list_id: str) -> list[KeptItem]:
        """The items of the list in the order they were added."""
        try:
            rows = await self._rows.select_by(ItemsOfList(list_id)).order_by(ItemPosition).all()
        except UnavailableError as error:
            raise ListsUnavailableError() from error
        if not rows:
            raise ListNotFoundError(list_id)
        return [_kept(row) for row in rows]

    async def item(self, list_id: str, item_id: int) -> KeptItem:
        """One item of the list, with its version as it is now."""
        try:
            row = await self._rows.select_by(ItemOfList(list_id, item_id)).single()
        except UnavailableError as error:
            raise ListsUnavailableError() from error
        if row is None:
            raise ItemNotFoundError(list_id, item_id)
        return _kept(row)

    async def save(self, list_id: str, kept: KeptItem) -> KeptItem:
        """Writes kept back to the list under the version it carries - the
        one the caller read - and answers it with its new version. An item
        changed since is a concurrent change: the caller reads it again and
        decides."""
        row = ItemRow(id=kept.id, list_id=list_id, title=kept.item.title, done=kept.item.done, version=kept.version)
        try:
            await self._rows.update(row)
        except RowChangedError as error:
            raise ItemChangedError(list_id, kept.id) from error
        except DuplicateKeyError as error:
            raise ItemAlreadyListedError(list_id, kept.item.title) from error
        except UnavailableError as error:
            raise ListsUnavailableError() from error
        return _kept(row)


def _kept(row: ItemRow) -> KeptItem:
    """The entity of a row, its rules checked again, with its id and version."""
    item = Item(row.title)
    if row.done:
        item.complete()
    return KeptItem(id=row.id, version=row.version or "", item=item)
