import json
import logging
from dataclasses import dataclass, field

import pytest
from fastapi.testclient import TestClient
from httpx2 import Response
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.http import Endpoints, routes
from todo.api.http.access import authenticate
from todo.api.http.pipeline import HttpConfig, Server
from todo.command import build_handlers
from todo.command.access import load_config, with_access_check, with_batch_limit
from todo.command.pipeline import with_logging

scenarios("../features/access.feature")


class Recorder(logging.Handler):
    """Keeps every record the server and the decorators wrote."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class World:
    recorder: Recorder = field(default_factory=Recorder)
    client: TestClient | None = None
    response: Response | None = None

    def logger(self) -> logging.Logger:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        return logger

    def answered(self) -> Response:
        assert self.response is not None, "no request was sent"
        return self.response


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the callers "{tokens}" and the batch limit {limit:d}'))
def the_callers(world: World, monkeypatch: pytest.MonkeyPatch, tokens: str, limit: int) -> None:
    # The application as main.py builds it with the access variation point
    # attached - the same decorators and middlewares in the same order, over
    # the real command registry and routes - served in memory.
    monkeypatch.setenv("ACCESS_TOKENS", tokens)
    settings = load_config()
    logger = world.logger()
    handlers = build_handlers([with_logging(logger), with_access_check(), with_batch_limit(limit)])
    server = Server(HttpConfig(), routes(Endpoints.from_handlers(handlers)), logger, [authenticate(settings.tokens)])
    world.client = TestClient(server.app(), raise_server_exceptions=False)


def _post(world: World, token: str, path: str, body: str) -> None:
    assert world.client is not None
    headers = {"Content-Type": "application/json"} | ({"Authorization": f"Bearer {token}"} if token else {})
    world.response = world.client.post(path, content=body, headers=headers)
    print(f"when: POST {path} {body} as {token!r} -> {world.response.status_code} {world.response.text}")


@when(parsers.re(r"\"(?P<token>[^\"]+)\" POSTs \"(?P<path>[^\"]+)\" with the body '(?P<body>[^']*)'"))
def a_caller_posts(world: World, token: str, path: str, body: str) -> None:
    _post(world, token, path, body)


@when(parsers.re(r"nobody POSTs \"(?P<path>[^\"]+)\" with the body '(?P<body>[^']*)'"))
def nobody_posts(world: World, path: str, body: str) -> None:
    _post(world, "", path, body)


@then(parsers.parse("the response status should be {status:d}"))
def the_response_status_should_be(world: World, status: int) -> None:
    assert world.answered().status_code == status


@then(parsers.parse('the error body should have code {code:d} and the message "{message}"'))
def the_error_body_should_be(world: World, code: int, message: str) -> None:
    body = json.loads(world.answered().text)
    assert (body["code"], body["message"]) == (code, message)


@then(parsers.parse('the records should be, in order, "{messages}"'))
def the_records_should_be_in_order(world: World, messages: str) -> None:
    assert "|".join(record.getMessage() for record in world.recorder.records) == messages
