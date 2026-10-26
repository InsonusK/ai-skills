# Source: skill vp-cli-api-in-python

"""The command line: `{app} {resource} {action} [flags]` built on click, one
run, and the answer of every failure as one error line and the exit code of
its kind."""

import asyncio
import contextlib
import io
import logging
import time
from collections.abc import Sequence
from dataclasses import dataclass, field
from typing import TextIO

import click

from todo.api.cli.pipeline.command import HandlerError, RunState
from todo.api.cli.pipeline.run_log import log_answered
from todo.command.pipeline import Kind, KindError
from todo.log import request_id_scope

INTERNAL_MESSAGE = "internal error"
"""All a person learns of an action that crashed; the crash goes to the log."""


@dataclass(frozen=True)
class Resource:
    """One resource of the command line - `todo items ...` - with its
    actions, built by command() in the resource's module of endpoint/."""

    name: str
    """The resource as typed: "items"."""

    short: str
    """Its one-line help."""

    actions: Sequence[click.Command] = field(default_factory=list)
    """Its commands: `todo items plan`."""


class InternalError(Exception):
    """The error of an action that crashed: its message is INTERNAL_MESSAGE,
    its cause the crash."""

    def __init__(self) -> None:
        super().__init__(INTERNAL_MESSAGE)


async def run(
    logger: logging.Logger,
    name: str,
    version: str,
    args: Sequence[str],
    resources: Sequence[Resource],
    stdout: TextIO,
    stderr: TextIO,
) -> None:
    """The CLI entry point's run: builds the command line name from the
    resources, runs the action args name, and raises its error. An error of
    the action's handler is raised as it is, so the process exit code
    follows its kind; an error of the command line itself - an unknown
    action, a bad flag - is a KindError of kind invalid (exit code 2); an
    action that crashed is an InternalError with no kind (exit code 1),
    logged with its stack. Every error is printed once, "Error: ..." on
    stderr; the result and the help go to stdout. Two records are logged per
    run - "cli call received" once the arguments are parsed, "cli call
    answered" when it ends - inside a request id of the run's own. The
    answer of every failure is fixed here, not left to the composition root
    (ADR fixed-middleware-inside-the-entry of skill base-plateau-extension).
    """
    started = time.perf_counter_ns()
    root = _root(name, version, resources)
    state = RunState(logger=logger, action=name)
    printed = io.StringIO()  # help and --version, which click prints itself
    error: Exception | None = None
    with request_id_scope(None):
        try:
            with contextlib.redirect_stdout(printed):
                returned = root.main(list(args), prog_name=name, standalone_mode=False, obj=state)
            if asyncio.iscoroutine(returned):
                await returned
        except click.exceptions.NoArgsIsHelpError as shown:
            printed.write(shown.ctx.get_help() + "\n")  # an application or a resource with no action
        except click.UsageError as usage:
            path = usage.ctx.command_path if usage.ctx is not None else name
            state.action = path
            error = KindError(Kind.INVALID, f"{usage.format_message()} (see {path} --help)")
        except HandlerError as handled:
            error = handled.error
        except KindError as rejected:
            error = rejected  # a usage error the action found in its flags
        except Exception as crash:
            logger.error("cli action crashed", exc_info=crash)
            error = InternalError()
            error.__cause__ = crash
        output = printed.getvalue() + "".join(state.printed)
        stdout.write(output)
        log_answered(logger, state.action, started, error is not None, output, list(args))
    if error is not None:
        print(f"Error: {error}", file=stderr)
        raise error


def _root(name: str, version: str, resources: Sequence[Resource]) -> click.Group:
    # `name` with a group per resource and its actions, and --version.
    root = click.Group(name, help=f"{name} - the command line of the application", no_args_is_help=True)
    click.version_option(version, prog_name=name, message="%(prog)s version %(version)s")(root)
    for resource in resources:
        group = click.Group(resource.name, help=resource.short, short_help=resource.short, no_args_is_help=True)
        for action in resource.actions:
            group.add_command(action)
        root.add_command(group)
    return root
