from collections.abc import Iterator
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.command.middleware.access import AccessConfig, load_config

scenarios("../features/config.feature")


@dataclass
class World:
    config: AccessConfig | None = None
    error: ValueError | None = None


@pytest.fixture
def world(monkeypatch: pytest.MonkeyPatch) -> Iterator[World]:
    # each scenario starts with none of the variables set
    for name in ("ACCESS_TOKENS", "ACCESS_MAX_BATCH"):
        monkeypatch.delenv(name, raising=False)
    yield World()


@given(parsers.re(r'the environment variable "(?P<name>[^"]+)" is "(?P<value>[^"]*)"'))
def the_environment_variable(world: World, monkeypatch: pytest.MonkeyPatch, name: str, value: str) -> None:
    monkeypatch.setenv(name, value)  # after world, which clears the variables


@when("the access settings are loaded")
def the_settings_are_loaded(world: World) -> None:
    try:
        world.config = load_config()
    except ValueError as error:
        world.error = error
    print(f"when: load_config -> {world.config!r}, error {world.error!r}")


@then(parsers.re(r'the token "(?P<token>[^"]+)" should name "(?P<user>[^"]+)" with the roles "(?P<roles>[^"]*)"'))
def the_token_should_name(world: World, token: str, user: str, roles: str) -> None:
    assert world.config is not None
    identity = world.config.tokens[token]
    assert (identity.user, ",".join(identity.roles)) == (user, roles)


@then(parsers.parse("the batch limit should be {limit:d}"))
def the_batch_limit_should_be(world: World, limit: int) -> None:
    assert world.config is not None and world.config.max_batch == limit


@then(parsers.parse('loading should fail mentioning "{text}"'))
def loading_should_fail(world: World, text: str) -> None:
    assert world.error is not None and text in str(world.error)
