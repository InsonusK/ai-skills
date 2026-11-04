"""The items resource (/v1/items/...) of the HTTP form of the proto
contract: its command handlers, its routes, and per route the mapping of the
rpc's generated messages to and from the command. A new rpc on items adds a
handler, its line of from_handlers, a route, and its mappings here."""

from fastapi import APIRouter, Request, Response

from todo.api.http.pipeline.proto import decode_proto, proto_json
from todo.command import Handlers, ItemView, PlanItemsCommand, PlanItemsResult
from todo.command.pipeline import Handler
from todo.gen.api.todo import todo_pb2


class ItemsHandler:
    """The items resource: the command handlers its routes call.
    from_handlers takes them from the command registry; router() lists its
    operations."""

    def __init__(self, plan_items: Handler[PlanItemsCommand, PlanItemsResult]) -> None:
        self._plan_items = plan_items

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "ItemsHandler":
        """Takes the handlers of the items commands from the command registry."""
        return cls(
            plan_items=handlers.plan_items,
        )

    def router(self) -> APIRouter:
        router = APIRouter()

        # 200: planning creates nothing that is kept, and its work is done.
        @router.post("/v1/items/plan")
        async def plan_items(request: Request) -> Response:
            message = await decode_proto(request, todo_pb2.PlanItemsRequest())
            result = await self._plan_items.handle(PlanItemsCommand(titles=list(message.titles)))
            return proto_json(todo_pb2.PlanItemsResponse(items=item_messages(result.items)), 200)

        return router


def item_messages(views: list[ItemView]) -> list[todo_pb2.Item]:
    """The command layer's items as the contract's messages."""
    return [todo_pb2.Item(title=view.title, done=view.done) for view in views]
