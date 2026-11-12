"""The command-line entry point's registration: the resources it offers, one
module each in endpoint/, and the entry factory the composition root lists.
The fixed code is in pipeline/."""

import logging
from collections.abc import Sequence
from dataclasses import dataclass
from typing import TextIO

from todo.api.cli import pipeline
from todo.api.cli.endpoint import ItemsHandler
from todo.api.cli.pipeline import Resource
from todo.api.core.entry import Entry
from todo.command import Handlers


@dataclass(frozen=True)
class Endpoints:
    """The resources of the command line, one field per module of endpoint/ -
    the same resources as the HTTP API's. A new resource adds a field here
    and a line to from_handlers and to resources(); a new action of an
    existing resource changes only that resource's module."""

    items: ItemsHandler

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "Endpoints":
        """Gives every resource the handlers of its commands from the command registry."""
        return cls(
            items=ItemsHandler.from_handlers(handlers),
        )


def resources(endpoints: Endpoints) -> list[Resource]:
    """The resources of the command line: `todo {resource} {action}`."""
    return [endpoints.items.resource()]


def entry(
    handlers: Handlers,
    logger: logging.Logger,
    name: str,
    version: str,
    args: Sequence[str],
    stdout: TextIO,
    stderr: TextIO,
) -> Entry:
    """The CLI entry point as the composition root lists it for run_all: it
    runs the action args name over the handlers of the registry and ends, and
    the process ends with it. It shows its error itself (reports_errors), so
    run_all does not log it again. The command line has no middleware of its
    own: a step on every action is a command middleware."""
    endpoints = Endpoints.from_handlers(handlers)
    return Entry(
        "cli",
        lambda _: run(logger, name, version, args, endpoints, stdout, stderr),
        reports_errors=True,
    )


async def run(
    logger: logging.Logger,
    name: str,
    version: str,
    args: Sequence[str],
    endpoints: Endpoints,
    stdout: TextIO,
    stderr: TextIO,
) -> None:
    """Runs the action args name over the resources and raises its error (see
    pipeline.run); the scenarios call it, the composition root lists entry."""
    await pipeline.run(logger, name, version, args, resources(endpoints), stdout, stderr)
