import asyncio
import logging
import socket
from collections.abc import Iterator
from dataclasses import dataclass, field

import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.grpc.pipeline import GrpcConfig, Server, run

from .echo import EchoCommand, call_echo, echo_service

scenarios("../features/serve.feature")


@dataclass
class Lifecycle:
    port: int = 0
    taken: socket.socket | None = None
    echo: EchoCommand = field(default_factory=EchoCommand)
    error: BaseException | None = None
    finished: bool = False
    in_flight: str | BaseException | None = None


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


def _serve(lifecycle: Lifecycle, stop_when_listening: bool, call_in_flight: bool = False) -> None:
    async def scenario() -> None:
        stop = asyncio.Event()
        server = Server(GrpcConfig(listen_port=lifecycle.port), [echo_service(lifecycle.echo)], logging.Logger("test"))
        serving = asyncio.create_task(server.serve(stop))
        if stop_when_listening:
            await _until_listening(lifecycle.port)
            calling = asyncio.create_task(call_echo(lifecycle.port, "go")) if call_in_flight else None
            if calling is not None:
                while not lifecycle.echo.received:  # the command has the call
                    await asyncio.sleep(0.01)
            stop.set()
            if calling is not None:
                try:
                    lifecycle.in_flight, _ = await calling
                except Exception as error:
                    lifecycle.in_flight = error
        await asyncio.wait_for(serving, timeout=15)

    try:
        asyncio.run(scenario())
        lifecycle.finished = True
    except Exception as error:
        lifecycle.error = error
    print(f"when: serve on {lifecycle.port} -> finished {lifecycle.finished}, error {lifecycle.error!r}, in flight {lifecycle.in_flight!r}")


@given("the server is serving on a free port")
def the_server_is_serving(lifecycle: Lifecycle) -> None:
    lifecycle.port = _free_port()


@given(parsers.parse('the command takes {milliseconds:d} milliseconds and answers "{answer}"'))
def the_command_takes(lifecycle: Lifecycle, milliseconds: int, answer: str) -> None:
    lifecycle.echo.delay, lifecycle.echo.answer = milliseconds / 1000, answer


@when("the process is told to terminate")
def the_process_is_told_to_terminate(lifecycle: Lifecycle) -> None:
    _serve(lifecycle, stop_when_listening=True)


@when("I start a call of the test method and the process is told to terminate")
def a_call_in_flight_when_told_to_terminate(lifecycle: Lifecycle) -> None:
    _serve(lifecycle, stop_when_listening=True, call_in_flight=True)


@given("the port the server wants is already taken")
def the_port_is_taken(lifecycle: Lifecycle) -> None:
    lifecycle.taken = socket.create_server(("", 0), family=socket.AF_INET6, dualstack_ipv6=True) if socket.has_dualstack_ipv6() else socket.create_server(("", 0))
    lifecycle.port = lifecycle.taken.getsockname()[1]


@when("the server starts")
def the_server_starts(lifecycle: Lifecycle) -> None:
    _serve(lifecycle, stop_when_listening=False)


@when(parsers.parse('the server starts with the port "{port}"'))
def the_server_starts_with_the_port(lifecycle: Lifecycle, monkeypatch: pytest.MonkeyPatch, port: str) -> None:
    monkeypatch.setenv("GRPC_LISTEN_PORT", port)
    try:
        asyncio.run(run([], logging.Logger("test"), asyncio.Event()))
    except ValueError as error:
        lifecycle.error = error


@then(parsers.parse('the call in flight should succeed with "{reply}"'))
def the_call_in_flight_should_succeed(lifecycle: Lifecycle, reply: str) -> None:
    assert lifecycle.in_flight == reply, lifecycle.in_flight


@then("the server should stop without an error")
def the_server_should_stop_without_an_error(lifecycle: Lifecycle) -> None:
    assert lifecycle.finished and lifecycle.error is None, lifecycle.error


@then("the server should stop with an error")
def the_server_should_stop_with_an_error(lifecycle: Lifecycle) -> None:
    assert lifecycle.error is not None and not lifecycle.finished


@then(parsers.parse('the server should stop with an error mentioning "{text}"'))
def the_server_should_stop_with_an_error_mentioning(lifecycle: Lifecycle, text: str) -> None:
    assert lifecycle.error is not None and text in str(lifecycle.error), lifecycle.error
