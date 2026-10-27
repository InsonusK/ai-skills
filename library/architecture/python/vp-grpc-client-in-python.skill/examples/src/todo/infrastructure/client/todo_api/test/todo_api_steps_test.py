import asyncio
import logging
import subprocess
import sys
import tempfile
from dataclasses import dataclass, field
from datetime import timedelta
from pathlib import Path
from typing import Any

import grpc
import pytest
from google.protobuf import descriptor_pb2, json_format
from pytest_bdd import given, parsers, scenarios, then, when

from todo.gen.client.todo import todo_pb2, todo_pb2_grpc
from todo.infrastructure.client.core.grpcclient import MOCK, GrpcConfig
from todo.infrastructure.client.todo_api import METHODS, Item, open_api

scenarios("../features/plan_items.feature", "../features/contract.feature")

# The folder of the copied contract, from this file's folder up to the example's root.
CONTRACT = Path(__file__).parents[6] / "docs" / "integration" / "client" / "todo"


@dataclass
class ScenarioTodoApi(todo_pb2_grpc.ItemsServiceServicer):
    """The todo API as a scenario sets it: it answers PlanItems with the scenario's answer and records what it received."""

    answer: todo_pb2.PlanItemsResponse = field(default_factory=todo_pb2.PlanItemsResponse)
    received: tuple[str, todo_pb2.PlanItemsRequest] | None = None

    async def PlanItems(self, request: todo_pb2.PlanItemsRequest, context: grpc.aio.ServicerContext[Any, Any]) -> todo_pb2.PlanItemsResponse:
        self.received = ("/todo.v1.ItemsService/PlanItems", request)
        return self.answer


@dataclass
class World:
    api: ScenarioTodoApi = field(default_factory=ScenarioTodoApi)
    mocked: bool = False
    items: list[Item] | None = None
    error: Exception | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse("the todo API answers '{answer}'"))
def the_todo_api_answers(world: World, answer: str) -> None:
    world.api.answer = json_format.Parse(answer, todo_pb2.PlanItemsResponse())


@given("the todo API is mocked")
def the_todo_api_is_mocked(world: World) -> None:
    world.mocked = True


@when(parsers.parse('I call PlanItems with the titles "{titles}"'))
def i_call_plan_items(world: World, titles: str) -> None:
    async def call() -> list[Item]:
        """The adapter against a gRPC server of the scenario's todo API, or, mocked, against its own emulator."""
        logger = logging.Logger("test", logging.CRITICAL)
        server = None
        if world.mocked:
            api = await open_api(GrpcConfig(name="todo-api", host=MOCK, port=80, timeout=timedelta(seconds=1)), logger)
        else:
            server = grpc.aio.server()
            todo_pb2_grpc.add_ItemsServiceServicer_to_server(world.api, server)
            port = server.add_insecure_port("127.0.0.1:0")
            await server.start()
            api = await open_api(GrpcConfig(name="todo-api", host="127.0.0.1", port=port, timeout=timedelta(seconds=5)), logger)
        try:
            return await api.plan_items(titles.split("|"))
        finally:
            await api.aclose()
            if server is not None:
                await server.stop(None)

    try:
        world.items = asyncio.run(call())
    except Exception as error:
        world.error = error
    print(f"when: PlanItems {titles} -> {world.items}, error {world.error!r}")


@then(parsers.parse("the todo API should have received \"{method}\" with '{request}'"))
def the_todo_api_received(world: World, method: str, request: str) -> None:
    assert world.api.received == (method, json_format.Parse(request, todo_pb2.PlanItemsRequest())), world.api.received


@then(parsers.parse('the items should be "{items}"'))
def the_items_should_be(world: World, items: str) -> None:
    assert world.items is not None, world.error
    assert "|".join(f"{item.title}:{'done' if item.done else 'open'}" for item in world.items) == items


def _compiled_contract() -> descriptor_pb2.FileDescriptorProto:
    """The copied proto file compiled now, with the import path make proto-gen
    gives it, so the scenarios read the contract, not the code generated from
    it some time ago."""
    with tempfile.TemporaryDirectory() as out:
        descriptors = Path(out) / "contract.pb"
        subprocess.run(
            [sys.executable, "-m", "grpc_tools.protoc", f"-Itodo/gen/client/todo={CONTRACT}", f"--descriptor_set_out={descriptors}", str(CONTRACT / "todo.proto")],
            check=True,
        )
        files = descriptor_pb2.FileDescriptorSet.FromString(descriptors.read_bytes())
    [file] = files.file
    return file


@then("the adapter should call exactly these methods, each in the contract:")
def the_adapter_calls_exactly(datatable: list[list[str]]) -> None:
    assert [row[0] for row in datatable[1:]] == METHODS
    contract = _compiled_contract()
    methods = [f"/{contract.package}.{service.name}/{method.name}" for service in contract.service for method in service.method]
    print(f"then: adapter {METHODS}, contract {methods}")
    assert all(method in methods for method in METHODS), (METHODS, methods)


@then("the code in src/todo/gen/client/todo should be generated from docs/integration/client/todo/todo.proto")
def the_code_is_generated_from_the_contract() -> None:
    generated = descriptor_pb2.FileDescriptorProto()
    todo_pb2.DESCRIPTOR.CopyToProto(generated)
    contract = _compiled_contract()
    # json_name is derived by every compiler, kept by some: no part of the contract.
    for described in (generated, contract):
        for message in described.message_type:
            for message_field in message.field:
                message_field.ClearField("json_name")
    assert generated == contract, "src/todo/gen/client/todo is not generated from the copied contract; run make proto-gen"
