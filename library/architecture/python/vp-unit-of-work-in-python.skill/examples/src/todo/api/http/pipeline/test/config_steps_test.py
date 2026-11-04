from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.http.pipeline import HttpConfig, load_config

scenarios("../features/config.feature")

_VARIABLES = ("HTTP_LISTEN_PORT", "HTTP_BASE_PATH", "HTTP_API_DOCS_ENABLED")


@dataclass
class Settings:
    loaded: HttpConfig | None = None
    error: ValueError | None = None


@pytest.fixture
def settings(monkeypatch: pytest.MonkeyPatch) -> Settings:
    for name in _VARIABLES:
        monkeypatch.delenv(name, raising=False)
    return Settings()


@given(parsers.re(r'the environment variable "(?P<name>[^"]+)" is "(?P<value>[^"]*)"'))
def the_environment_variable_is(settings: Settings, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    # settings first: it clears the variables the scenario sets
    monkeypatch.setenv(name, value)


@when("the HTTP settings are loaded")
def the_http_settings_are_loaded(settings: Settings) -> None:
    try:
        settings.loaded = load_config()
    except ValueError as error:
        settings.error = error
    print(f"when: load_config -> {settings.loaded}, error {settings.error}")


def _loaded(settings: Settings) -> HttpConfig:
    assert settings.loaded is not None, settings.error
    return settings.loaded


@then(parsers.parse('the listen port should be "{port}"'))
def the_listen_port_should_be(settings: Settings, port: str) -> None:
    assert str(_loaded(settings).listen_port) == port


@then(parsers.re(r'the base path should be "(?P<path>[^"]*)"'))
def the_base_path_should_be(settings: Settings, path: str) -> None:
    assert _loaded(settings).base_path == path


@then(parsers.parse("the docs should be {state}"))
def the_docs_should_be(settings: Settings, state: str) -> None:
    assert _loaded(settings).docs_enabled == (state == "enabled")


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(settings: Settings, text: str) -> None:
    assert settings.error is not None and text in str(settings.error), settings.error
