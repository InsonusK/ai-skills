"""The HTTP entry point's registration: the resources it serves, one module
each in endpoint/, and the entry factory the composition root lists. The
fixed code is in pipeline/."""

import logging
from collections.abc import Sequence
from dataclasses import dataclass

from fastapi import APIRouter

from todo.api.core.entry import Entry
from todo.api.http import pipeline
from todo.api.http.endpoint import ItemsHandler
from todo.api.http.pipeline import Middleware
from todo.command import Handlers


@dataclass(frozen=True)
class Endpoints:
    """The resources this API serves, one field per module of endpoint/. A new
    resource adds a field here and a line to from_handlers and to routes(); a
    new command of an existing resource changes only that resource's module."""

    items: ItemsHandler

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "Endpoints":
        """Gives every resource the handlers of its commands from the command registry."""
        return cls(
            items=ItemsHandler.from_handlers(handlers),
        )


def routes(endpoints: Endpoints) -> list[APIRouter]:
    """The operations of the published contract
    (docs/integration/api/todo/openapi.json): the router of every resource.
    The contract and the routes are kept in step by one scenario."""
    return [endpoints.items.router()]


def entry(handlers: Handlers, logger: logging.Logger, middlewares: Sequence[Middleware]) -> Entry:
    """The HTTP entry point as the composition root lists it for run_all: the
    routes of every resource over the handlers of the registry, each wrapped
    by the middlewares - the first the outermost, all inside the request log -
    served until the process stops (see pipeline.run)."""
    routers = routes(Endpoints.from_handlers(handlers))
    return Entry("http", lambda stop: pipeline.run(routers, logger, stop, middlewares))
