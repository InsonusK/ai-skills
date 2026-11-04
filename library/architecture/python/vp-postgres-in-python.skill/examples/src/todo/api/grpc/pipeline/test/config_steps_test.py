from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.grpc.pipeline import GrpcConfig, load_config

scenarios("../features/config.feature")

_VARIABLES = ("GRPC_LISTEN_PORT", "GRPC_REFLECTION_ENABLED")


@dataclass
class Settings:
    loaded: GrpcConfig | None = None
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


@when("the gRPC settings are loaded")
def the_grpc_settings_are_loaded(settings: Settings) -> None:
    try:
        settings.loaded = load_config()
    except ValueError as error:
        settings.error = error
    print(f"when: load_config -> {settings.loaded}, error {settings.error}")


def _loaded(settings: Settings) -> GrpcConfig:
    assert settings.loaded is not None, settings.error
    return settings.loaded


@then(parsers.parse('the listen port should be "{port}"'))
def the_listen_port_should_be(settings: Settings, port: str) -> None:
    assert str(_loaded(settings).listen_port) == port


@then(parsers.parse("reflection should be {state}"))
def reflection_should_be(settings: Settings, state: str) -> None:
    assert _loaded(settings).reflection_enabled == (state == "enabled")


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(settings: Settings, text: str) -> None:
    assert settings.error is not None and text in str(settings.error), settings.error
