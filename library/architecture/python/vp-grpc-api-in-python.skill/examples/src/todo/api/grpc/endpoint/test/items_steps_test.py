import asyncio
import logging
from dataclasses import dataclass, field

import grpc
import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.grpc import Endpoints, services
from todo.api.grpc.endpoint import ItemsHandler
from todo.api.grpc.pipeline import GrpcConfig, Server
from todo.command import ItemView, PlanItemsCommand, PlanItemsResult
from todo.gen.api.todo import todo_pb2, todo_pb2_grpc

scenarios("../features/items.feature")


@dataclass
class FakePlanItems:
    """PlanItems faked: what it returns, and the commands it received."""

    titles: list[str] = field(default_factory=list)
    received: list[PlanItemsCommand] = field(default_factory=list)

    async def handle(self, command: PlanItemsCommand) -> PlanItemsResult:
        self.received.append(command)
        return PlanItemsResult(items=[ItemView(title=title, done=False) for title in self.titles])


@dataclass
class World:
    plan_items: FakePlanItems = field(default_factory=FakePlanItems)
    response: todo_pb2.PlanItemsResponse | None = None


@pytest.fixture
def world() -> World:
    return World()


def _call_plan_items(world: World, titles: list[str]) -> None:
    # The real server with ItemsService over the fake handler, called through
    # the generated client.
    async def scenario() -> todo_pb2.PlanItemsResponse:
        contract = services(Endpoints(items=ItemsHandler(plan_items=world.plan_items)))
        server, port = await Server(GrpcConfig(listen_port=0), contract, logging.Logger("test")).start()
        try:
            async with grpc.aio.insecure_channel(f"127.0.0.1:{port}") as channel:
                return await todo_pb2_grpc.ItemsServiceStub(channel).PlanItems(todo_pb2.PlanItemsRequest(titles=titles))
        finally:
            await server.stop(None)

    world.response = asyncio.run(scenario())
    print(f"when: PlanItems({titles}) -> {[(item.title, item.done) for item in world.response.items]}")


@given(parsers.re(r'PlanItems returns the items "(?P<titles>[^"]*)"'))
def plan_items_returns(world: World, titles: str) -> None:
    world.plan_items.titles = titles.split("|") if titles else []


@when(parsers.parse('I call ItemsService.PlanItems with the titles "{titles}"'))
def i_call_plan_items(world: World, titles: str) -> None:
    _call_plan_items(world, titles.split("|"))


@when("I call ItemsService.PlanItems with no titles")
def i_call_plan_items_with_no_titles(world: World) -> None:
    _call_plan_items(world, [])


@then(parsers.parse('the response should hold the items "{titles}", none done'))
def the_response_should_hold(world: World, titles: str) -> None:
    assert world.response is not None
    assert [(item.title, item.done) for item in world.response.items] == [(title, False) for title in titles.split("|")]


@then(parsers.parse('PlanItems should have received the titles "{titles}"'))
def plan_items_should_have_received(world: World, titles: str) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=titles.split("|"))]


@then("PlanItems should have received no titles")
def plan_items_should_have_received_no_titles(world: World) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=[])]
