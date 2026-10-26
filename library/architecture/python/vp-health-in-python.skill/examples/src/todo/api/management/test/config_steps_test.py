from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.management import ManagementConfig, load_config

scenarios("../features/config.feature")


@dataclass
class Settings:
    loaded: ManagementConfig | None = None
    error: ValueError | None = None


@pytest.fixture
def settings(monkeypatch: pytest.MonkeyPatch) -> Settings:
    monkeypatch.delenv("MANAGEMENT_LISTEN_PORT", raising=False)
    return Settings()


@given(parsers.parse('the environment variable "{name}" is "{value}"'))
def the_environment_variable_is(settings: Settings, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    # settings first: it clears the variable the scenario sets
    monkeypatch.setenv(name, value)


@when("the management settings are loaded")
def the_management_settings_are_loaded(settings: Settings) -> None:
    try:
        settings.loaded = load_config()
    except ValueError as error:
        settings.error = error
    print(f"when: load_config -> {settings.loaded}, error {settings.error}")


@then(parsers.parse('the listen port should be "{port}"'))
def the_listen_port_should_be(settings: Settings, port: str) -> None:
    assert settings.loaded is not None, settings.error
    assert str(settings.loaded.listen_port) == port


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(settings: Settings, text: str) -> None:
    assert settings.error is not None and text in str(settings.error), settings.error
