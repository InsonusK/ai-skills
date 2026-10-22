import json
import logging
from dataclasses import dataclass, field

import pytest
from fastapi.testclient import TestClient
from httpx2 import Response
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.http import Endpoints, routes
from todo.api.http.endpoint import ItemsHandler
from todo.api.http.pipeline import HttpConfig, Server
from todo.command import ItemView, PlanItemsCommand, PlanItemsResult

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
    response: Response | None = None


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.re(r'PlanItems returns the items "(?P<titles>[^"]*)"'))
def plan_items_returns(world: World, titles: str) -> None:
    world.plan_items.titles = titles.split("|") if titles else []


@when(parsers.re(r"I POST \"(?P<path>[^\"]+)\" with the body '(?P<body>[^']*)'$"))
def i_post(world: World, path: str, body: str) -> None:
    app = Server(HttpConfig(), routes(Endpoints(items=ItemsHandler(plan_items=world.plan_items))), logging.Logger("test")).app()
    with TestClient(app) as client:
        world.response = client.post(path, content=body, headers={"Content-Type": "application/json"})
    print(f"when: POST {path} -> {world.response.status_code} {world.response.text}")


@then(parsers.parse("the response status should be {status:d}"))
def the_response_status_should_be(world: World, status: int) -> None:
    assert world.response is not None and world.response.status_code == status


@then(parsers.re(r"the response body should be '(?P<body>[^']*)'$"))
def the_response_body_should_be(world: World, body: str) -> None:
    assert world.response is not None and world.response.json() == json.loads(body)


@then(parsers.parse('PlanItems should have received the titles "{titles}"'))
def plan_items_should_have_received(world: World, titles: str) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=titles.split("|"))]


@then("PlanItems should have received no titles")
def plan_items_should_have_received_no_titles(world: World) -> None:
    assert world.plan_items.received == [PlanItemsCommand(titles=[])]
