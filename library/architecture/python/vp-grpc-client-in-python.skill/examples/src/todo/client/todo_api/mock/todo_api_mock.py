"""The mock of the todo API's client (todo_api): an emulator of the API inside
the process, which TODO_API_HOST=mock runs the client against, so the
application runs with no todo service reachable."""

from typing import Any

import grpc

from todo.gen.client.todo import todo_pb2, todo_pb2_grpc


class _ItemsService(todo_pb2_grpc.ItemsServiceServicer):
    """ItemsService inside the process; a method it does not write answers UNIMPLEMENTED."""

    async def PlanItems(self, request: todo_pb2.PlanItemsRequest, context: grpc.aio.ServicerContext[Any, Any]) -> todo_pb2.PlanItemsResponse:
        if not request.titles:
            await context.abort(grpc.StatusCode.INVALID_ARGUMENT, "no items to plan")
        return todo_pb2.PlanItemsResponse(items=[todo_pb2.Item(title=title, done=False) for title in request.titles])


def emulator(server: grpc.aio.Server) -> None:
    """Adds the todo API's services to server, answering every method of
    todo_api.METHODS as the todo API does, with stable data and no state. A
    new method of the adapter adds its method to _ItemsService."""
    todo_pb2_grpc.add_ItemsServiceServicer_to_server(_ItemsService(), server)
