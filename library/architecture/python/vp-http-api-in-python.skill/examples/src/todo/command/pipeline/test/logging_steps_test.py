import asyncio
import logging
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.pipeline import Handler, HandlerFunc, with_logging
from todo.log import FAILED_KEY, PAYLOAD_KEY, attributes_of, level_name

scenarios("../features/handler_logging.feature")


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too, so the
    scenarios see everything the decorator writes - what the base handler then
    drops is the log package's scenarios' concern."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class World:
    recorder: Recorder = field(default_factory=Recorder)
    wrapped: Handler[str, str] | None = None
    error: Exception | None = None

    def logger(self) -> logging.Logger:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        return logger


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('a handler that succeeds, wrapped with logging as "{name}"'))
def a_succeeding_handler(world: World, name: str) -> None:
    async def succeed(command: str) -> str:
        return "ok"

    world.wrapped = with_logging(name, world.logger(), HandlerFunc(succeed))


@given(parsers.parse('a handler that fails with "{message}", wrapped with logging as "{name}"'))
def a_failing_handler(world: World, message: str, name: str) -> None:
    async def fail(command: str) -> str:
        raise RuntimeError(message)

    world.wrapped = with_logging(name, world.logger(), HandlerFunc(fail))


@when(parsers.parse('the wrapped handler handles the command "{command}"'))
def the_wrapped_handler_handles(world: World, command: str) -> None:
    assert world.wrapped is not None
    try:
        asyncio.run(world.wrapped.handle(command))
    except RuntimeError as error:
        world.error = error
    print(f"when: handle {command!r} -> error {world.error!r}, {len(world.recorder.records)} records")


@then(parsers.parse('the wrapped handler should return the error "{message}"'))
def the_wrapped_handler_should_return(world: World, message: str) -> None:
    assert str(world.error) == message


def _cell(value: object) -> str:
    if value is None:
        return ""
    return str(value).lower() if isinstance(value, bool) else str(value)


@then("the records should be:")
def the_records_should_be(world: World, datatable: list[list[str]]) -> None:
    header, *rows = datatable
    got = []
    for record in world.recorder.records:
        given = attributes_of(record)
        cells = {
            "level": level_name(record.levelno),
            "msg": record.getMessage(),
            "command": _cell(given.get("command")),
            "payload": _cell(given.get(PAYLOAD_KEY)),
            "failed": _cell(given.get(FAILED_KEY)),
        }
        got.append([cells[column] for column in header])
    print(f"then: records {got}")
    assert got == rows
