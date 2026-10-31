import asyncio
import json
import logging
import threading
from collections.abc import Iterator
from dataclasses import dataclass, field
from datetime import timedelta
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.core.httpclient import MOCK, HttpConfig, Operation
from todo.infrastructure.client.todo_api import OPERATIONS, Item, TodoApi

scenarios("../features/plan_items.feature", "../features/contract.feature")

# The copied contract, from this file's folder up to the example's root.
CONTRACT = Path(__file__).parents[6] / "docs" / "integration" / "client" / "todo" / "openapi.json"


@dataclass
class World:
    answer: str = ""
    mocked: bool = False
    received: tuple[str, str, str] | None = None
    server: ThreadingHTTPServer | None = None
    items: list[Item] | None = None
    error: Exception | None = None

    def client(self) -> TodoApi:
        """The adapter against an HTTP server of the todo API serving the
        scenario's answer, or, mocked, against its own emulator."""
        logger = logging.Logger("test", logging.CRITICAL)
        if self.mocked:
            return TodoApi(HttpConfig(name="todo-api", host=MOCK, port=80, timeout=timedelta(seconds=1)), logger, [])
        world = self

        class Handler(BaseHTTPRequestHandler):
            def do_POST(self) -> None:
                body = self.rfile.read(int(self.headers.get("Content-Length") or 0)).decode()
                world.received = (self.command, self.path, body)
                answer = world.answer.encode()
                self.send_response(200)
                self.send_header("Content-Type", "application/json")
                self.send_header("Content-Length", str(len(answer)))
                self.end_headers()
                self.wfile.write(answer)

            def log_message(self, format: str, *args: object) -> None:
                pass

        self.server = ThreadingHTTPServer(("127.0.0.1", 0), Handler)
        threading.Thread(target=self.server.serve_forever, daemon=True).start()
        port = int(self.server.server_address[1])
        return TodoApi(HttpConfig(name="todo-api", host="127.0.0.1", port=port, timeout=timedelta(seconds=1)), logger, [])


@pytest.fixture
def world() -> Iterator[World]:
    world = World()
    yield world
    if world.server is not None:
        world.server.shutdown()
        world.server.server_close()


@given(parsers.parse("the todo API answers '{answer}'"))
def the_todo_api_answers(world: World, answer: str) -> None:
    world.answer = answer


@given("the todo API is mocked")
def the_todo_api_is_mocked(world: World) -> None:
    world.mocked = True


@when(parsers.parse('I call PlanItems with the titles "{titles}"'))
def i_call_plan_items(world: World, titles: str) -> None:
    async def call() -> list[Item]:
        client = world.client()
        try:
            return await client.plan_items(titles.split("|"))
        finally:
            await client.aclose()

    try:
        world.items = asyncio.run(call())
    except Exception as error:
        world.error = error
    print(f"when: PlanItems {titles} -> {world.items}, error {world.error!r}")


@then(parsers.parse("the todo API should have received {method} \"{path}\" with '{body}'"))
def the_todo_api_received(world: World, method: str, path: str, body: str) -> None:
    assert world.received is not None
    assert (world.received[0], world.received[1], json.loads(world.received[2])) == (method, path, json.loads(body))


@then(parsers.parse('the items should be "{items}"'))
def the_items_should_be(world: World, items: str) -> None:
    assert world.items is not None, world.error
    assert "|".join(f"{item.title}:{'done' if item.done else 'open'}" for item in world.items) == items


@then("the adapter should call exactly these operations, each in the contract:")
def the_adapter_calls_exactly(datatable: list[list[str]]) -> None:
    assert [Operation(row[0], row[1], row[2] == "yes") for row in datatable[1:]] == OPERATIONS
    paths = json.loads(CONTRACT.read_text())["paths"]
    for operation in OPERATIONS:
        assert operation.method.lower() in paths.get(operation.path, {}), f"{operation} is no operation of the contract"
