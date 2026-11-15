"""The steps several features of the HTTP pipeline share: a resource "echo"
served by the real server, a fake command behind it, and the answers and log
records of a request."""

import json
import urllib.parse
import logging
import re
from dataclasses import dataclass, field
from typing import Any

import pytest
from fastapi import APIRouter
from fastapi.testclient import TestClient
from httpx2 import Response
from pydantic import BaseModel
from pytest_bdd import given, parsers, then, when

from todo.api.http.pipeline import HttpConfig, Middleware, Server
from todo.api.http.pipeline.docs import find_contract
from todo.command.middleware.logging import LoggingMiddleware
from todo.command.pipeline import Handler, HandlerFunc, Kind, KindError, decorate
from todo.log import attributes_of, request_id


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


class EchoRequest(BaseModel):
    text: str


class EchoResponse(BaseModel):
    text: str


@dataclass
class EchoCommand:
    """What the fake command answers or raises, and what it received."""

    answer: str = ""
    error: BaseException | None = None
    received: list[str] = field(default_factory=list)
    request_ids: list[str | None] = field(default_factory=list)

    async def handle(self, command: str) -> str:
        self.received.append(command)
        self.request_ids.append(request_id())
        if self.error is not None:
            raise self.error
        return self.answer


@dataclass
class World:
    echo: EchoCommand = field(default_factory=EchoCommand)
    handler: Handler[str, str] | None = None
    recorder: Recorder = field(default_factory=Recorder)
    config: HttpConfig = field(default_factory=HttpConfig)
    middlewares: list[Middleware] = field(default_factory=list)
    routers: list[APIRouter] = field(default_factory=list)
    """Resources a feature serves beside echo (proto.feature)."""
    response: Response | None = None

    def logger(self) -> logging.Logger:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        return logger

    def router(self) -> APIRouter:
        # The echo resource: one route of the shape every resource module has.
        handler = self.handler or self.echo
        router = APIRouter()

        @router.post("/v1/echo", status_code=201)
        async def echo(body: EchoRequest) -> EchoResponse:
            return EchoResponse(text=await handler.handle(body.text))

        return router

    def request(self, method: str, path: str, **kwargs: Any) -> None:
        app = Server(self.config, [self.router(), *self.routers], self.logger(), self.middlewares).app()
        with TestClient(app, raise_server_exceptions=False, follow_redirects=False) as client:
            self.response = client.request(method, path, **kwargs)
        print(f"when: {method} {path} -> {self.response.status_code} {self.response.text[:200]!r}")

    def answered(self) -> Response:
        assert self.response is not None, "no request was sent"
        return self.response

    def records(self, message: str) -> list[logging.LogRecord]:
        return [record for record in self.recorder.records if record.getMessage() == message]


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the echo command answers "{answer}"'))
@when(parsers.parse('the echo command answers "{answer}"'))
def the_command_answers(world: World, answer: str) -> None:
    world.echo.answer = answer
    world.echo.error = None


@given(parsers.parse('the echo command is rejected with "{message}"'))
def the_command_is_rejected(world: World, message: str) -> None:
    world.echo.error = KindError(Kind.INVALID, message)


@given(parsers.re(r'the echo command fails with the kind "(?P<kind>[^"]+)" and "(?P<message>[^"]+)"$'))
def the_command_fails_with_the_kind(world: World, kind: str, message: str) -> None:
    world.echo.error = KindError(Kind(kind), message)


@given(parsers.parse('the echo command fails with the kind "{kind}" and "{message}", wrapped in "{outer}"'))
def the_command_fails_wrapped(world: World, kind: str, message: str, outer: str) -> None:
    wrapped = RuntimeError(outer)
    wrapped.__cause__ = KindError(Kind(kind), message)
    world.echo.error = wrapped


@given(parsers.parse('the echo command fails with "{message}"'))
def the_command_fails(world: World, message: str) -> None:
    world.echo.error = ConnectionError(message)


@given("the echo command crashes")
def the_command_crashes(world: World) -> None:
    # A bug of the service: what Go calls a panic.
    world.echo.error = TypeError("unsupported operand type(s) for +: 'int' and 'str'")


@given("the echo command is wrapped with logging")
def the_command_is_wrapped_with_logging(world: World) -> None:
    world.handler = decorate("Echo", HandlerFunc(world.echo.handle), [LoggingMiddleware(world.logger())])


@given("the HTTP docs are enabled")
def the_docs_are_enabled(world: World) -> None:
    world.config = HttpConfig(docs_enabled=True, base_path=world.config.base_path)


@given(parsers.parse('the service is published under "{base_path}"'))
def the_service_is_published_under(world: World, base_path: str) -> None:
    world.config = HttpConfig(docs_enabled=world.config.docs_enabled, base_path=base_path)


@when(parsers.re(r"I POST \"(?P<path>[^\"]+)\" to the HTTP API with the body '(?P<body>[^']*)'$"))
def i_post(world: World, path: str, body: str) -> None:
    world.request("POST", path, content=body, headers={"Content-Type": "application/json"})


@when(parsers.re(r"I POST \"(?P<path>[^\"]+)\" to the HTTP API with the body '(?P<body>[^']*)' as \"(?P<content_type>[^\"]+)\"$"))
def i_post_as(world: World, path: str, body: str, content_type: str) -> None:
    world.request("POST", path, content=body, headers={"Content-Type": content_type})


@when(parsers.re(r"I POST \"(?P<path>[^\"]+)\" to the HTTP API with the body '(?P<body>[^']*)' and the request id \"(?P<id>[^\"]+)\"$"))
def i_post_with_a_request_id(world: World, path: str, body: str, id: str) -> None:
    world.request("POST", path, content=body, headers={"Content-Type": "application/json", "X-Request-Id": id})


@when(parsers.parse('I GET "{path}" from the HTTP API'))
def i_get(world: World, path: str) -> None:
    world.request("GET", path)


@then(parsers.parse("the HTTP status should be {status:d}"))
def the_response_status_should_be(world: World, status: int) -> None:
    assert world.answered().status_code == status


@then(parsers.re(r"the HTTP response body should be '(?P<body>[^']*)'$"))
def the_response_body_should_be(world: World, body: str) -> None:
    assert world.answered().json() == json.loads(body)


@then(parsers.parse('the HTTP response body should mention "{text}"'))
def the_response_body_should_mention(world: World, text: str) -> None:
    assert text in world.answered().text


@then(parsers.parse('the echo command should have received "{text}"'))
def the_command_should_have_received(world: World, text: str) -> None:
    assert world.echo.received == [text]


@then("the echo command should not have been called")
def the_command_should_not_have_been_called(world: World) -> None:
    assert world.echo.received == []


@then(parsers.parse('the HTTP error should have code {code:d} and the message "{message}"'))
def the_error_body_should_have_the_message(world: World, code: int, message: str) -> None:
    assert world.answered().json() == {"code": code, "message": message, "details": []}


@then(parsers.parse('the HTTP error should have code {code:d} and a message containing "{text}"'))
def the_error_body_should_have_a_message_containing(world: World, code: int, text: str) -> None:
    body = world.answered().json()
    assert body["code"] == code and text in body["message"] and body["details"] == [], body


@then(parsers.parse('the HTTP log should hold the error "{error}" of "{message}"'))
def the_log_should_hold_the_error(world: World, error: str, message: str) -> None:
    failures = world.records(message)
    assert failures and failures[0].exc_info is not None and str(failures[0].exc_info[1]) == error


def payload_of(record: logging.LogRecord) -> dict[str, Any]:
    payload = attributes_of(record).get("payload")
    assert isinstance(payload, dict), payload
    return payload


# request_log.feature


@then(parsers.parse('the HTTP records should be, in order, "{messages}"'))
def the_records_should_be_in_order(world: World, messages: str) -> None:
    assert [record.getMessage() for record in world.recorder.records] == messages.split("|")


@then(parsers.re(r"the \"http request received\" record should hold the query \"(?P<key>[^\"]+)\" = \"(?P<value>[^\"]*)\" and the body '(?P<body>[^']*)'$"))
def the_received_record_should_hold(world: World, key: str, value: str, body: str) -> None:
    (record,) = world.records("http request received")
    payload = payload_of(record)
    assert payload["query"].get(key, [""])[0] == value
    assert json.loads(payload["body"]) == json.loads(body)


@then(parsers.re(r"the \"http response sent\" record should hold the query \"(?P<key>[^\"]+)\" = \"(?P<value>[^\"]*)\" and the body '(?P<body>[^']*)'$"))
def the_sent_record_should_hold_the_request(world: World, key: str, value: str, body: str) -> None:
    (record,) = world.records("http response sent")
    request = payload_of(record)["request"]
    assert request["query"].get(key, [""])[0] == value
    assert json.loads(request["body"]) == json.loads(body)


@then(parsers.parse('one "http response sent" record should be logged with:'))
def one_sent_record_should_be_logged_with(world: World, datatable: list[list[str]]) -> None:
    (record,) = world.records("http response sent")
    given = attributes_of(record)
    header, row = datatable[0], datatable[1]
    got = [str(given[column]).lower() if isinstance(given[column], bool) else str(given[column]) for column in header]
    assert got == row


@then(parsers.re(r"the \"http response sent\" record should hold the response body '(?P<body>[^']*)'$"))
def the_sent_record_should_hold_the_response_body(world: World, body: str) -> None:
    (record,) = world.records("http response sent")
    assert json.loads(payload_of(record)["response"]["body"]) == json.loads(body)


@then(parsers.re(r"the \"http response sent\" record should hold the request body '(?P<body>[^']*)'$"))
def the_sent_record_should_hold_the_request_body(world: World, body: str) -> None:
    (record,) = world.records("http response sent")
    assert json.loads(payload_of(record)["request"]["body"]) == json.loads(body)


@then('the "http response sent" record should hold no request')
def the_sent_record_should_hold_no_request(world: World) -> None:
    (record,) = world.records("http response sent")
    assert "request" not in payload_of(record)


@then(parsers.parse('the HTTP response should carry the request id "{id}"'))
def the_response_should_carry_the_request_id(world: World, id: str) -> None:
    assert world.answered().headers["X-Request-Id"] == id


@then(parsers.parse('the echo command should have run with the request id "{id}"'))
def the_command_should_have_run_with(world: World, id: str) -> None:
    assert world.echo.request_ids == [id]


@then("the HTTP response should carry a request id of 16 hex characters")
def the_response_should_carry_a_new_request_id(world: World) -> None:
    assert re.fullmatch("[0-9a-f]{16}", world.answered().headers["X-Request-Id"])


# docs.feature


@then(parsers.parse('the spec\'s server URL should be "{url}"'))
def the_specs_server_url_should_be(world: World, url: str) -> None:
    # basePath in an OpenAPI 2 spec generated from a proto file, servers in an OpenAPI 3 one
    spec = world.answered().json()
    assert (spec["basePath"] if "swagger" in spec else spec["servers"]) == (url if "swagger" in spec else [{"url": url}])


@when("I open the Swagger UI of the HTTP API")
def i_open_the_swagger_ui(world: World) -> None:
    # Swagger UI's page, then the script that tells it where the spec is.
    world.request("GET", "/swagger/")
    assert world.answered().status_code == 200
    world.request("GET", "/swagger/swagger-initializer.js")
    assert world.answered().status_code == 200


@then(parsers.parse('the Swagger UI should load the spec from "{url}"'))
def the_swagger_ui_should_load_the_spec_from(world: World, url: str) -> None:
    assert f'"{url}"' in world.answered().text or f"'{url}'" in world.answered().text


@then(parsers.parse('the HTTP response should redirect into "{prefix}"'))
def the_response_should_redirect_into(world: World, prefix: str) -> None:
    # The Location, resolved against the URL the client called (the published
    # prefix and the path asked for), lands under the prefix.
    answered = world.answered()
    assert 300 <= answered.status_code < 400, answered.status_code
    called = f"http://client{world.config.base_path}{answered.request.url.path}"
    landed = urllib.parse.urlsplit(urllib.parse.urljoin(called, answered.headers["location"])).path
    assert landed.startswith(prefix), landed


@then("the HTTP response should be the embedded spec")
def the_response_should_be_the_committed_spec(world: World) -> None:
    assert world.answered().content == find_contract().read_bytes()
