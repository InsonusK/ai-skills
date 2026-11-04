"""The PlanItems command."""

from dataclasses import dataclass

from todo.domain.entity import KeptItem
from todo.domain.service import Planner


@dataclass(frozen=True)
class PlanItemsCommand:
    """Asks to plan one open to-do item per title."""

    titles: list[str]


@dataclass(frozen=True)
class ItemView:
    """An item as callers see it; domain types never leave the application.
    id and version are set for an item kept in a list, None for a planned
    one; the version is given back unchanged with a change of the item."""

    title: str
    done: bool
    id: int | None = None
    version: str | None = None


@dataclass(frozen=True)
class PlanItemsResult:
    """The planned items, in the order of the titles."""

    items: list[ItemView]


class PlanItemsHandler:
    """Handles PlanItemsCommand: a Handler[PlanItemsCommand, PlanItemsResult]."""

    def __init__(self) -> None:
        self._planner = Planner()

    async def handle(self, command: PlanItemsCommand) -> PlanItemsResult:
        items = self._planner.plan(command.titles)
        return PlanItemsResult(items=[ItemView(title=item.title, done=item.done) for item in items])


def kept_view(kept: KeptItem) -> ItemView:
    """The view of an item kept in a list: its fields, its id, its version."""
    return ItemView(title=kept.item.title, done=kept.item.done, id=kept.id, version=kept.version)
