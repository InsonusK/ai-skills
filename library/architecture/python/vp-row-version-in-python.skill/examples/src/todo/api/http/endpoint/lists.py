"""The lists resource (/v1/lists/{listId}/...) of the HTTP form of the proto
contract. The path parameter is named as the generated spec names it -
listId, the lowerCamelCase form of the proto field list_id - and each route
takes it by that alias, since it is not in the body."""

from typing import Annotated

from fastapi import APIRouter, Path, Request, Response

from todo.api.http.endpoint.items import item_message, item_messages
from todo.api.http.pipeline.proto import decode_proto, proto_json, unimplemented
from todo.command import (
    AddItemCommand,
    AddItemResult,
    CompleteItemCommand,
    CompleteItemResult,
    Handlers,
    ListItemsCommand,
    ListItemsResult,
)
from todo.command.pipeline import Handler, Kind, KindError
from todo.gen.api.todo import todo_pb2

type ListId = Annotated[str, Path(alias="listId")]
type ItemId = Annotated[str, Path(alias="itemId")]


class ListsHandler:
    """The lists resource: the command handlers its routes call.
    from_handlers takes them from the command registry; router() lists its
    operations."""

    def __init__(
        self,
        add_item: Handler[AddItemCommand, AddItemResult],
        list_items: Handler[ListItemsCommand, ListItemsResult],
        complete_item: Handler[CompleteItemCommand, CompleteItemResult],
    ) -> None:
        self._add_item = add_item
        self._list_items = list_items
        self._complete_item = complete_item

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "ListsHandler":
        """Takes the handlers of the lists commands from the command registry."""
        return cls(
            add_item=handlers.add_item,
            list_items=handlers.list_items,
            complete_item=handlers.complete_item,
        )

    def router(self) -> APIRouter:
        router = APIRouter()

        # 201: the item is created and kept.
        @router.post("/v1/lists/{listId}/items")
        async def add_item(list_id: ListId, request: Request) -> Response:
            message = await decode_proto(request, todo_pb2.AddItemRequest())
            result = await self._add_item.handle(AddItemCommand(list_id=list_id, title=message.title))
            return proto_json(todo_pb2.AddItemResponse(item=item_message(result.item)), 201)

        # A GET has no body: the command is read from the path only.
        @router.get("/v1/lists/{listId}/items")
        async def list_items(list_id: ListId) -> Response:
            result = await self._list_items.handle(ListItemsCommand(list_id=list_id))
            return proto_json(todo_pb2.ListItemsResponse(items=item_messages(result.items)), 200)

        # The item from the path, the version from the body. An item id that is
        # no number is a request the contract cannot decode: 400.
        @router.post("/v1/lists/{listId}/items/{itemId}/complete")
        async def complete_item(list_id: ListId, item_id: ItemId, request: Request) -> Response:
            message = await decode_proto(request, todo_pb2.CompleteItemRequest())
            if not item_id.isdigit():
                raise KindError(Kind.INVALID, f'item id "{item_id}" is not a number')
            command = CompleteItemCommand(list_id=list_id, item_id=int(item_id), version=message.version)
            result = await self._complete_item.handle(command)
            return proto_json(todo_pb2.CompleteItemResponse(item=item_message(result.item)), 200)

        # ClearList is deprecated since 1.1.0: no command, the same answer as its gRPC form.
        @router.post("/v1/lists/{listId}/clear")
        async def clear_list(list_id: ListId) -> Response:
            unimplemented("ClearList is deprecated since 1.1.0")

        return router
