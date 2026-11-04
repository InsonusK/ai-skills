import asyncio
import logging
from collections.abc import Awaitable, Callable
from dataclasses import dataclass, field

import grpc
import pytest
from google.protobuf.message import Message
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.grpc import Endpoints, services
from todo.api.grpc.endpoint import ItemsHandler
from todo.api.grpc.pipeline import GrpcConfig, Server
from todo.command import (
    AddItemCommand,
    AddItemResult,
    CompleteItemCommand,
    CompleteItemResult,
    ItemView,
    ListItemsCommand,
    ListItemsResult,
    PlanItemsCommand,
    PlanItemsResult,
)
from todo.command.pipeline import Kind, KindError
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
class FakeAddItem:
    """AddItem faked: the added item is the title it received, or the error it raises."""

    error: KindError | None = None
    received: list[AddItemCommand] = field(default_factory=list)

    async def handle(self, command: AddItemCommand) -> AddItemResult:
        self.received.append(command)
        if self.error is not None:
            raise self.error
        return AddItemResult(item=ItemView(title=command.title, done=False))


@dataclass
class FakeListItems:
    """ListItems faked: the items it returns, and the commands it received."""

    titles: list[str] = field(default_factory=list)
    received: list[ListItemsCommand] = field(default_factory=list)

    async def handle(self, command: ListItemsCommand) -> ListItemsResult:
        self.received.append(command)
        return ListItemsResult(items=[ItemView(title=title, done=False) for title in self.titles])


@dataclass
class FakeCompleteItem:
    """CompleteItem faked: the item done at the version after the caller's,
    or the error it raises; the commands it received."""

    error: KindError | None = None
    received: list[CompleteItemCommand] = field(default_factory=list)

    async def handle(self, command: CompleteItemCommand) -> CompleteItemResult:
        self.received.append(command)
        if self.error is not None:
            raise self.error
        return CompleteItemResult(item=ItemView(title="buy milk", done=True, id=command.item_id, version=command.version + "+1"))


@dataclass
class World:
    plan_items: FakePlanItems = field(default_factory=FakePlanItems)
    add_item: FakeAddItem = field(default_factory=FakeAddItem)
    list_items: FakeListItems = field(default_factory=FakeListItems)
    complete_item: FakeCompleteItem = field(default_factory=FakeCompleteItem)
    response: Message | None = None
    error: grpc.aio.AioRpcError | None = None

    # ItemsServiceAsyncStub exists only in the type stub: the annotation is a string.
    def call(self, method: "Callable[[todo_pb2_grpc.ItemsServiceAsyncStub], Awaitable[Message]]") -> None:
        # The real server with ItemsService over the fake handlers, called
        # through the generated client.
        async def scenario() -> Message:
            handler = ItemsHandler(
                plan_items=self.plan_items, add_item=self.add_item, list_items=self.list_items, complete_item=self.complete_item
            )
            server, port = await Server(GrpcConfig(listen_port=0), services(Endpoints(items=handler)), logging.Logger("test")).start()
            try:
                async with grpc.aio.insecure_channel(f"127.0.0.1:{port}") as channel:
                    return await method(todo_pb2_grpc.ItemsServiceStub(channel))
            finally:
                await server.stop(None)

        try:
            self.response = asyncio.run(scenario())
        except grpc.aio.AioRpcError as error:
            self.error = error
        print(f"when: -> {self.response}, error {self.error and (self.error.code(), self.error.details())}")

    def answered[TMessage: Message](self, kind: type[TMessage]) -> TMessage:
        assert isinstance(self.response, kind), (self.response, self.error)
        return self.response


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.re(r'PlanItems returns the items "(?P<titles>[^"]*)"'))
def plan_items_returns(world: World, titles: str) -> None:
    world.plan_items.titles = titles.split("|") if titles else []


@given("AddItem fails with the kind conflict")
def add_item_fails_with_a_conflict(world: World) -> None:
    world.add_item.error = KindError(Kind.CONFLICT, "the title is taken")


@given(parsers.parse('ListItems holds "{titles}"'))
def list_items_holds(world: World, titles: str) -> None:
    world.list_items.titles = titles.split("|")


@when(parsers.parse('I call ItemsService.PlanItems with the titles "{titles}"'))
def i_call_plan_items(world: World, titles: str) -> None:
    world.call(lambda stub: stub.PlanItems(todo_pb2.PlanItemsRequest(titles=titles.split("|"))))


@when(parsers.parse('I call ItemsService.AddItem with the list "{list_id}" and the title "{title}"'))
def i_call_add_item(world: World, list_id: str, title: str) -> None:
    world.call(lambda stub: stub.AddItem(todo_pb2.AddItemRequest(list_id=list_id, title=title)))


@when(parsers.parse('I call ItemsService.ListItems with the list "{list_id}"'))
def i_call_list_items(world: World, list_id: str) -> None:
    world.call(lambda stub: stub.ListItems(todo_pb2.ListItemsRequest(list_id=list_id)))


@when(parsers.parse('I call ItemsService.ClearList with the list "{list_id}"'))
def i_call_clear_list(world: World, list_id: str) -> None:
    world.call(lambda stub: stub.ClearList(todo_pb2.ClearListRequest(list_id=list_id)))


@then(parsers.parse('the response should hold the items "{titles}", none done'))
def the_response_should_hold(world: World, titles: str) -> None:
    items = world.answered(todo_pb2.PlanItemsResponse).items
    assert [(item.title, item.done) for item in items] == [(title, False) for title in titles.split("|")]


@then(parsers.parse('PlanItems should have received the titles "{titles}"'))
def plan_items_should_have_received(world: World, titles: str) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=titles.split("|"))]


@then(parsers.parse('the added item should be "{title}"'))
def the_added_item_should_be(world: World, title: str) -> None:
    item = world.answered(todo_pb2.AddItemResponse).item
    assert (item.title, item.done) == (title, False)


@then(parsers.parse('AddItem should have received the list "{list_id}" and the title "{title}"'))
def add_item_should_have_received(world: World, list_id: str, title: str) -> None:
    assert world.add_item.received == [AddItemCommand(list_id=list_id, title=title)]


@then(parsers.parse('the listed items should be "{titles}" for the list "{list_id}"'))
def the_listed_items_should_be(world: World, titles: str, list_id: str) -> None:
    items = world.answered(todo_pb2.ListItemsResponse).items
    assert [item.title for item in items] == titles.split("|")
    assert world.list_items.received == [ListItemsCommand(list_id=list_id)]


@then(parsers.parse('the call should fail with {code} and the message "{message}"'))
def the_call_should_fail_with(world: World, code: str, message: str) -> None:
    assert world.error is not None, world.response
    assert (world.error.code(), world.error.details()) == (grpc.StatusCode[code], message)


@when("I call ItemsService.PlanItems with no titles")
def i_call_plan_items_with_no_titles(world: World) -> None:
    world.call(lambda stub: stub.PlanItems(todo_pb2.PlanItemsRequest()))


@then("PlanItems should have received no titles")
def plan_items_should_have_received_no_titles(world: World) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=[])]


@given("CompleteItem fails with the kind concurrent change")
def complete_item_fails_with_a_concurrent_change(world: World) -> None:
    world.complete_item.error = KindError(Kind.CONCURRENT_CHANGE, "the item was changed since it was read: read it again")


@when(parsers.parse('I call ItemsService.CompleteItem with the list "{list_id}", the item {item_id:d}, and the version "{version}"'))
def i_call_complete_item(world: World, list_id: str, item_id: int, version: str) -> None:
    world.call(lambda stub: stub.CompleteItem(todo_pb2.CompleteItemRequest(list_id=list_id, item_id=item_id, version=version)))


@then(parsers.parse('the completed item should be done, with the id {item_id:d} and the version "{version}"'))
def the_completed_item_should_be_done(world: World, item_id: int, version: str) -> None:
    item = world.answered(todo_pb2.CompleteItemResponse).item
    assert (item.done, item.id, item.version) == (True, item_id, version), item


@then(parsers.parse('CompleteItem should have received the list "{list_id}", the item {item_id:d}, and the version "{version}"'))
def complete_item_should_have_received(world: World, list_id: str, item_id: int, version: str) -> None:
    assert world.complete_item.received == [CompleteItemCommand(list_id=list_id, item_id=item_id, version=version)]
