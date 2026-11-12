"""The ImportItems command."""

from dataclasses import dataclass

from todo.command.add_item import AddItemCommand, AddItemResult
from todo.command.pipeline import Handler
from todo.command.plan_items import ItemView


@dataclass(frozen=True)
class ImportItemsCommand:
    """Asks to add every one of titles to the list list_id, or none."""

    list_id: str
    titles: tuple[str, ...]


@dataclass(frozen=True)
class ImportItemsResult:
    """The added items, in the order of the titles."""

    items: tuple[ItemView, ...]


class ImportItemsHandler:
    """Handles ImportItemsCommand by one nested AddItem command per title, one
    after another. The first that fails ends the import with its error; the
    unit of work, begun by this command, then drops the items added before it."""

    def __init__(self, add_item: Handler[AddItemCommand, AddItemResult]) -> None:
        """add_item is the AddItem handler the command registry decorated, so
        the nested command runs through every middleware."""
        self._add_item = add_item

    async def handle(self, command: ImportItemsCommand) -> ImportItemsResult:
        items: list[ItemView] = []
        for title in command.titles:
            added = await self._add_item.handle(AddItemCommand(list_id=command.list_id, title=title))
            items.append(added.item)
        return ImportItemsResult(items=tuple(items))
