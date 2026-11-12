import io
import json
import logging
import re
from dataclasses import dataclass, field
from datetime import timedelta
from typing import Any

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.log import attrs, new_logger, request_id, request_id_scope
from todo.shared.errors import Kind, KindError

scenarios("../features/json-log.feature", "../features/json-log-in-python.feature")

_LEVELS = {"DEBUG": logging.DEBUG, "INFO": logging.INFO, "WARN": logging.WARNING, "ERROR": logging.ERROR}


@dataclass
class World:
    level: int = logging.INFO
    written: io.StringIO = field(default_factory=io.StringIO)
    request_id: str | None = None

    def line(self) -> dict[str, Any]:
        lines = self.written.getvalue().splitlines()
        assert len(lines) == 1, lines
        parsed: dict[str, Any] = json.loads(lines[0])
        return parsed


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the level is "{level}"'))
def the_level_is(world: World, level: str) -> None:
    world.level = _LEVELS[level]


@when(parsers.parse('a record "{message}" is logged with the command "{command}" and the payload "{payload}"'))
def a_record_is_logged(world: World, message: str, command: str, payload: str) -> None:
    # Logs through the logger as the composition root builds it.
    new_logger(world.written, world.level).info(message, extra=attrs(command=command, payload=payload))
    print(f"when: written {world.written.getvalue().strip()}")


@when(parsers.parse('a record "{message}" is logged with a duration of {milliseconds:d} milliseconds'))
def a_record_is_logged_with_a_duration(world: World, message: str, milliseconds: int) -> None:
    new_logger(world.written, world.level).info(message, extra=attrs(duration=timedelta(milliseconds=milliseconds)))
    print(f"when: written {world.written.getvalue().strip()}")


def _log_error(world: World, message: str, error: BaseException) -> None:
    # A record of a failed call, as the command logging middleware writes it.
    new_logger(world.written, world.level).warning(message, exc_info=error)
    print(f"when: written {world.written.getvalue().strip()}")


@when(parsers.parse('a record "{message}" is logged with an error "{error}" of no kind'))
def a_record_is_logged_with_an_error_of_no_kind(world: World, message: str, error: str) -> None:
    try:
        raise RuntimeError(error)
    except RuntimeError as raised:
        _log_error(world, message, raised)


@when(parsers.re(r'a record "(?P<message>[^"]*)" is logged with an error "(?P<error>[^"]*)" of the kind "(?P<kind>[^"]*)"$'))
def a_record_is_logged_with_an_error_of_the_kind(world: World, message: str, error: str, kind: str) -> None:
    _log_error(world, message, KindError(Kind(kind), error))


@when(parsers.parse('a record "{message}" is logged with an error "{error}" of the kind "{kind}" caused by "{cause}"'))
def a_record_is_logged_with_an_error_with_a_cause(world: World, message: str, error: str, kind: str, cause: str) -> None:
    try:
        raise KindError(Kind(kind), error) from ConnectionRefusedError(cause)
    except KindError as raised:
        _log_error(world, message, raised)


@then(parsers.parse('the line should have "{key}" = "{value}"'))
def the_line_should_have(world: World, key: str, value: str) -> None:
    got = world.line()[key]
    assert (got if isinstance(got, str) else json.dumps(got)) == value


@then(parsers.parse('the line should have an "{key}"'))
def the_line_should_have_an(world: World, key: str) -> None:
    assert world.line().get(key)


@then(parsers.parse('the line should have no "{key}"'))
def the_line_should_have_no(world: World, key: str) -> None:
    assert key not in world.line()


@then(parsers.parse('the line should not mention "{text}"'))
def the_line_should_not_mention(world: World, text: str) -> None:
    assert text not in world.written.getvalue()


@then("nothing should be written")
def nothing_should_be_written(world: World) -> None:
    assert world.written.getvalue() == ""


@when(parsers.re(r'the request id "(?P<id>[^"]*)" is begun'))
def the_request_id_is_put(world: World, id: str) -> None:
    with request_id_scope(id):
        world.request_id = request_id()
    print(f"when: request_id_scope({id!r}) -> {world.request_id}; after: {request_id()}")
    assert request_id() is None  # restored when the call ends


@then(parsers.parse('the current request id should be "{id}"'))
def the_request_id_should_be(world: World, id: str) -> None:
    assert world.request_id == id


@then("the current request id should be 16 hex characters")
def the_request_id_should_be_hex(world: World) -> None:
    assert world.request_id is not None and re.fullmatch("[0-9a-f]{16}", world.request_id)
