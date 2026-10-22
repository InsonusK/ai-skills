import io
import json
import logging
import re
from dataclasses import dataclass, field
from typing import Any

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.log import attrs, new_logger, request_id, request_id_scope

scenarios("../features/log.feature")

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


@then(parsers.parse('the line should have "{key}" = "{value}"'))
def the_line_should_have(world: World, key: str, value: str) -> None:
    assert world.line()[key] == value


@then(parsers.parse('the line should have no "{key}"'))
def the_line_should_have_no(world: World, key: str) -> None:
    assert key not in world.line()


@then(parsers.parse('the line should not mention "{text}"'))
def the_line_should_not_mention(world: World, text: str) -> None:
    assert text not in world.written.getvalue()


@then("nothing should be written")
def nothing_should_be_written(world: World) -> None:
    assert world.written.getvalue() == ""


@when(parsers.re(r'the request id "(?P<id>[^"]*)" is put into the context'))
def the_request_id_is_put(world: World, id: str) -> None:
    with request_id_scope(id):
        world.request_id = request_id()
    print(f"when: request_id_scope({id!r}) -> {world.request_id}; after: {request_id()}")
    assert request_id() is None  # restored when the call ends


@then(parsers.parse('the request id of the context should be "{id}"'))
def the_request_id_should_be(world: World, id: str) -> None:
    assert world.request_id == id


@then("the request id of the context should be 16 hex characters")
def the_request_id_should_be_hex(world: World) -> None:
    assert world.request_id is not None and re.fullmatch("[0-9a-f]{16}", world.request_id)
