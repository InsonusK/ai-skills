"""The items resource (/v1/items/...): its command handlers, its routes, and
the request and response types of each route with their mappings. A new
command on items adds a handler, a route, and its types here, and its path in
openapi.json."""

from fastapi import APIRouter
from pydantic import BaseModel

from todo.command import PlanItemsCommand, PlanItemsResult
from todo.command.pipeline import Handler


class PlanItemsRequest(BaseModel):
    """The body of POST /v1/items/plan. Unknown fields are ignored, so a
    client built against a newer minor version of the contract still works."""

    titles: list[str] = []


class ItemResponse(BaseModel):
    title: str
    done: bool


class PlanItemsResponse(BaseModel):
    """The answer of POST /v1/items/plan."""

    items: list[ItemResponse]

    @classmethod
    def of(cls, result: PlanItemsResult) -> "PlanItemsResponse":
        return cls(items=[ItemResponse(title=item.title, done=item.done) for item in result.items])


class ItemsHandler:
    """The items resource: the command handlers its routes call. The
    composition root builds it; router() lists its operations."""

    def __init__(self, plan_items: Handler[PlanItemsCommand, PlanItemsResult]) -> None:
        self._plan_items = plan_items

    def router(self) -> APIRouter:
        router = APIRouter()

        # 200: planning creates nothing that is kept, and its work is done.
        @router.post("/v1/items/plan", status_code=200)
        async def plan_items(body: PlanItemsRequest) -> PlanItemsResponse:
            result = await self._plan_items.handle(PlanItemsCommand(titles=body.titles))
            return PlanItemsResponse.of(result)

        return router
