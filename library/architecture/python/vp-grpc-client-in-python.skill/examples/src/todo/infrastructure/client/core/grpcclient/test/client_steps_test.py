import asyncio
import logging
import re
import socket
from dataclasses import dataclass, field
from datetime import timedelta
from typing import Any

import grpc
import pytest
from google.protobuf.wrappers_pb2 import StringValue
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.core.grpcclient import MOCK, Emulator, GrpcClient, GrpcConfig
from todo.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from todo.log import FAILED_KEY, PAYLOAD_KEY, attributes_of, request_id_scope

from .echo_api import ECHO_METHOD, EchoBehavior, serve

scenarios("../features/call.feature", "../features/call_log.feature", "../features/mock.feature")


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


def _free_port() -> int:
    with socket.socket() as probe:
        probe.bind(("127.0.0.1", 0))
        return int(probe.getsockname()[1])


@dataclass
class World:
    api: EchoBehavior = field(default_factory=EchoBehavior)
    down: bool = False
    emulator: Emulator | None = None
    recorder: Recorder = field(default_factory=Recorder)
    answer: str | None = None
    error: Exception | None = None

    def only(self, message: str) -> dict[str, Any]:
        found = [attributes_of(record) for record in self.recorder.records if record.getMessage() == message]
        assert len(found) == 1, [record.getMessage() for record in self.recorder.records]
        return found[0]


@pytest.fixture
def world() -> World:
    return World()


# the test API


@given(parsers.parse('the test API answers "{text}"'))
def the_test_api_answers(world: World, text: str) -> None:
    world.api.text = text


@given(parsers.parse('the test API fails with {code} and the message "{message}"'))
def the_test_api_fails(world: World, code: str, message: str) -> None:
    world.api.failure = (grpc.StatusCode[code], message)


@given("the test API takes longer than the timeout")
def the_test_api_is_slow(world: World) -> None:
    world.api.delay = 0.3


@given("the test API is not listening")
def the_test_api_is_down(world: World) -> None:
    world.down = True


@given(parsers.parse('the client of "{prefix}" is mocked by an emulator answering "{text}"'))
def the_client_is_mocked(world: World, prefix: str, text: str) -> None:
    world.emulator = EchoBehavior(text=text).add_to


# the call


def _call(world: World, text: str, id: str | None = None) -> None:
    async def call() -> str:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(world.recorder)
        server = None
        if world.emulator is not None:
            config = GrpcConfig(name="test-api", host=MOCK, port=80, timeout=timedelta(seconds=1))
            client = await GrpcClient.open(config, logger, world.emulator)
        else:
            port = _free_port()
            if not world.down:
                server, port = await serve(world.api)
            timeout = timedelta(milliseconds=50) if world.api.delay else timedelta(seconds=5)
            client = await GrpcClient.open(GrpcConfig(name="test-api", host="127.0.0.1", port=port, timeout=timeout), logger, lambda _: None)
        echo = client.channel.unary_unary(ECHO_METHOD, request_serializer=StringValue.SerializeToString, response_deserializer=StringValue.FromString)
        try:
            if id is None:
                answer = await echo(StringValue(value=text))
            else:
                with request_id_scope(id):
                    answer = await echo(StringValue(value=text))
            return str(answer.value)
        finally:
            await client.aclose()
            if server is not None:
                await server.stop(None)

    try:
        world.answer = asyncio.run(call())
    except Exception as error:
        world.error = error
    print(f"when: Echo {text} -> {world.answer}, error {world.error!r}")


@when(parsers.parse('I call Echo with the text "{text}"'))
def i_call_echo(world: World, text: str) -> None:
    _call(world, text)


@when(parsers.parse('I call Echo with the text "{text}" in the request "{id}"'))
def i_call_echo_in_the_request(world: World, text: str, id: str) -> None:
    _call(world, text, id)


@then(parsers.parse('the call should succeed with the text "{text}"'))
def the_call_should_succeed(world: World, text: str) -> None:
    assert world.error is None and world.answer == text, (world.answer, world.error)


@then(parsers.parse('the test API should have received Echo with the text "{text}"'))
def the_test_api_received(world: World, text: str) -> None:
    [received] = world.api.received
    assert received.text == text, received


@then(parsers.parse('the test API should have received the request id "{id}"'))
def the_test_api_received_the_id(world: World, id: str) -> None:
    [received] = world.api.received
    assert received.request_id == id, received


@then("the test API should have received no request id")
def the_test_api_received_no_id(world: World) -> None:
    [received] = world.api.received
    assert received.request_id is None, received


@then(parsers.parse('the call should fail with the code {code} and the message "{message}"'))
def the_call_should_fail_with_the_code(world: World, code: str, message: str) -> None:
    assert isinstance(world.error, ApiStatusError), world.error
    assert (world.error.code, world.error.message, world.error.http_status) == (StatusCode[code], message, None)


@then("the call should fail as unavailable")
def the_call_should_fail_as_unavailable(world: World) -> None:
    assert isinstance(world.error, ApiUnavailableError), world.error


# call_log.feature and mock.feature


@then(parsers.parse('the records should be, in order, "{messages}"'))
def the_records_should_be(world: World, messages: str) -> None:
    assert "|".join(record.getMessage() for record in world.recorder.records) == messages


@then(parsers.parse('the "{message}" record should hold the request text "{text}"'))
def the_record_holds_the_request(world: World, message: str, text: str) -> None:
    assert world.only(message)[PAYLOAD_KEY]["request"] == StringValue(value=text)


# The code as the log writes it, grpc.StatusCode's name as the gRPC server of
# this stack logs it too (OK, INVALID_ARGUMENT), from its name in the feature,
# which is Go's (OK, InvalidArgument).
def _logged(code: str) -> str:
    return re.sub(r"(?<=[a-z])(?=[A-Z])", "_", code).upper()


@then(
    parsers.parse(
        'one "grpc client call answered" record should be logged with the API "{api}", the method "{method}", '
        'the code "{code}", and failed "{failed}"'
    )
)
def one_call_answered_record(world: World, api: str, method: str, code: str, failed: str) -> None:
    record = world.only("grpc client call answered")
    got = (record["api"], record["method"], record["code"], record[FAILED_KEY])
    assert got == (api, method, _logged(code), failed == "true"), got


@then(parsers.parse('the "{message}" record should hold the response text "{text}" and no request'))
def the_record_holds_the_response(world: World, message: str, text: str) -> None:
    payload = world.only(message)[PAYLOAD_KEY]
    assert "request" not in payload and payload["response"] == StringValue(value=text), payload


@then(parsers.parse('the "{message}" record should hold the status message "{text}"'))
def the_record_holds_the_status(world: World, message: str, text: str) -> None:
    assert world.only(message)[PAYLOAD_KEY]["status"]["message"] == text


@then(parsers.parse('the "api client mocked" record should be a warning naming the API "{api}"'))
def the_mocked_record_is_a_warning(world: World, api: str) -> None:
    [record] = [record for record in world.recorder.records if record.getMessage() == "api client mocked"]
    assert record.levelno == logging.WARNING and attributes_of(record)["api"] == api
