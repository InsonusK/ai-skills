from collections.abc import Iterator
from dataclasses import dataclass
from datetime import timedelta

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.infrastructure.client.core.grpcclient import GrpcConfig, load_config

scenarios("../features/config.feature")

# The settings of the test API a scenario may set; each starts with none.
VARIABLES = ["TEST_API_HOST", "TEST_API_GRPC_PORT", "TEST_API_TLS", "TEST_API_TIMEOUT"]


@dataclass
class World:
    loaded: GrpcConfig | None = None
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


@given(parsers.parse('the environment variable "{name}" is ""'))
def the_variable_is_empty(world: World, monkeypatch: pytest.MonkeyPatch, name: str) -> None:
    monkeypatch.setenv(name, "")


@when(parsers.parse('the settings of "{prefix}" are loaded'))
def the_settings_are_loaded(world: World, prefix: str) -> None:
    try:
        world.loaded = load_config(prefix)
    except ValueError as error:
        world.error = error
    print(f"when: load_config({prefix}) -> {world.loaded}, error {world.error!r}")


def _duration(text: str) -> timedelta:
    if text.endswith("ms"):
        return timedelta(milliseconds=float(text[:-2]))
    return timedelta(seconds=float(text[:-1]))


@then(parsers.parse('the target should be "{target}", TLS "{tls}", and the timeout "{timeout}"'))
def the_target_should_be(world: World, target: str, tls: str, timeout: str) -> None:
    assert world.loaded is not None, world.error
    assert (world.loaded.target, world.loaded.tls, world.loaded.timeout) == (target, tls == "true", _duration(timeout))


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(world: World, text: str) -> None:
    assert world.error is not None and text in str(world.error), world.error
