"""The adapter of the todo API of another service
(docs/integration/client/todo/todo.proto): one method per method of the
contract this application calls, each a call of the generated stub over
GrpcClient's channel, its response mapped to types of its own. It reports
technical outcomes - ApiUnavailableError, ApiStatusError - and leaves their
meaning to the service over it (todo.service)."""

import logging
from dataclasses import dataclass

from todo.gen.client.todo import todo_pb2, todo_pb2_grpc
from todo.infrastructure.client.core.grpcclient import GrpcClient, GrpcConfig, load_config
from todo.infrastructure.client.todo_api.mock import emulator

PREFIX = "TODO_API"
"""Names the settings of this API: TODO_API_HOST, TODO_API_GRPC_PORT, TODO_API_TLS, TODO_API_TIMEOUT."""

METHODS = [
    "/todo.v1.ItemsService/PlanItems",
]
"""Every method of the contract this adapter calls, by full name; one scenario
holds them against the copied contract. A new method of the adapter adds its
line."""


@dataclass(frozen=True)
class Item:
    """An item as the API answers it, the adapter's own type."""

    title: str
    done: bool


class TodoApi:
    """The todo API."""

    def __init__(self, client: GrpcClient) -> None:
        self._grpc = client
        self._items = todo_pb2_grpc.ItemsServiceStub(client.channel)

    async def aclose(self) -> None:
        """Closes the channel to the API."""
        await self._grpc.aclose()

    async def plan_items(self, titles: list[str]) -> list[Item]:
        """ItemsService.PlanItems: one open item per title, planned by the API."""
        answer = await self._items.PlanItems(todo_pb2.PlanItemsRequest(titles=titles))
        return [Item(title=item.title, done=item.done) for item in answer.items]


async def open_api(config: GrpcConfig, logger: logging.Logger) -> TodoApi:
    """Builds the client from settings given - the scenarios point it at an
    API of their own. With the host mock it answers from the emulator."""
    return TodoApi(await GrpcClient.open(config, logger, emulator))


async def new(logger: logging.Logger) -> TodoApi:
    """Reads the settings of the API and builds its client; a missing or
    unusable setting raises ValueError naming its variable."""
    return await open_api(load_config(PREFIX), logger)
