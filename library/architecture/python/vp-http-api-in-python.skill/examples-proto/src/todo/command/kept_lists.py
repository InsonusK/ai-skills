"""The lists of this example, kept in the process."""

import asyncio

from todo.domain.entity import Item
from todo.shared.errors import Kind, KindError


class KeptLists:
    """Keeps the lists of this example in the process while it runs, lost when
    it stops: the HTTP variation point adds a way to call the application, not
    a store, and AddItem and ListItems need somewhere to keep what they show.
    It has what the handlers ask of the item lists (ItemLists of handlers.py);
    the DB variation point builds a service over the database in its place, and
    no handler changes. A title is unique in its list."""

    def __init__(self) -> None:
        self._items: dict[str, list[Item]] = {}
        self._lock = asyncio.Lock()

    async def add(self, list_id: str, item: Item) -> None:
        """Keeps item at the end of the list, started on its first item; a
        title already in the list is a conflict."""
        async with self._lock:
            kept = self._items.setdefault(list_id, [])
            if any(existing.title == item.title for existing in kept):
                raise KindError(Kind.CONFLICT, f'list "{list_id}" already has an item "{item.title}"')
            kept.append(item)

    async def items(self, list_id: str) -> list[Item]:
        """The items of the list in the order they were added; a list nobody
        added to is not found."""
        async with self._lock:
            if list_id not in self._items:
                raise KindError(Kind.NOT_FOUND, f'no list "{list_id}"')
            return list(self._items[list_id])
