"""The lists resource (/v1/lists/{listId}/...) of the HTTP form of the proto
contract. The path parameter is named as the generated spec names it -
listId, the lowerCamelCase form of the proto field list_id - and each route
takes it by that alias, since it is not in the body."""

from typing import Annotated

from fastapi import APIRouter, Path, Request, Response

from todo.api.http.endpoint.items import item_messages
from todo.api.http.pipeline.proto import decode_proto, proto_json, unimplemented
from todo.command import AddItemCommand, AddItemResult, Handlers, ListItemsCommand, ListItemsResult
from todo.command.pipeline import Handler
from todo.gen.api.todo import todo_pb2

type ListId = Annotated[str, Path(alias="listId")]


class ListsHandler:
    """The lists resource: the command handlers its routes call.
    from_handlers takes them from the command registry; router() lists its
    operations."""

    def __init__(
        self,
        add_item: Handler[AddItemCommand, AddItemResult],
        list_items: Handler[ListItemsCommand, ListItemsResult],
    ) -> None:
        self._add_item = add_item
        self._list_items = list_items

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "ListsHandler":
        """Takes the handlers of the lists commands from the command registry."""
        return cls(
            add_item=handlers.add_item,
            list_items=handlers.list_items,
        )

    def router(self) -> APIRouter:
        router = APIRouter()

        # 201: the item is created and kept.
        @router.post("/v1/lists/{listId}/items")
        async def add_item(list_id: ListId, request: Request) -> Response:
            message = await decode_proto(request, todo_pb2.AddItemRequest())
            result = await self._add_item.handle(AddItemCommand(list_id=list_id, title=message.title))
            item = todo_pb2.Item(title=result.item.title, done=result.item.done)
            return proto_json(todo_pb2.AddItemResponse(item=item), 201)

        # A GET has no body: the command is read from the path only.
        @router.get("/v1/lists/{listId}/items")
        async def list_items(list_id: ListId) -> Response:
            result = await self._list_items.handle(ListItemsCommand(list_id=list_id))
            return proto_json(todo_pb2.ListItemsResponse(items=item_messages(result.items)), 200)

        # ClearList is deprecated since 1.1.0: no command, the same answer as its gRPC form.
        @router.post("/v1/lists/{listId}/clear")
        async def clear_list(list_id: ListId) -> Response:
            unimplemented("ClearList is deprecated since 1.1.0")

        return router
