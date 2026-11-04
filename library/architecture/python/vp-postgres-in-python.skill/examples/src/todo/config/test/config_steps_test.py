import logging
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo import config

scenarios("../features/config.feature")


@dataclass
class World:
    loaded: config.Config | None = None
    error: ValueError | None = None


@pytest.fixture
def world(monkeypatch: pytest.MonkeyPatch) -> World:
    monkeypatch.delenv("LOG_LEVEL", raising=False)
    return World()


@given(parsers.parse('the environment variable "{name}" is "{value}"'))
def the_environment_variable_is(world: World, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    # world first: it clears the variables the scenario sets
    monkeypatch.setenv(name, value)
    print(f"given: {name}={value}")


@when("the configuration is loaded")
def the_configuration_is_loaded(world: World) -> None:
    try:
        world.loaded = config.load()
    except ValueError as error:
        world.error = error
    print(f"when: load -> {world.loaded}, error {world.error}")


@then(parsers.parse('the log level should be "{level}"'))
def the_log_level_should_be(world: World, level: str) -> None:
    assert world.loaded is not None, world.error
    assert logging.getLevelName(world.loaded.log_level) == level


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(world: World, text: str) -> None:
    assert world.error is not None and text in str(world.error), world.error
