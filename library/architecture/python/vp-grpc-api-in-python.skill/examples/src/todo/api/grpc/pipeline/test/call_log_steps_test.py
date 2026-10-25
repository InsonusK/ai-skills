import logging
import re
from typing import Any, Protocol

from pytest_bdd import parsers, scenarios, then

from todo.log import attributes_of

scenarios("../features/call_log.feature")


class World(Protocol):
    """What these steps use of conftest's World."""

    echo: Any
    metadata: Any

    def records(self, message: str) -> list[logging.LogRecord]: ...


def _payload(world: World, message: str) -> dict[str, Any]:
    (record,) = world.records(message)
    payload = attributes_of(record).get("payload")
    assert isinstance(payload, dict), payload
    return payload


@then(parsers.parse('the "grpc call received" record should hold the request "{text}"'))
def the_received_record_should_hold_the_request(world: World, text: str) -> None:
    assert _payload(world, "grpc call received") == {"request": text}


@then(parsers.parse('the "grpc call answered" record should hold the response "{text}" and no request'))
def the_answered_record_should_hold_the_response(world: World, text: str) -> None:
    assert _payload(world, "grpc call answered") == {"response": text}


@then(parsers.parse('the "grpc call answered" record should hold the request "{text}"'))
def the_answered_record_should_hold_the_request(world: World, text: str) -> None:
    assert _payload(world, "grpc call answered")["request"] == text


def _sent_request_id(world: World) -> str:
    sent = dict(world.metadata or ()).get("x-request-id")
    assert isinstance(sent, str), world.metadata
    return sent


@then(parsers.parse('the server should send back the request id "{id}"'))
def the_server_should_send_back_the_request_id(world: World, id: str) -> None:
    assert _sent_request_id(world) == id


@then("the server should send back a request id of 16 hex characters")
def the_server_should_send_back_a_new_request_id(world: World) -> None:
    assert re.fullmatch("[0-9a-f]{16}", _sent_request_id(world))


@then(parsers.parse('the command should have run with the request id "{id}"'))
def the_command_should_have_run_with(world: World, id: str) -> None:
    assert world.echo.request_ids == [id]
