import logging
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.log.masked import MaskedConfig, load_config

scenarios("../features/settings.feature")

_VARIABLES = ("LOG_PAYLOAD", "LOG_PAYLOAD_MAX_BYTES", "LOG_PAYLOAD_UNMASKED")
_LEVELS = {"DEBUG": logging.DEBUG, "INFO": logging.INFO, "WARN": logging.WARNING, "ERROR": logging.ERROR}


@dataclass
class Settings:
    loaded: MaskedConfig | None = None
    error: ValueError | None = None

    def load(self, level: int) -> None:
        try:
            self.loaded = load_config(level)
        except ValueError as error:
            self.error = error
        print(f"when: load_config -> {self.loaded}, error {self.error}")


@pytest.fixture
def settings(monkeypatch: pytest.MonkeyPatch) -> Settings:
    for name in _VARIABLES:
        monkeypatch.delenv(name, raising=False)
    return Settings()


@given(parsers.parse('the environment variable "{name}" is "{value}"'))
def the_environment_variable_is(settings: Settings, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    # settings first: it clears the variables the scenario sets
    monkeypatch.setenv(name, value)


@when("the logging settings are loaded")
def the_logging_settings_are_loaded(settings: Settings) -> None:
    settings.load(logging.INFO)


@when(parsers.parse('the logging settings are loaded for the level "{level}"'))
def the_logging_settings_are_loaded_for(settings: Settings, level: str) -> None:
    settings.load(_LEVELS[level])


@then(parsers.parse("masking should be {state}"))
def masking_should_be(settings: Settings, state: str) -> None:
    assert settings.loaded is not None, settings.error
    assert settings.loaded.unmasked == (state == "off")


@then(parsers.parse('LOG_PAYLOAD should be "{mode}" and the limit {limit:d}'))
def log_payload_should_be(settings: Settings, mode: str, limit: int) -> None:
    assert settings.loaded is not None, settings.error
    assert (settings.loaded.payload, settings.loaded.max_bytes) == (mode, limit)


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(settings: Settings, text: str) -> None:
    assert settings.error is not None and text in str(settings.error), settings.error
