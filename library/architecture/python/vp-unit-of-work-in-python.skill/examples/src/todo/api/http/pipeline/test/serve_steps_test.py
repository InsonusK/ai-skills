import asyncio
import logging
import socket
from collections.abc import Iterator
from dataclasses import dataclass

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.http.pipeline import HttpConfig, Server, run

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


async def _until_listening(port: int) -> None:
    for _ in range(200):
        try:
            _, writer = await asyncio.open_connection("127.0.0.1", port)
        except OSError:
            await asyncio.sleep(0.02)
            continue
        writer.close()
        return
    raise TimeoutError(f"nothing listens on {port}")


def _serve(lifecycle: Lifecycle, stop_when_listening: bool) -> None:
    async def scenario() -> None:
        stop = asyncio.Event()
        serving = asyncio.create_task(Server(HttpConfig(listen_port=lifecycle.port), [], logging.Logger("test")).serve(stop))
        if stop_when_listening:
            await _until_listening(lifecycle.port)
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
    lifecycle.taken = socket.create_server(("", 0), family=socket.AF_INET6, dualstack_ipv6=True) if socket.has_dualstack_ipv6() else socket.create_server(("", 0))
    lifecycle.port = lifecycle.taken.getsockname()[1]


@when("the server starts")
def the_server_starts(lifecycle: Lifecycle) -> None:
    _serve(lifecycle, stop_when_listening=False)


@when(parsers.parse('the server starts with the port "{port}"'))
def the_server_starts_with_the_port(lifecycle: Lifecycle, monkeypatch: pytest.MonkeyPatch, port: str) -> None:
    monkeypatch.setenv("HTTP_LISTEN_PORT", port)
    try:
        asyncio.run(run([], logging.Logger("test"), asyncio.Event()))
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
