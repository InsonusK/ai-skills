"""The proto-JSON form: routes that read and write a generated message
(google.protobuf.Method stands in for a contract's), served beside echo."""

from fastapi import APIRouter, Request, Response
from google.protobuf import api_pb2
from pytest_bdd import given, scenarios

from todo.api.http.pipeline.proto import decode_proto, proto_json, unimplemented

from .conftest import World

scenarios("../features/proto-json.feature")


@given("the HTTP routes read and write proto messages")
def the_routes_read_and_write_proto_messages(world: World) -> None:
    router = APIRouter()

    @router.post("/v1/proto")
    async def proto(request: Request) -> Response:
        message = await decode_proto(request, api_pb2.Method())
        answer = await world.echo.handle(message.request_type_url)
        return proto_json(api_pb2.Method(response_type_url=answer), 200)

    @router.post("/v1/old")
    async def old() -> Response:
        unimplemented("Old is deprecated since 1.1.0")

    world.routers.append(router)
