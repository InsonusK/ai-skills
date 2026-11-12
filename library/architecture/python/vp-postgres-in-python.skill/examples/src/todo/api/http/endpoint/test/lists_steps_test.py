import json
import logging
from dataclasses import dataclass, field

import pytest
from fastapi.testclient import TestClient
from httpx2 import Response
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.http.endpoint import ListsHandler
from todo.api.http.pipeline import HttpConfig, Server
from todo.command import AddItemCommand, AddItemResult, ItemView, ListItemsCommand, ListItemsResult
from todo.command.pipeline import Kind, KindError

scenarios("../features/lists.feature")


@dataclass
class FakeAddItem:
    """AddItem faked: the item it returns or the error it raises, and the commands it received."""

    title: str = ""
    error: KindError | None = None
    received: list[AddItemCommand] = field(default_factory=list)

    async def handle(self, command: AddItemCommand) -> AddItemResult:
        self.received.append(command)
        if self.error is not None:
            raise self.error
        return AddItemResult(item=ItemView(title=self.title, done=False))


@dataclass
class FakeListItems:
    """ListItems faked: the items it returns, and the commands it received."""

    titles: list[str] = field(default_factory=list)
    received: list[ListItemsCommand] = field(default_factory=list)

    async def handle(self, command: ListItemsCommand) -> ListItemsResult:
        self.received.append(command)
        return ListItemsResult(items=[ItemView(title=title, done=False) for title in self.titles])


@dataclass
class World:
    add_item: FakeAddItem = field(default_factory=FakeAddItem)
    list_items: FakeListItems = field(default_factory=FakeListItems)
    response: Response | None = None

    def request(self, method: str, path: str, body: str | None = None) -> None:
        router = ListsHandler(add_item=self.add_item, list_items=self.list_items).router()
        app = Server(HttpConfig(), [router], logging.Logger("test")).app()
        with TestClient(app) as client:
            self.response = client.request(method, path, content=body, headers={"Content-Type": "application/json"} if body is not None else {})
        print(f"when: {method} {path} -> {self.response.status_code} {self.response.text}")

    def answered(self) -> Response:
        assert self.response is not None, "no request was sent"
        return self.response


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('AddItem returns the item "{title}"'))
def add_item_returns(world: World, title: str) -> None:
    world.add_item.title = title


@given(parsers.re(r"AddItem fails with the kind \"(?P<kind>[^\"]+)\" and '(?P<message>[^']+)'$"))
def add_item_fails_with_the_kind(world: World, kind: str, message: str) -> None:
    world.add_item.error = KindError(Kind(kind), message)


@given("the HTTP API serves the application's routes")
def the_api_serves_the_application_routes(world: World) -> None:
    # Every scenario here calls the application's own routes over fake commands.
    pass


@given(parsers.parse('ListItems returns the items "{titles}"'))
def list_items_returns(world: World, titles: str) -> None:
    world.list_items.titles = titles.split("|")


@when(parsers.re(r"I POST \"(?P<path>[^\"]+)\" to the HTTP API with the body '(?P<body>[^']*)'$"))
def i_post(world: World, path: str, body: str) -> None:
    world.request("POST", path, body)


@when(parsers.parse('I GET "{path}" from the HTTP API'))
def i_get(world: World, path: str) -> None:
    world.request("GET", path)


@then(parsers.parse("the HTTP status should be {status:d}"))
def the_response_status_should_be(world: World, status: int) -> None:
    assert world.answered().status_code == status


@then(parsers.re(r"the HTTP error should have code (?P<code>\d+) and the message (?P<quote>[\"'])(?P<message>.*)(?P=quote)$"))
def the_error_should_have(world: World, code: str, message: str) -> None:
    assert world.answered().json() == {"code": int(code), "message": message, "details": []}


@then(parsers.re(r"the HTTP response body should be '(?P<body>[^']*)'$"))
def the_response_body_should_be(world: World, body: str) -> None:
    assert world.answered().json() == json.loads(body)


@then(parsers.parse('AddItem should have received the list "{list_id}" and the title "{title}"'))
def add_item_should_have_received(world: World, list_id: str, title: str) -> None:
    assert world.add_item.received == [AddItemCommand(list_id=list_id, title=title)]


@then(parsers.parse('ListItems should have received the list "{list_id}"'))
def list_items_should_have_received(world: World, list_id: str) -> None:
    assert world.list_items.received == [ListItemsCommand(list_id=list_id)]
