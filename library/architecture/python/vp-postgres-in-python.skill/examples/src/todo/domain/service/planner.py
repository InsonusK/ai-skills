"""The planner domain service."""

from collections.abc import Sequence

from todo.domain.entity import DuplicateTitleError, Item, NoItemsError


class Planner:
    """A domain service: it holds the rules that span several items, which
    no single item can check on its own.
    """

    def plan(self, titles: Sequence[str]) -> list[Item]:
        """Creates one open item per title; titles must be valid items and must
        not repeat, compared case-insensitively after trimming.
        """
        if not titles:
            raise NoItemsError()
        seen: set[str] = set()
        items: list[Item] = []
        for title in titles:
            item = Item(title)
            key = item.title.casefold()
            if key in seen:
                raise DuplicateTitleError()
            seen.add(key)
            items.append(item)
        return items
