"""The steps several features of the gRPC pipeline share: the test service
(echo.py) served by the real server over TCP on a free port, the fake command
behind it, and the answers and log records of a call."""

import asyncio
import logging
from collections.abc import Iterator
from dataclasses import dataclass, field

import grpc
import pytest
from pytest_bdd import given, parsers, then, when

from todo.api.grpc.middleware import Interceptor
from todo.api.grpc.pipeline import GrpcConfig, Server
from todo.command.middleware.logging import LoggingMiddleware
from todo.command.pipeline import Handler, HandlerFunc, Kind, KindError, decorate
from todo.log import attributes_of

from .echo import EchoCommand, call_echo, echo_service


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class World:
    """One scenario: its own event loop, on which the server - started by the
    first call, after the Given steps set it up - serves every call of the
    scenario until the scenario ends."""

    echo: EchoCommand = field(default_factory=EchoCommand)
    handler: Handler[str, str] | None = None
    recorder: Recorder = field(default_factory=Recorder)
    config: GrpcConfig = field(default_factory=lambda: GrpcConfig(listen_port=0))
    interceptors: list[Interceptor] = field(default_factory=list)
    loop: asyncio.AbstractEventLoop = field(default_factory=asyncio.new_event_loop)
    server: grpc.aio.Server | None = None
    port: int = 0
    reply: str | None = None
    metadata: grpc.aio.Metadata | None = None
    error: grpc.aio.AioRpcError | None = None

    def logger(self) -> logging.Logger:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        return logger

    def serving(self) -> int:
        """The port of the scenario's server, started on the first call."""
        if self.server is None:
            server = Server(self.config, [echo_service(self.handler or self.echo)], self.logger(), self.interceptors)
            self.server, self.port = self.loop.run_until_complete(server.start())
        return self.port

    def call(self, text: str, metadata: tuple[tuple[str, str], ...] = ()) -> None:
        self.reply, self.metadata, self.error = None, None, None
        try:
            self.reply, self.metadata = self.loop.run_until_complete(call_echo(self.serving(), text, metadata))
        except grpc.aio.AioRpcError as error:
            self.error = error
        print(f"when: Echo({text!r}) -> {self.reply!r}, error {self.error and (self.error.code(), self.error.details())}")

    def close(self) -> None:
        if self.server is not None:
            self.loop.run_until_complete(self.server.stop(None))
        self.loop.close()

    def failed(self) -> grpc.aio.AioRpcError:
        assert self.error is not None, f"the call succeeded with {self.reply!r}"
        return self.error

    def records(self, message: str) -> list[logging.LogRecord]:
        return [record for record in self.recorder.records if record.getMessage() == message]


@pytest.fixture
def world() -> Iterator[World]:
    state = World()
    yield state
    state.close()


@given(parsers.parse('the command answers "{answer}"'))
@when(parsers.parse('the command answers "{answer}"'))
def the_command_answers(world: World, answer: str) -> None:
    world.echo.answer, world.echo.error = answer, None


@given(parsers.parse('the command is rejected with "{message}"'))
def the_command_is_rejected(world: World, message: str) -> None:
    world.echo.error = KindError(Kind.INVALID, message)


@given(parsers.re(r'the command fails with the kind "(?P<kind>[^"]+)" and "(?P<message>[^"]+)"$'))
def the_command_fails_with_the_kind(world: World, kind: str, message: str) -> None:
    world.echo.error = KindError(Kind(kind), message)


@given(parsers.parse('the command fails with the kind "{kind}" and "{message}", wrapped in "{outer}"'))
def the_command_fails_wrapped(world: World, kind: str, message: str, outer: str) -> None:
    wrapped = RuntimeError(outer)
    wrapped.__cause__ = KindError(Kind(kind), message)
    world.echo.error = wrapped


@given(parsers.parse('the command fails with "{message}"'))
def the_command_fails(world: World, message: str) -> None:
    world.echo.error = ConnectionError(message)


@given("the command crashes")
def the_command_crashes(world: World) -> None:
    # A bug of the service: what Go calls a panic.
    world.echo.error = TypeError("unsupported operand type(s) for +: 'int' and 'str'")


@given("the command is wrapped with logging")
def the_command_is_wrapped_with_logging(world: World) -> None:
    world.handler = decorate("Echo", HandlerFunc(world.echo.handle), [LoggingMiddleware(world.logger())])


@when(parsers.re(r'I call the test method with "(?P<text>[^"]*)"$'))
def i_call_the_test_method(world: World, text: str) -> None:
    world.call(text)


@when(parsers.parse('I call the test method with "{text}" and the request id "{id}"'))
def i_call_the_test_method_with_a_request_id(world: World, text: str, id: str) -> None:
    world.call(text, (("x-request-id", id),))


@then(parsers.parse('the call should succeed with "{reply}"'))
def the_call_should_succeed_with(world: World, reply: str) -> None:
    assert world.error is None and world.reply == reply, (world.reply, world.error)


@then(parsers.parse('the call should fail with {code} and the message "{message}"'))
def the_call_should_fail_with(world: World, code: str, message: str) -> None:
    error = world.failed()
    assert (error.code(), error.details()) == (grpc.StatusCode[code], message)


@then(parsers.parse('the command should have received "{text}"'))
def the_command_should_have_received(world: World, text: str) -> None:
    assert world.echo.received == [text]


@then("the command should not have been called")
def the_command_should_not_have_been_called(world: World) -> None:
    assert world.echo.received == []


@then(parsers.parse('the log should hold the error "{error}" of "{message}"'))
def the_log_should_hold_the_error(world: World, error: str, message: str) -> None:
    failures = world.records(message)
    assert failures and failures[0].exc_info is not None and str(failures[0].exc_info[1]) == error


@then(parsers.parse('the records should be, in order, "{messages}"'))
def the_records_should_be_in_order(world: World, messages: str) -> None:
    assert [record.getMessage() for record in world.recorder.records] == messages.split("|")


@then(parsers.parse('one "grpc call answered" record should be logged with the method "{method}", the code "{code}", and failed "{failed}"'))
def one_answered_record_should_be_logged_with(world: World, method: str, code: str, failed: str) -> None:
    (record,) = world.records("grpc call answered")
    given = attributes_of(record)
    assert (given["method"], given["code"], str(given["failed"]).lower()) == (method, code, failed)
