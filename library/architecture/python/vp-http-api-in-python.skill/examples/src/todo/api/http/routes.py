"""The HTTP entry point's registration: the resources it serves, one module
each in endpoint/, and the one call that runs it. The fixed code is in
pipeline/."""

import asyncio
import logging
from dataclasses import dataclass

from fastapi import APIRouter

from todo.api.http import pipeline
from todo.api.http.endpoint import ItemsHandler


@dataclass(frozen=True)
class Endpoints:
    """The resources this API serves, one field per module of endpoint/. The
    composition root fills them. A new resource adds a field here and a line
    to routes(); a new command of an existing resource changes only that
    resource's module."""

    items: ItemsHandler


def routes(endpoints: Endpoints) -> list[APIRouter]:
    """The operations of the published contract
    (docs/integration/api/todo/openapi.json): the router of every resource.
    The contract and the routes are kept in step by one scenario."""
    return [endpoints.items.router()]


async def run(endpoints: Endpoints, logger: logging.Logger, stop: asyncio.Event) -> None:
    """Serves the routes of every resource until stop is set (see pipeline.run)."""
    await pipeline.run(routes(endpoints), logger, stop)
