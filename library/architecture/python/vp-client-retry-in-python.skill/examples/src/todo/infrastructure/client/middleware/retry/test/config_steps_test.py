from collections.abc import Iterator
from dataclasses import dataclass
from datetime import timedelta

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.middleware.retry import RetryConfig, load_config

scenarios("../features/config.feature")

# The settings a scenario may set; each starts with none.
VARIABLES = ["TEST_API_RETRY_ATTEMPTS", "TEST_API_RETRY_BACKOFF"]


@dataclass
class World:
    loaded: RetryConfig | None = None
    error: ValueError | None = None


# Every step takes world, so the variables are unset before the first one sets any.
@pytest.fixture
def world(monkeypatch: pytest.MonkeyPatch) -> Iterator[World]:
    for name in VARIABLES:
        monkeypatch.delenv(name, raising=False)
    yield World()


@given(parsers.parse('the environment variable "{name}" is "{value}"'))
def the_variable_is(world: World, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    monkeypatch.setenv(name, value)


@when(parsers.parse('the retry settings of "{prefix}" are loaded'))
def the_settings_are_loaded(world: World, prefix: str) -> None:
    try:
        world.loaded = load_config(prefix)
    except ValueError as error:
        world.error = error


@then(parsers.parse('the retry should allow {attempts:d} attempts with a backoff of "{backoff}"'))
def the_retry_should_allow(world: World, attempts: int, backoff: str) -> None:
    assert world.loaded == RetryConfig(attempts=attempts, backoff=timedelta(milliseconds=int(backoff[:-2]))), world.error


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(world: World, text: str) -> None:
    assert world.error is not None and text in str(world.error), world.error
