"""The PlanRemoteItems command."""

from dataclasses import dataclass
from typing import Protocol

from todo.command.plan_items import ItemView
from todo.domain.entity import Item


@dataclass(frozen=True)
class PlanRemoteItemsCommand:
    """Asks the todo service to plan one open item per title."""

    titles: list[str]


@dataclass(frozen=True)
class PlanRemoteItemsResult:
    """The items the todo service planned, in the order of the titles."""

    items: list[ItemView]


class RemotePlanner(Protocol):
    """What PlanRemoteItemsHandler needs; service.RemotePlanner has it."""

    async def plan(self, titles: list[str]) -> list[Item]: ...


class PlanRemoteItemsHandler:
    """Handles PlanRemoteItemsCommand: titles the todo service rejects are
    invalid; a todo service that does not answer is unavailable."""

    def __init__(self, planner: RemotePlanner) -> None:
        self._planner = planner

    async def handle(self, command: PlanRemoteItemsCommand) -> PlanRemoteItemsResult:
        items = await self._planner.plan(command.titles)
        return PlanRemoteItemsResult(items=[ItemView(title=item.title, done=item.done) for item in items])
