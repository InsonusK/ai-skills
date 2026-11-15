"""The CompleteItem command."""

from dataclasses import dataclass
from typing import Protocol

from todo.command.plan_items import ItemView, kept_view
from todo.domain.entity import KeptItem
from todo.shared.errors import Kind, KindError


@dataclass(frozen=True)
class CompleteItemCommand:
    """Asks to mark the item item_id of the list list_id done. version is the
    item's version the caller read: an item changed since is not completed
    (skill vp-row-version)."""

    list_id: str
    item_id: int
    version: str


@dataclass(frozen=True)
class CompleteItemResult:
    """The completed item, with its new version."""

    item: ItemView


class ItemCompleter(Protocol):
    """What CompleteItemHandler needs of the item lists; service.ItemLists has it."""

    async def item(self, list_id: str, item_id: int) -> KeptItem: ...

    async def save(self, list_id: str, kept: KeptItem) -> KeptItem: ...


class NoVersionError(KindError):
    """A change with no version would skip the check, so it is refused
    (skill vp-row-version, "Require the version on a write")."""

    def __init__(self) -> None:
        super().__init__(Kind.INVALID, "the version of the item is missing: give back the version you read")


class CompleteItemHandler:
    """Handles CompleteItemCommand: reads the item, completes it by the
    entity's rule, and saves it under the caller's version - not the one just
    read - so a change made since the caller's read is a concurrent change,
    never overwritten."""

    def __init__(self, lists: ItemCompleter) -> None:
        self._lists = lists

    async def handle(self, command: CompleteItemCommand) -> CompleteItemResult:
        if not command.version:
            raise NoVersionError()
        kept = await self._lists.item(command.list_id, command.item_id)
        kept.item.complete()
        kept.version = command.version
        saved = await self._lists.save(command.list_id, kept)
        return CompleteItemResult(item=kept_view(saved))
