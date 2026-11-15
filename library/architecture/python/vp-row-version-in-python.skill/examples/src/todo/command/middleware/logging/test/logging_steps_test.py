import asyncio
import json
import logging
from dataclasses import dataclass, field
from typing import Any

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.middleware.logging import LoggingMiddleware
from todo.command.pipeline import Handler, HandlerFunc, decorate
from todo.log import FAILED_KEY, PAYLOAD_KEY, JsonFormatter, attributes_of, level_name
from todo.shared.errors import Kind, KindError

scenarios("../features/command-logging.feature")


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too, so the
    scenarios see everything the middleware writes - what the base handler then
    drops is the log package's scenarios' concern."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass(frozen=True)
class PlanItems:
    """The command of the scenarios: one title, logged as {"title": ...}."""

    title: str


@dataclass
class World:
    recorder: Recorder = field(default_factory=Recorder)
    wrapped: Handler[PlanItems, str] | None = None
    error: Exception | None = None

    def logger(self) -> logging.Logger:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        return logger


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.re(r'a handler of PlanItems that returns "(?P<result>[^"]*)", wrapped with logging'))
def a_succeeding_handler(world: World, result: str) -> None:
    async def succeed(command: PlanItems) -> str:
        return result

    world.wrapped = decorate("PlanItems", HandlerFunc(succeed), [LoggingMiddleware(world.logger())])


@given(parsers.re(r'a handler of PlanItems that fails with "(?P<message>[^"]*)", wrapped with logging'))
def a_failing_handler(world: World, message: str) -> None:
    async def fail(command: PlanItems) -> str:
        raise RuntimeError(message)

    world.wrapped = decorate("PlanItems", HandlerFunc(fail), [LoggingMiddleware(world.logger())])


@given(parsers.re(r'a handler of PlanItems that fails as unavailable with "(?P<message>[^"]*)" caused by "(?P<cause>[^"]*)", wrapped with logging'))
def a_handler_failing_with_a_cause(world: World, message: str, cause: str) -> None:
    async def fail(command: PlanItems) -> str:
        raise KindError(Kind.UNAVAILABLE, message) from ConnectionRefusedError(cause)

    world.wrapped = decorate("PlanItems", HandlerFunc(fail), [LoggingMiddleware(world.logger())])


@when(parsers.parse('the wrapped handler handles PlanItems for "{title}"'))
def the_wrapped_handler_handles(world: World, title: str) -> None:
    assert world.wrapped is not None
    try:
        asyncio.run(world.wrapped.handle(PlanItems(title)))
    except Exception as error:
        world.error = error
    print(f"when: handle {title!r} -> error {world.error!r}, {len(world.recorder.records)} records")


@then(parsers.parse('the wrapped handler should fail with "{message}"'))
def the_wrapped_handler_should_fail_with(world: World, message: str) -> None:
    assert str(world.error) == message


def _cell(value: Any) -> str:
    """A value as the table writes it: a bool lower-case, a command as its JSON."""
    if value is None:
        return ""
    if isinstance(value, bool):
        return str(value).lower()
    if isinstance(value, PlanItems):
        return json.dumps({"title": value.title}, separators=(",", ":"))
    return str(value)


@then("the records should be:")
def the_records_should_be(world: World, datatable: list[list[str]]) -> None:
    header, *rows = datatable
    got = []
    for record in world.recorder.records:
        given = attributes_of(record)
        # The error's fields are written by the formatter from the record's
        # exception, so they are read from the line it writes.
        line = json.loads(JsonFormatter().format(record))
        cells = {
            "level": level_name(record.levelno),
            "msg": record.getMessage(),
            "command": _cell(given.get("command")),
            "payload": _cell(given.get(PAYLOAD_KEY)),
            "failed": _cell(given.get(FAILED_KEY)),
            "error": _cell(line.get("error")),
            "cause": _cell(line.get("cause")),
            "exception": _cell(line.get("exception")),
        }
        got.append([cells[column] for column in header])
    print(f"then: records {got}")
    assert got == rows
