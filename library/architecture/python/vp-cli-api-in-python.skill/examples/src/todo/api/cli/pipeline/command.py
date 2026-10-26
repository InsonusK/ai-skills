# Source: skill vp-cli-api-in-python

"""The one adapter every action of the command line is built with."""

import dataclasses
import json
from collections.abc import Callable, Coroutine, Mapping, Sequence
from dataclasses import dataclass
from typing import Any

import logging

import click

from todo.api.cli.pipeline.run_log import log_received
from todo.command.pipeline import Handler, Kind, KindError

OUTPUT_JSON = "json"
OUTPUT_TEXT = "text"
"""The values of the --output flag every action has."""


@dataclass(frozen=True)
class Binding[TCommand]:
    """The flags of one action and how they become its command."""

    params: Sequence[click.Parameter]
    """The action's flags, e.g. click.Option(["--title", "titles"], multiple=True)."""

    read: Callable[[Mapping[str, Any]], TCommand]
    """Turns the parsed flags, by their names, into the command; a ValueError
    it raises is a usage error (exit code 2) and the handler is not called."""


@dataclass
class RunState:
    """What one run shares between run() and the action it runs: the logger,
    the action's path, the flags given, and what it printed. run() puts it in
    click's context object; the action fills it."""

    logger: logging.Logger
    action: str
    flags: dict[str, Any] = dataclasses.field(default_factory=dict)
    printed: list[str] = dataclasses.field(default_factory=list)


class HandlerError(Exception):
    """Marks an error raised by an action's handler, so run() tells it from an
    error of the command line and of the fixed code."""

    def __init__(self, error: Exception) -> None:
        super().__init__(str(error))
        self.error = error


def command[TCommand, TResult](
    name: str,
    short: str,
    handler: Handler[TCommand, TResult],
    bind: Callable[[], Binding[TCommand]],
    encode: Callable[[TResult], Any],
    text: Callable[[TResult], str] | None,
) -> click.Command:
    """Turns one command handler into one action of the command line. It is
    written once for every command:

    1. bind declares the action's flags and read, which turns the parsed
       flags into the command (TCommand); a ValueError of read is a usage
       error (exit code 2) and the handler is not called;
    2. the handler runs the command; its error goes back to run(), which
       prints it and keeps its kind;
    3. encode turns the result (TResult) into the action's output type - a
       dataclass - written as one JSON line, or, with --output text, text
       writes it for a person; when text is None, --output text writes
       indented JSON.

    bind, encode, and text belong to the action (its resource's module in
    endpoint/, e.g. endpoint/items.py): the flags and the output are the
    command line's contract, never the command's types.

    click calls the action while it parses; the action returns a coroutine,
    which run() awaits, so the handler runs on the application's event loop.
    """
    binding = bind()

    def parsed(output: str, **values: Any) -> Coroutine[Any, Any, None]:
        ctx = click.get_current_context()
        state: RunState = ctx.obj
        state.action = ctx.command_path
        state.flags = {
            key: list(value) if isinstance(value, tuple) else value
            for key, value in {"output": output, **values}.items()
            if ctx.get_parameter_source(key) is click.core.ParameterSource.COMMANDLINE
        }
        log_received(state.logger, state.action, state.flags)
        return act(state, ctx.command_path, output, values)

    async def act(state: RunState, path: str, output: str, values: Mapping[str, Any]) -> None:
        if output not in (OUTPUT_JSON, OUTPUT_TEXT):
            raise KindError(Kind.INVALID, f"--output must be json or text, not {output!r} (see {path} --help)")
        try:
            cmd = binding.read(values)
        except ValueError as error:
            raise KindError(Kind.INVALID, f"{error} (see {path} --help)") from error
        try:
            result = await handler.handle(cmd)
        except Exception as error:
            raise HandlerError(error) from error
        state.printed.append(_written(output, encode(result), None if text is None else lambda: text(result)))

    return click.Command(
        name,
        help=short,
        short_help=short,
        params=[*binding.params, click.Option(["--output"], default=OUTPUT_JSON, show_default=True, help="output format: json or text")],
        callback=parsed,
    )


def _written(output: str, encoded: Any, text: Callable[[], str] | None) -> str:
    # The result in the chosen output format, with its line end.
    value = dataclasses.asdict(encoded) if dataclasses.is_dataclass(encoded) and not isinstance(encoded, type) else encoded
    if output == OUTPUT_TEXT:
        if text is not None and (written := text()):
            return written + "\n"
        return json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    return json.dumps(value, ensure_ascii=False, separators=(",", ":")) + "\n"
