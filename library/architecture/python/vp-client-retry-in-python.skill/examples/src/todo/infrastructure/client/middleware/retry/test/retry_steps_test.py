import asyncio
import logging
from dataclasses import dataclass, field
from datetime import timedelta

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from todo.infrastructure.client.core.pipeline import Call, run
from todo.infrastructure.client.middleware.retry import RetryConfig, RetryMiddleware
from todo.log import attributes_of

scenarios("../features/retry.feature")


class Recorder(logging.Handler):
    """Keeps every record with all its attributes."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


def _duration(text: str) -> timedelta:
    if text.endswith("ms"):
        return timedelta(milliseconds=int(text[:-2]))
    return timedelta(seconds=int(text[:-1]))


@dataclass
class World:
    config: RetryConfig = field(default_factory=RetryConfig)
    call: Call = field(default_factory=lambda: Call("todo-api", "POST /v1/items/plan", True))
    outcomes: list[str] = field(default_factory=list)
    attempts: int = 0
    recorder: Recorder = field(default_factory=Recorder)
    error: BaseException | None = None

    async def attempt(self) -> None:
        """Ends each attempt as the scenario lists: ok, unavailable, or status."""
        outcome = self.outcomes[self.attempts]
        self.attempts += 1
        if outcome == "unavailable":
            raise ApiUnavailableError("todo-api", "connection refused")
        if outcome == "status":
            raise ApiStatusError("todo-api", StatusCode.INVALID_ARGUMENT, "rejected")

    async def run(self) -> None:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        await run(self.call, [RetryMiddleware(self.config, logger)], self.attempt)


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse("the retry allows {attempts:d} attempts with a backoff of {backoff}"))
def the_retry_allows(world: World, attempts: int, backoff: str) -> None:
    world.config = RetryConfig(attempts=attempts, backoff=_duration(backoff))


@given(parsers.parse('a retryable call whose attempts end "{outcomes}"'))
def a_retryable_call(world: World, outcomes: str) -> None:
    world.outcomes = outcomes.split("|")


@given(parsers.parse('a call not marked retryable whose attempts end "{outcomes}"'))
def a_call_not_retryable(world: World, outcomes: str) -> None:
    world.call = Call("todo-api", "POST /v1/items/plan", False)
    world.outcomes = outcomes.split("|")


@when("the call is run through the retry")
def the_call_is_run(world: World) -> None:
    try:
        asyncio.run(world.run())
    except Exception as error:
        world.error = error
    print(f"when: {world.attempts} attempts, error {world.error!r}")


@when(parsers.parse("the call is run through the retry, cancelled after {after}"))
def the_call_is_run_and_cancelled(world: World, after: str) -> None:
    async def cancelled() -> None:
        task = asyncio.create_task(world.run())
        await asyncio.sleep(_duration(after).total_seconds())
        task.cancel()
        try:
            await task
        except asyncio.CancelledError as error:
            world.error = error

    asyncio.run(cancelled())
    print(f"when: {world.attempts} attempts, error {world.error!r}")


@then(parsers.parse("the call should succeed after {attempts:d} attempts"))
def the_call_should_succeed(world: World, attempts: int) -> None:
    assert world.error is None and world.attempts == attempts, (world.attempts, world.error)


@then(parsers.parse("the call should fail as unavailable after {attempts:d} attempts"))
def the_call_should_fail_as_unavailable(world: World, attempts: int) -> None:
    assert isinstance(world.error, ApiUnavailableError) and world.attempts == attempts, (world.attempts, world.error)


@then(parsers.parse("the call should fail with a status after {attempts:d} attempts"))
def the_call_should_fail_with_a_status(world: World, attempts: int) -> None:
    assert isinstance(world.error, ApiStatusError) and world.attempts == attempts, (world.attempts, world.error)


@then(parsers.parse("the call should be cancelled after {attempts:d} attempts"))
def the_call_should_be_cancelled(world: World, attempts: int) -> None:
    assert isinstance(world.error, asyncio.CancelledError) and world.attempts == attempts, (world.attempts, world.error)


@then(parsers.parse('{count:d} "api client call retried" warnings should name the API "{api}" and the operation "{operation}"'))
def warnings_should_name(world: World, count: int, api: str, operation: str) -> None:
    found = [
        record
        for record in world.recorder.records
        if record.getMessage() == "api client call retried"
        and record.levelno == logging.WARNING
        and attributes_of(record)["api"] == api
        and attributes_of(record)["operation"] == operation
    ]
    assert len(found) == count, world.recorder.records


@then('no "api client call retried" warning should be logged')
def no_warning(world: World) -> None:
    assert not world.recorder.records
