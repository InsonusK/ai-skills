import asyncio
from collections.abc import Callable, Coroutine
import logging
from dataclasses import dataclass, field
from typing import Any

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.core.entry import Entry, exit_code, run_all
from todo.command.pipeline import Kind, KindError
from todo.log import attributes_of

scenarios("../features/entry.feature")


class Recorder(logging.Handler):
    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class World:
    entries: list[Entry] = field(default_factory=list)
    started: set[str] = field(default_factory=set)
    stopped: set[str] = field(default_factory=set)
    recorder: Recorder = field(default_factory=Recorder)
    error: BaseException | None = None

    def add(self, name: str, work: "Work", reports_errors: bool = False) -> None:
        async def run(stop: asyncio.Event) -> None:
            self.started.add(name)
            try:
                await work(stop)
            finally:
                self.stopped.add(name)

        self.entries.append(Entry(name, run, reports_errors))

    def run(self, terminate: bool) -> None:
        logger = logging.Logger("test")
        logger.addHandler(self.recorder)

        async def run_together() -> BaseException | None:
            stopping = asyncio.Event()
            if terminate:
                asyncio.get_running_loop().call_later(0.05, stopping.set)
            return await run_all(logger, stopping, *self.entries)

        self.error = asyncio.run(run_together())
        print(f"when: run_all -> {self.error!r}; stopped {sorted(self.stopped)}")


type Work = Callable[[asyncio.Event], Coroutine[Any, Any, None]]


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('a long-running entry "{name}"'))
def a_long_running_entry(world: World, name: str) -> None:
    async def serve(stop: asyncio.Event) -> None:
        await stop.wait()

    world.add(name, serve)


@given(parsers.parse('a one-shot entry "{name}" that succeeds'))
def a_one_shot_entry_succeeds(world: World, name: str) -> None:
    async def done(stop: asyncio.Event) -> None:
        await asyncio.sleep(0.01)

    world.add(name, done)


@given(parsers.parse('an entry "{name}" that fails with "{message}"'))
def an_entry_fails(world: World, name: str, message: str) -> None:
    async def fail(stop: asyncio.Event) -> None:
        await asyncio.sleep(0.01)
        raise OSError(message)

    world.add(name, fail)


@given(parsers.parse('a one-shot entry "{name}" that fails with the kind "{kind}"'))
def a_one_shot_entry_fails_with_kind(world: World, name: str, kind: str) -> None:
    async def fail(stop: asyncio.Event) -> None:
        raise KindError(Kind(kind), "rejected")

    world.add(name, fail)


@given(parsers.parse('an entry "{name}" that reports its own error "{message}" of the kind "{kind}"'))
def an_entry_reports_its_own_error(world: World, name: str, message: str, kind: str) -> None:
    async def fail(stop: asyncio.Event) -> None:
        raise KindError(Kind(kind), message)

    world.add(name, fail, reports_errors=True)


@when("the entry points run together")
def the_entry_points_run(world: World) -> None:
    world.run(terminate=False)


@when("the entry points run together and the process is told to terminate")
def the_entry_points_run_and_terminate(world: World) -> None:
    world.run(terminate=True)


@then("every entry point should have stopped")
def every_entry_should_have_stopped(world: World) -> None:
    assert world.stopped == world.started == {entry.name for entry in world.entries}


@then("running should end without an error")
def running_should_end_without_an_error(world: World) -> None:
    assert world.error is None


@then(parsers.parse('running should end with the error "{message}"'))
def running_should_end_with_the_error(world: World, message: str) -> None:
    assert str(world.error) == message


@then(parsers.parse("the exit code should be {code:d}"))
def the_exit_code_should_be(world: World, code: int) -> None:
    assert exit_code(world.error) == code


@then(parsers.parse('the log should name the entry "{name}" with its error'))
def the_log_should_name_the_entry(world: World, name: str) -> None:
    named = [r for r in world.recorder.records if attributes_of(r).get("entry") == name]
    assert named and named[0].exc_info is not None and named[0].exc_info[1] is world.error


@then("the log should name no entry")
def the_log_should_name_no_entry(world: World) -> None:
    assert not [r for r in world.recorder.records if "entry" in attributes_of(r)]
