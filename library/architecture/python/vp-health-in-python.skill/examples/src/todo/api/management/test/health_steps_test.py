import asyncio
import json
import logging
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.management import Check, ManagementConfig, Server
from todo.log import attributes_of

from .conftest import request

scenarios("../features/health.feature")


class Recorder(logging.Handler):
    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class World:
    checks: list[Check] = field(default_factory=list)
    version: str = "0.0.0"
    recorder: Recorder = field(default_factory=Recorder)
    status: int = 0
    body: bytes = b""

    def send(self, method: str, path: str) -> None:
        # The real server on a free port, one request through a socket.
        async def scenario() -> tuple[int, bytes]:
            logger = logging.Logger("test", logging.DEBUG)
            logger.addHandler(self.recorder)
            server, port = await Server(ManagementConfig(listen_port=0), self.version, logger, self.checks).start()
            try:
                return await request(port, method, path)
            finally:
                server.close()
                await server.wait_closed()

        self.status, self.body = asyncio.run(scenario())
        print(f"when: {method} {path} -> {self.status} {self.body!r}")


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('a readiness check "{name}" that {outcome}'))
def a_readiness_check(world: World, name: str, outcome: str) -> None:
    async def check() -> None:
        if outcome == "fails":
            raise ConnectionError(f"{name} is down")

    world.checks.append(Check(name, check))


@given(parsers.parse('the service version is "{version}"'))
def the_service_version_is(world: World, version: str) -> None:
    world.version = version


@when(parsers.parse('I GET "{path}"'))
def i_get(world: World, path: str) -> None:
    world.send("GET", path)


@when(parsers.parse('I send "{method}" "{path}"'))
def i_send(world: World, method: str, path: str) -> None:
    world.send(method, path)


@then(parsers.parse("the response status should be {status:d}"))
def the_response_status_should_be(world: World, status: int) -> None:
    assert world.status == status


@then(parsers.re(r"the response body should be '(?P<body>[^']*)'"))
def the_response_body_should_be(world: World, body: str) -> None:
    assert json.loads(world.body) == json.loads(body)


@then(parsers.parse('the log should hold the warning "{message}" for the check "{name}"'))
def the_log_should_hold_the_warning(world: World, message: str, name: str) -> None:
    warnings = [record for record in world.recorder.records if record.levelno == logging.WARNING and record.getMessage() == message]
    assert [attributes_of(record)["check"] for record in warnings] == [name]
