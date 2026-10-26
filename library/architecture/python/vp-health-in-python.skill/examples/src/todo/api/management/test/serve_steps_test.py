import asyncio
import logging
import socket
from collections.abc import Iterator
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.management import ManagementConfig, Server, run

from .conftest import request

scenarios("../features/serve.feature")


@dataclass
class Lifecycle:
    port: int = 0
    taken: socket.socket | None = None
    error: BaseException | None = None
    finished: bool = False


@pytest.fixture
def lifecycle() -> Iterator[Lifecycle]:
    state = Lifecycle()
    yield state
    if state.taken is not None:
        state.taken.close()


def _free_port() -> int:
    with socket.socket() as probe:
        probe.bind(("127.0.0.1", 0))
        port: int = probe.getsockname()[1]
        return port


def _serve(lifecycle: Lifecycle, stop_when_listening: bool) -> None:
    async def scenario() -> None:
        stop = asyncio.Event()
        serving = asyncio.create_task(Server(ManagementConfig(listen_port=lifecycle.port), "0.0.0", logging.Logger("test")).serve(stop))
        if stop_when_listening:
            for _ in range(200):  # until it answers
                try:
                    await request(lifecycle.port, "GET", "/health/live")
                    break
                except OSError:
                    await asyncio.sleep(0.02)
            stop.set()
        await asyncio.wait_for(serving, timeout=15)

    try:
        asyncio.run(scenario())
        lifecycle.finished = True
    except Exception as error:
        lifecycle.error = error
    print(f"when: serve on {lifecycle.port} -> finished {lifecycle.finished}, error {lifecycle.error!r}")


@given("the server is serving on a free port")
def the_server_is_serving(lifecycle: Lifecycle) -> None:
    lifecycle.port = _free_port()


@when("the process is told to terminate")
def the_process_is_told_to_terminate(lifecycle: Lifecycle) -> None:
    _serve(lifecycle, stop_when_listening=True)


@given("the port the server wants is already taken")
def the_port_is_taken(lifecycle: Lifecycle) -> None:
    lifecycle.taken = socket.create_server(("", 0))
    lifecycle.port = lifecycle.taken.getsockname()[1]


@when("the server starts")
def the_server_starts(lifecycle: Lifecycle) -> None:
    _serve(lifecycle, stop_when_listening=False)


@when(parsers.parse('the server starts with the port "{port}"'))
def the_server_starts_with_the_port(lifecycle: Lifecycle, monkeypatch: pytest.MonkeyPatch, port: str) -> None:
    monkeypatch.setenv("MANAGEMENT_LISTEN_PORT", port)
    try:
        asyncio.run(run(logging.Logger("test"), "0.0.0", asyncio.Event()))
    except ValueError as error:
        lifecycle.error = error


@then("the server should stop without an error")
def the_server_should_stop_without_an_error(lifecycle: Lifecycle) -> None:
    assert lifecycle.finished and lifecycle.error is None, lifecycle.error


@then("the server should stop with an error")
def the_server_should_stop_with_an_error(lifecycle: Lifecycle) -> None:
    assert isinstance(lifecycle.error, OSError), lifecycle.error


@then(parsers.parse('the server should stop with an error mentioning "{text}"'))
def the_server_should_stop_with_an_error_mentioning(lifecycle: Lifecycle, text: str) -> None:
    assert lifecycle.error is not None and text in str(lifecycle.error), lifecycle.error
