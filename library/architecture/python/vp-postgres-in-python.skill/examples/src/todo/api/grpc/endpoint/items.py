"""The ItemsService of the contract: its command handlers, the implementation
of its generated servicer, and per method the mapping of its messages to and
from the command. A new method adds a handler, its line of from_handlers, a
method of _ItemsServicer, and its decode and encode here."""

from typing import Any

import grpc

from todo.api.grpc.pipeline import Service, call
from todo.command import (
    AddItemCommand,
    AddItemResult,
    Handlers,
    ItemView,
    ListItemsCommand,
    ListItemsResult,
    PlanItemsCommand,
    PlanItemsResult,
)
from todo.command.pipeline import Handler, Kind, KindError
from todo.gen.api.todo import todo_pb2, todo_pb2_grpc


class ItemsHandler:
    """The ItemsService of the contract: the command handlers its methods
    call. from_handlers takes them from the command registry; service()
    registers it."""

    def __init__(
        self,
        plan_items: Handler[PlanItemsCommand, PlanItemsResult],
        add_item: Handler[AddItemCommand, AddItemResult],
        list_items: Handler[ListItemsCommand, ListItemsResult],
    ) -> None:
        self.plan_items = plan_items
        self.add_item = add_item
        self.list_items = list_items

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "ItemsHandler":
        """Takes the handlers of the ItemsService's commands from the command registry."""
        return cls(
            plan_items=handlers.plan_items,
            add_item=handlers.add_item,
            list_items=handlers.list_items,
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

    async def AddItem(self, request: todo_pb2.AddItemRequest, context: grpc.aio.ServicerContext[Any, Any]) -> todo_pb2.AddItemResponse:
        return await call(self._handler.add_item, request, decode_add_item, encode_add_item)

    async def ListItems(self, request: todo_pb2.ListItemsRequest, context: grpc.aio.ServicerContext[Any, Any]) -> todo_pb2.ListItemsResponse:
        return await call(self._handler.list_items, request, decode_list_items, encode_list_items)

    async def ClearList(self, request: todo_pb2.ClearListRequest, context: grpc.aio.ServicerContext[Any, Any]) -> todo_pb2.ClearListResponse:
        # Deprecated since 1.1.0: no command, the same answer as its HTTP form.
        raise KindError(Kind.UNIMPLEMENTED, "ClearList is deprecated since 1.1.0")


def decode_plan_items(request: todo_pb2.PlanItemsRequest) -> PlanItemsCommand:
    return PlanItemsCommand(titles=list(request.titles))


def encode_plan_items(result: PlanItemsResult) -> todo_pb2.PlanItemsResponse:
    return todo_pb2.PlanItemsResponse(items=item_messages(result.items))


def decode_add_item(request: todo_pb2.AddItemRequest) -> AddItemCommand:
    return AddItemCommand(list_id=request.list_id, title=request.title)


def encode_add_item(result: AddItemResult) -> todo_pb2.AddItemResponse:
    return todo_pb2.AddItemResponse(item=todo_pb2.Item(title=result.item.title, done=result.item.done))


def decode_list_items(request: todo_pb2.ListItemsRequest) -> ListItemsCommand:
    return ListItemsCommand(list_id=request.list_id)


def encode_list_items(result: ListItemsResult) -> todo_pb2.ListItemsResponse:
    return todo_pb2.ListItemsResponse(items=item_messages(result.items))


def item_messages(views: list[ItemView]) -> list[todo_pb2.Item]:
    """The command layer's items as the contract's messages."""
    return [todo_pb2.Item(title=view.title, done=view.done) for view in views]
