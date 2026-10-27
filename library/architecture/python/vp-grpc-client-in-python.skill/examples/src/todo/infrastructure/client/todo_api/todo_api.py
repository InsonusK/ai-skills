"""The adapter of the todo API of another service
(docs/integration/client/todo/openapi.json): one method per operation this
application calls, each a call of HttpClient with a request and a reading of
the answer of its own. It reports technical outcomes - ApiUnavailableError,
ApiStatusError - and leaves their meaning to the service over it
(todo.service)."""

import logging
from dataclasses import dataclass
from typing import Any

from todo.infrastructure.client.core.httpclient import HttpClient, HttpConfig, load_config
from todo.infrastructure.client.todo_api.mock import emulator

PREFIX = "TODO_API"
"""Names the settings of this API: TODO_API_HOST, TODO_API_HTTP_PORT, TODO_API_TLS, TODO_API_TIMEOUT."""

OPERATIONS = [
    ("POST", "/v1/items/plan"),
]
"""Every operation this adapter calls; one scenario holds them against the
copied contract. A new method of the adapter adds its line."""


@dataclass(frozen=True)
class Item:
    """An item as the API answers it, the adapter's own type."""

    title: str
    done: bool


@dataclass(frozen=True)
class _PlanItemsRequest:
    titles: list[str]


def _plan_items_response(answer: Any) -> list[Item]:
    return [Item(title=str(item["title"]), done=bool(item["done"])) for item in answer["items"]]


class TodoApi:
    """The todo API."""

    def __init__(self, config: HttpConfig, logger: logging.Logger) -> None:
        """Builds the client from settings given - the scenarios point it at an
        API of their own. With the host mock it answers from the emulator."""
        self._http = HttpClient(config, logger, emulator)

    async def aclose(self) -> None:
        """Closes the connections to the API."""
        await self._http.aclose()

    async def plan_items(self, titles: list[str]) -> list[Item]:
        """POST /v1/items/plan: one open item per title, planned by the API."""
        return await self._http.call("POST", "/v1/items/plan", _PlanItemsRequest(titles=titles), _plan_items_response)


def new(logger: logging.Logger) -> TodoApi:
    """Reads the settings of the API and builds its client; a missing or
    unusable setting raises ValueError naming its variable."""
    return TodoApi(load_config(PREFIX), logger)
