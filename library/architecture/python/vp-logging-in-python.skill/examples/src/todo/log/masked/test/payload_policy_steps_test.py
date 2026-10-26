import io
import json
import logging
from collections.abc import Iterator
from contextlib import ExitStack
from dataclasses import dataclass, field, replace
from typing import Any

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.log import attrs, request_id_scope
from todo.log.masked import NOT_PARSED_MESSAGE, UNMASKED_MESSAGE, MaskedConfig, new_masked_logger

from .support import encode

scenarios("../features/payload-policy.feature")

_LEVELS = {"DEBUG": logging.DEBUG, "INFO": logging.INFO, "WARN": logging.WARNING, "ERROR": logging.ERROR}


@dataclass
class World:
    config: MaskedConfig = field(default_factory=MaskedConfig)
    level: int = logging.INFO
    out: io.StringIO = field(default_factory=io.StringIO)
    logger: logging.Logger | None = None
    scopes: ExitStack = field(default_factory=ExitStack)

    def log(self, message: str, payload: Any, failed: bool = False, level: int = logging.INFO) -> None:
        if self.logger is None:  # built by the first record, after the Given steps
            self.logger = new_masked_logger(self.out, self.level, self.config)
        self.logger.log(level, message, extra=attrs(command="PlanItems", failed=failed, payload=payload))

    def lines(self) -> list[dict[str, Any]]:
        written = [json.loads(line) for line in self.out.getvalue().splitlines() if line]
        print("then: log\n" + "\n".join(encode(line) for line in written))
        return written

    def record(self) -> tuple[dict[str, Any], str]:
        # The record: the first line that is not a warning of the formatter.
        for line in self.lines():
            if not _is_warning(line):
                return line, encode(line)
        raise AssertionError("nothing was logged")


def _is_warning(line: dict[str, Any]) -> bool:
    return line.get("msg") in (NOT_PARSED_MESSAGE, UNMASKED_MESSAGE)


@pytest.fixture
def world() -> Iterator[World]:
    state = World()
    with state.scopes:
        yield state


@given(parsers.parse('the request id "{id}" is in the context'))
def the_request_id_is_in_the_context(world: World, id: str) -> None:
    world.scopes.enter_context(request_id_scope(id))


@given(parsers.parse('the log is written from the level "{level}"'))
def the_log_is_written_from(world: World, level: str) -> None:
    world.level = _LEVELS[level]


@given(parsers.parse('LOG_PAYLOAD is "{mode}"'))
def log_payload_is(world: World, mode: str) -> None:
    world.config = replace(world.config, payload=mode)


@given(parsers.parse("LOG_PAYLOAD_MAX_BYTES is {limit:d}"))
def log_payload_max_bytes_is(world: World, limit: int) -> None:
    world.config = replace(world.config, max_bytes=limit)


@given("masking is switched off")
def masking_is_switched_off(world: World) -> None:
    world.config = replace(world.config, unmasked=True)


@when(parsers.re(r'a record is logged with the payload password "(?P<password>[^"]*)" and title "(?P<title>[^"]*)" in the request "(?P<id>[^"]*)"'))
def a_record_in_a_request(world: World, password: str, title: str, id: str) -> None:
    with request_id_scope(id):
        world.log("command called", {"password": password, "title": title})


@when(parsers.re(r'a record is logged with the payload password "(?P<password>[^"]*)" and title "(?P<title>[^"]*)"$'))
def a_record_with_password_and_title(world: World, password: str, title: str) -> None:
    world.log("command called", {"password": password, "title": title})


@when(parsers.parse('a record with failed "{failed}" is logged with the payload title "{title}"'))
def a_record_with_failed(world: World, failed: str, title: str) -> None:
    world.log("command called", {"title": title}, failed=failed == "true")


@when(parsers.parse('a record is logged with the payload password "{password}" and a title of {length:d} characters'))
def a_record_with_a_long_title_and_password(world: World, password: str, length: int) -> None:
    world.log("command called", {"password": password, "title": "a" * length})


@when(parsers.parse("a record is logged with the payload title of {length:d} characters"))
def a_record_with_a_long_title(world: World, length: int) -> None:
    world.log("command called", {"title": "a" * length})


@when(parsers.re(r"a record \"(?P<message>[^\"]*)\" is logged at ERROR with the payload body '(?P<body>[^']*)'"))
def a_record_at_error(world: World, message: str, body: str) -> None:
    world.log(message, {"body": body.encode()}, failed=True, level=logging.ERROR)


@when(parsers.re(r"a record \"(?P<message>[^\"]*)\" is logged with the payload body '(?P<body>[^']*)'"))
def a_record_with_a_body(world: World, message: str, body: str) -> None:
    world.log(message, {"body": body.encode()})


@then(parsers.re(r'the record should (?P<how>have|not have) "(?P<key>[^"]*)"$'))
def the_record_should_have(world: World, how: str, key: str) -> None:
    line, text = world.record()
    assert (key in line) == (how == "have"), text


@then(parsers.re(r'the record should have "(?P<key>[^"]*)" = "(?P<want>[^"]*)"'))
def the_record_should_have_the_value(world: World, key: str, want: str) -> None:
    line, text = world.record()
    assert line.get(key) == want, text


@then(parsers.re(r'the record should (?P<how>mention|not mention) "(?P<text>[^"]*)"'))
def the_record_should_mention(world: World, how: str, text: str) -> None:
    _, line = world.record()
    assert (text in line) == (how == "mention"), line


@then(parsers.parse('the log should not mention "{text}"'))
def the_log_should_not_mention(world: World, text: str) -> None:
    assert text not in "\n".join(encode(line) for line in world.lines())


@then(parsers.parse('every record should have "{key}" = "{want}"'))
def every_record_should_have(world: World, key: str, want: str) -> None:
    for line in world.lines():
        assert _is_warning(line) or line.get(key) == want, line


@then(parsers.parse('every warning should have "{key}" = "{want}"'))
def every_warning_should_have(world: World, key: str, want: str) -> None:
    for line in world.lines():
        assert not _is_warning(line) or line.get(key) == want, line


def _not_parsed(world: World) -> list[str]:
    return [f"{line['level']} {line['record']}|{line['field']}|{line['bytes']}" for line in world.lines() if line.get("msg") == NOT_PARSED_MESSAGE]


@then("the log should hold these warnings:")
def the_log_should_hold_these_warnings(world: World, datatable: list[list[str]]) -> None:
    assert _not_parsed(world) == [f"WARN {record}|{path}|{size}" for record, path, size in datatable[1:]]


@then("the log should hold no warning")
def the_log_should_hold_no_warning(world: World) -> None:
    assert _not_parsed(world) == []


@then(parsers.parse('the log should hold these "{message}" warnings:'))
def the_log_should_hold_these_named_warnings(world: World, message: str, datatable: list[list[str]]) -> None:
    got = [line.get("request_id", "") for line in world.lines() if line.get("msg") == message]
    assert got == [row[0] for row in datatable[1:]]


@then(parsers.parse('the log should hold no "{message}" warning'))
def the_log_should_hold_no_named_warning(world: World, message: str) -> None:
    assert [line for line in world.lines() if line.get("msg") == message] == []
