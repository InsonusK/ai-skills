"""The items resource (`todo items ...`): its command handlers, its actions,
and per action its flags, its output type, and its text form. A new command
on items adds a handler, its line of from_handlers, an action line, and its
bind, encode, and text functions here."""

from dataclasses import dataclass

import click

from todo.api.cli.pipeline import Binding, Resource, command
from todo.command import Handlers, PlanItemsCommand, PlanItemsResult
from todo.command.pipeline import Handler


class ItemsHandler:
    """The items resource: the command handlers its actions call.
    from_handlers takes them from the command registry; resource() lists its
    actions."""

    def __init__(self, plan_items: Handler[PlanItemsCommand, PlanItemsResult]) -> None:
        self._plan_items = plan_items

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "ItemsHandler":
        """Takes the handlers of the items commands from the command registry."""
        return cls(
            plan_items=handlers.plan_items,
        )

    def resource(self) -> Resource:
        """`todo items` with its actions."""
        return Resource("items", "Plan to-do items", [
            command("plan", "Plan one open item per --title", self._plan_items, bind_plan_items, encode_plan_items, text_plan_items),
        ])


def bind_plan_items() -> Binding[PlanItemsCommand]:
    """The flags of `todo items plan`."""
    return Binding(
        params=[click.Option(["--title", "titles"], multiple=True, help="the title of an item; repeat for several")],
        read=lambda flags: PlanItemsCommand(titles=list(flags["titles"])),
    )


@dataclass(frozen=True)
class ItemOutput:
    title: str
    done: bool


@dataclass(frozen=True)
class PlanItemsOutput:
    """The JSON line of `todo items plan`."""

    items: list[ItemOutput]


def encode_plan_items(result: PlanItemsResult) -> PlanItemsOutput:
    return PlanItemsOutput(items=[ItemOutput(title=item.title, done=item.done) for item in result.items])


def text_plan_items(result: PlanItemsResult) -> str:
    """`todo items plan --output text`: one checkbox line per item."""
    return "\n".join(f"{'[x]' if item.done else '[ ]'} {item.title}" for item in result.items)
