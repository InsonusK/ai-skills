"""The PlanItems command."""

from dataclasses import dataclass

from todo.domain.service import Planner


@dataclass(frozen=True)
class PlanItemsCommand:
    """Asks to plan one open to-do item per title."""

    titles: list[str]

    def required_role(self) -> str:
        """Marks PlanItemsCommand access.Restricted: only a planner may plan."""
        return "planner"

    def batch_size(self) -> int:
        """Marks PlanItemsCommand batch_limit.Batch: its size is the number of titles."""
        return len(self.titles)


@dataclass(frozen=True)
class ItemView:
    """An item as callers see it; domain types never leave the application."""

    title: str
    done: bool


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
