"""The ItemsService of the contract: its command handlers, the implementation
of its generated servicer, and per method the mapping of its messages to and
from the command. A new method adds a handler, its line of from_handlers, a
method of _ItemsServicer, and its decode and encode here."""

from typing import Any

import grpc

from todo.api.grpc.pipeline import Service, call
from todo.command import Handlers, PlanItemsCommand, PlanItemsResult
from todo.command.pipeline import Handler
from todo.gen.api.todo import todo_pb2, todo_pb2_grpc


class ItemsHandler:
    """The ItemsService of the contract: the command handlers its methods
    call. from_handlers takes them from the command registry; service()
    registers it."""

    def __init__(self, plan_items: Handler[PlanItemsCommand, PlanItemsResult]) -> None:
        self.plan_items = plan_items

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "ItemsHandler":
        """Takes the handlers of the ItemsService's commands from the command registry."""
        return cls(
            plan_items=handlers.plan_items,
        )

    def service(self) -> Service:
        """ItemsService as the server registers it."""
        return Service(
            name=todo_pb2.DESCRIPTOR.services_by_name["ItemsService"].full_name,
            add_to=lambda server: todo_pb2_grpc.add_ItemsServiceServicer_to_server(_ItemsServicer(self), server),
        )


class _ItemsServicer(todo_pb2_grpc.ItemsServiceServicer):
    """Implements the generated servicer. Its methods are abstract in the
    generated type stub, so a method added to the proto file and not written
    here fails the type check (make lint) - "cannot instantiate abstract
    class" - instead of answering UNIMPLEMENTED at run time."""

    def __init__(self, handler: ItemsHandler) -> None:
        self._handler = handler

    async def PlanItems(self, request: todo_pb2.PlanItemsRequest, context: grpc.aio.ServicerContext[Any, Any]) -> todo_pb2.PlanItemsResponse:
        return await call(self._handler.plan_items, request, decode_plan_items, encode_plan_items)


def decode_plan_items(request: todo_pb2.PlanItemsRequest) -> PlanItemsCommand:
    return PlanItemsCommand(titles=list(request.titles))


def encode_plan_items(result: PlanItemsResult) -> todo_pb2.PlanItemsResponse:
    return todo_pb2.PlanItemsResponse(items=[todo_pb2.Item(title=item.title, done=item.done) for item in result.items])
