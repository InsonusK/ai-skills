"""The fixed code of the command line through a test resource of its own,
`todo test echo|plain --text ...`, over a fake command."""

import asyncio
import io
import json
import logging
import shlex
from dataclasses import dataclass, field
from typing import Any

import click
import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.cli.pipeline import Binding, Resource, command, run
from todo.command.pipeline import Kind, KindError, kind_of
from todo.log import attributes_of

scenarios("../features/cli.feature")


class Recorder(logging.Handler):
    """Keeps every record with all its attributes, the payload too."""

    def __init__(self) -> None:
        super().__init__()
        self.records: list[logging.LogRecord] = []

    def emit(self, record: logging.LogRecord) -> None:
        self.records.append(record)


@dataclass
class EchoCommand:
    """The fake command of the test resource: it answers or raises what the
    scenario set, and records what it received."""

    answer: str = ""
    error: Exception | None = None
    received: list[str] = field(default_factory=list)

    async def handle(self, command: str) -> str:
        self.received.append(command)
        if self.error is not None:
            raise self.error
        return self.answer


@dataclass(frozen=True)
class EchoOutput:
    text: str


@dataclass
class World:
    echo: EchoCommand = field(default_factory=EchoCommand)
    crash: bool = False
    recorder: Recorder = field(default_factory=Recorder)
    stdout: str = ""
    stderr: str = ""
    error: Exception | None = None

    def resource(self) -> Resource:
        # `todo test echo` with a text form, `todo test plain` without one.
        def bind() -> Binding[str]:
            def read(flags: Any) -> str:
                if not flags["text"]:
                    raise ValueError("--text is required")
                return str(flags["text"])

            return Binding(params=[click.Option(["--text"], help="the text to echo")], read=read)

        def encode(text: str) -> EchoOutput:
            if self.crash:
                raise TypeError("unsupported operand type(s) for +: 'int' and 'str'")
            return EchoOutput(text=text)

        return Resource("test", "A resource of the scenarios", [
            command("echo", "Echo --text", self.echo, bind, encode, lambda text: f"echo: {text}"),
            command("plain", "Echo --text, no text form", self.echo, bind, encode, None),
        ])

    def run(self, args: str) -> None:
        logger = logging.Logger("test", logging.DEBUG)
        logger.addHandler(self.recorder)
        stdout, stderr = io.StringIO(), io.StringIO()
        try:
            asyncio.run(run(logger, "todo", "1.2.3", shlex.split(args), [self.resource()], stdout, stderr))
        except Exception as error:
            self.error = error
        self.stdout, self.stderr = stdout.getvalue(), stderr.getvalue()
        print(f"when: todo {args} -> stdout {self.stdout!r}, stderr {self.stderr!r}, error {self.error!r}")

    def records(self, message: str) -> list[logging.LogRecord]:
        return [record for record in self.recorder.records if record.getMessage() == message]

    def payload(self, message: str) -> dict[str, Any]:
        (record,) = self.records(message)
        payload = attributes_of(record)["payload"]
        assert isinstance(payload, dict)
        return payload


@pytest.fixture
def world() -> World:
    return World()


@given(parsers.parse('the command answers "{answer}"'))
def the_command_answers(world: World, answer: str) -> None:
    world.echo.answer = answer


@given(parsers.parse('the command is rejected with the kind "{kind}" and "{message}"'))
def the_command_is_rejected(world: World, kind: str, message: str) -> None:
    world.echo.error = KindError(Kind(kind), message)


@given(parsers.parse('the command fails with "{message}"'))
def the_command_fails(world: World, message: str) -> None:
    world.echo.error = ConnectionError(message)


@given("the output of the action crashes")
def the_output_crashes(world: World) -> None:
    # A bug of the action itself - its encode raises - not an error of the command.
    world.crash = True


@when(parsers.re(r'I run "(?P<args>[^"]*)"'))
def i_run(world: World, args: str) -> None:
    world.run(args)


@then(parsers.re(r"stdout should be the line '(?P<line>[^']*)'"))
def stdout_should_be_the_line(world: World, line: str) -> None:
    assert world.stdout == line + "\n"


@then(parsers.parse('stdout should be indented JSON with "{key}" = "{value}"'))
def stdout_should_be_indented_json(world: World, key: str, value: str) -> None:
    assert "\n  " in world.stdout and json.loads(world.stdout)[key] == value


@then("stdout should be empty")
def stdout_should_be_empty(world: World) -> None:
    assert world.stdout == ""


@then(parsers.parse('stdout should mention "{text}"'))
def stdout_should_mention(world: World, text: str) -> None:
    assert text in world.stdout


@then(parsers.parse('stderr should mention "{text}"'))
def stderr_should_mention(world: World, text: str) -> None:
    assert text in world.stderr


@then(parsers.parse('the command should have received "{text}"'))
def the_command_should_have_received(world: World, text: str) -> None:
    assert world.echo.received == [text]


@then("the command should not have been called")
def the_command_should_not_have_been_called(world: World) -> None:
    assert world.echo.received == []


@then("running should end without an error")
def running_should_end_without_an_error(world: World) -> None:
    assert world.error is None, world.error


@then(parsers.parse('running should fail with the kind "{kind}"'))
def running_should_fail_with_the_kind(world: World, kind: str) -> None:
    assert kind_of(world.error) == kind, world.error


@then("running should fail with no kind")
def running_should_fail_with_no_kind(world: World) -> None:
    assert world.error is not None and kind_of(world.error) is None, world.error


@then(parsers.parse('the records should be, in order, "{messages}"'))
def the_records_should_be_in_order(world: World, messages: str) -> None:
    assert [record.getMessage() for record in world.recorder.records] == messages.split("|")


@then(parsers.parse('one "cli call answered" record should be logged for the action "{action}", failed "{failed}"'))
def one_answered_record(world: World, action: str, failed: str) -> None:
    (record,) = world.records("cli call answered")
    given = attributes_of(record)
    assert (given["action"], str(given["failed"]).lower()) == (action, failed)


@then(parsers.parse('the "cli call received" record should hold the flag "{name}" = "{value}"'))
def the_received_record_should_hold_the_flag(world: World, name: str, value: str) -> None:
    assert world.payload("cli call received")["flags"][name] == value


@then(parsers.parse('the "cli call answered" record should hold the output "{output}"'))
def the_answered_record_should_hold_the_output(world: World, output: str) -> None:
    assert world.payload("cli call answered")["output"] == output + "\n"


@then(parsers.parse('the "cli call answered" record should hold the arguments "{args}"'))
def the_answered_record_should_hold_the_arguments(world: World, args: str) -> None:
    assert world.payload("cli call answered")["args"] == args.split()
