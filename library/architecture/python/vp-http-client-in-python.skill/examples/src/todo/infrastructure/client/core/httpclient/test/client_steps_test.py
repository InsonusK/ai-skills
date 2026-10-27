import asyncio
import json
import logging
from collections.abc import Iterator
from dataclasses import dataclass, field
from datetime import timedelta
from typing import Any

import httpx2
import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.core.httpclient import MOCK, Emulator, HttpClient, HttpConfig
from .api_server import TestApi, free_port
from todo.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from todo.log import FAILED_KEY, PAYLOAD_KEY, attributes_of, request_id_scope

scenarios("../features/call.feature", "../features/call_log.feature", "../features/mock.feature")


@dataclass(frozen=True)
class Echo:
    """The request and the response of the test API's one operation, POST /v1/echo."""

    text: str


def _echo(answer: Any) -> Echo:
    if not isinstance(answer["text"], str):
        raise TypeError(f"text {answer['text']!r} is no string")
    return Echo(text=answer["text"])


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class World:
    api: TestApi = field(default_factory=TestApi)
    down: bool = False
    emulator: Emulator | None = None
    recorder: Recorder = field(default_factory=Recorder)
    answer: Echo | None = None
    error: Exception | None = None

    def client(self) -> HttpClient:
        """The client of the test API: against its server, or, mocked, against the scenario's emulator."""
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        if self.emulator is not None:
            return HttpClient(HttpConfig(name="test-api", host=MOCK, port=80, timeout=timedelta(seconds=1)), logger, self.emulator)
        port = free_port() if self.down else self.api.start()
        timeout = timedelta(milliseconds=50) if self.api.delay else timedelta(seconds=1)
        return HttpClient(HttpConfig(name="test-api", host="127.0.0.1", port=port, timeout=timeout), logger, lambda _: httpx2.Response(500))

    def records(self, message: str) -> list[dict[str, Any]]:
        return [attributes_of(record) for record in self.recorder.records if record.getMessage() == message]

    def only(self, message: str) -> dict[str, Any]:
        found = self.records(message)
        assert len(found) == 1, [record.getMessage() for record in self.recorder.records]
        return found[0]


@pytest.fixture
def world() -> Iterator[World]:
    world = World()
    yield world
    world.api.stop()


# the test API


@given(parsers.parse("the test API answers {status:d} with '{body}'"))
def the_test_api_answers(world: World, status: int, body: str) -> None:
    world.api.status, world.api.body = status, body


@given(parsers.parse("the test API answers {status:d} with ''"))
def the_test_api_answers_nothing(world: World, status: int) -> None:
    world.api.status, world.api.body = status, ""


@given("the test API takes longer than the timeout")
def the_test_api_is_slow(world: World) -> None:
    world.api.delay = 0.3


@given("the test API is not listening")
def the_test_api_is_down(world: World) -> None:
    world.down = True


@given(parsers.parse("the client of \"{prefix}\" is mocked by an emulator answering '{body}'"))
def the_client_is_mocked(world: World, prefix: str, body: str) -> None:
    world.emulator = lambda _: httpx2.Response(200, content=body.encode())


# the call


def _call(world: World, method: str, path: str, text: str, id: str | None = None) -> None:
    async def call() -> Echo:
        client = world.client()
        try:
            if id is None:
                return await client.call(method, path, Echo(text=text), _echo)
            with request_id_scope(id):
                return await client.call(method, path, Echo(text=text), _echo)
        finally:
            await client.aclose()

    try:
        world.answer = asyncio.run(call())
    except Exception as error:
        world.error = error
    print(f"when: {method} {path} -> {world.answer}, error {world.error!r}")


@when(parsers.parse('I call {method} "{path}" with the text "{text}"'))
def i_call(world: World, method: str, path: str, text: str) -> None:
    _call(world, method, path, text)


@when(parsers.parse('I call {method} "{path}" with the text "{text}" in the request "{id}"'))
def i_call_in_the_request(world: World, method: str, path: str, text: str, id: str) -> None:
    _call(world, method, path, text, id)


@then(parsers.parse('the call should succeed with the text "{text}"'))
def the_call_should_succeed(world: World, text: str) -> None:
    assert world.error is None and world.answer == Echo(text=text), (world.answer, world.error)


@then(parsers.parse("the test API should have received {method} \"{path}\" with '{body}'"))
def the_test_api_received(world: World, method: str, path: str, body: str) -> None:
    [received] = world.api.received
    assert (received.method, received.path, json.loads(received.body)) == (method, path, json.loads(body)), received


@then(parsers.parse('the test API should have received the request id "{id}"'))
def the_test_api_received_the_id(world: World, id: str) -> None:
    [received] = world.api.received
    assert received.request_id == id, received


@then("the test API should have received no request id")
def the_test_api_received_no_id(world: World) -> None:
    [received] = world.api.received
    assert received.request_id is None, received


@then(parsers.parse('the call should fail with the status {status:d}, the code {code:d}, and the message "{message}"'))
def the_call_should_fail_with_the_status(world: World, status: int, code: int, message: str) -> None:
    assert isinstance(world.error, ApiStatusError), world.error
    # A body that is no google.rpc.Status (code 0 in the scenario) is the code UNKNOWN.
    expected = StatusCode.UNKNOWN if code == 0 else StatusCode(code)
    assert (world.error.http_status, world.error.code, world.error.message) == (status, expected, message)


@then(parsers.parse('the call should fail with the status {status:d}, the code {code:d}, and the message ""'))
def the_call_should_fail_with_no_message(world: World, status: int, code: int) -> None:
    the_call_should_fail_with_the_status(world, status, code, "")


@then("the call should fail as unavailable")
def the_call_should_fail_as_unavailable(world: World) -> None:
    assert isinstance(world.error, ApiUnavailableError), world.error


@then(parsers.parse('the call should fail mentioning "{text}"'))
def the_call_should_fail_mentioning(world: World, text: str) -> None:
    assert world.error is not None and not isinstance(world.error, (ApiUnavailableError, ApiStatusError)), world.error
    assert text in str(world.error), world.error


# call_log.feature and mock.feature


@then(parsers.parse('the records should be, in order, "{messages}"'))
def the_records_should_be(world: World, messages: str) -> None:
    assert "|".join(record.getMessage() for record in world.recorder.records) == messages


@then(parsers.parse('the "{message}" record should hold the request text "{text}"'))
def the_record_holds_the_request(world: World, message: str, text: str) -> None:
    assert world.only(message)[PAYLOAD_KEY]["request"] == Echo(text=text)


@then(
    parsers.parse(
        'one "http client response received" record should be logged with the API "{api}", '
        '{method} "{path}", the status {status:d}, and failed "{failed}"'
    )
)
def one_response_received_record(world: World, api: str, method: str, path: str, status: int, failed: str) -> None:
    record = world.only("http client response received")
    got = (record["api"], record["method"], record["path"], record["status"], record[FAILED_KEY])
    assert got == (api, method, path, status, failed == "true"), got


@then(parsers.parse("the \"{message}\" record should hold the response '{body}' and no request"))
def the_record_holds_the_response(world: World, message: str, body: str) -> None:
    payload = world.only(message)[PAYLOAD_KEY]
    assert "request" not in payload and payload["response"] == _echo(json.loads(body)), payload


@then(parsers.parse('the "api client mocked" record should be a warning naming the API "{api}"'))
def the_mocked_record_is_a_warning(world: World, api: str) -> None:
    [record] = [record for record in world.recorder.records if record.getMessage() == "api client mocked"]
    assert record.levelno == logging.WARNING and attributes_of(record)["api"] == api
