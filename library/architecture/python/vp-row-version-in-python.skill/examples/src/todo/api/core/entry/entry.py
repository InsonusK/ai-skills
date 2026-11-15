# Source: skill base-plateau-in-python

"""Runs the application's entry points - the startup command, an HTTP
server, a gRPC server, the health endpoints - together in one process, and
turns how they ended into the process exit code. It is what lets every
variation point attach by adding or replacing one Entry in the "# [entries]"
section of main.py: no variation point writes code to run itself beside
another.
"""

import asyncio
import logging
from collections.abc import Callable, Coroutine
from typing import Any
from dataclasses import dataclass

from todo.command.pipeline import Kind, kind_of
from todo.log import attrs


@dataclass(frozen=True)
class Entry:
    """One entry point of the application."""

    name: str
    """How logs name the entry point, e.g. "http"."""

    run: Callable[[asyncio.Event], Coroutine[Any, Any, None]]
    """Serves until the event it is given is set and then returns, or raises
    the error that stopped it early. A one-shot entry (the startup command)
    returns when its work is done."""

    reports_errors: bool = False
    """True for an entry that shows its own error to whoever called it - the
    CLI prints "Error: ..." on stderr - so run_all does not log that error a
    second time. The error still sets the exit code."""


async def run_all(logger: logging.Logger, stopping: asyncio.Event, *entries: Entry) -> BaseException | None:
    """Runs every entry at once and waits until the first one ends - its work
    done, its error, or stopping set on SIGTERM. Then it tells the others to
    stop, waits for them, and returns the first error any entry raised, or
    None. So one failed entry stops the whole process instead of leaving it
    half-alive, and a one-shot entry ends the process when it is done.
    """
    stop = asyncio.Event()  # what the entries see: set when the first ends, or on termination
    running: dict[asyncio.Task[None], Entry] = {asyncio.create_task(entry.run(stop), name=entry.name): entry for entry in entries}
    terminated = asyncio.create_task(stopping.wait())
    waiting: set[asyncio.Task[Any]] = {*running, terminated}
    first: BaseException | None = None
    while waiting - {terminated}:
        done, _ = await asyncio.wait(waiting, return_when=asyncio.FIRST_COMPLETED)
        stop.set()  # the first entry ended, or the process is told to terminate: stop the others
        waiting -= done
        for task in done - {terminated}:
            error = task.exception()
            if error is None:
                continue
            entry = running[task]
            if not entry.reports_errors:
                logger.error("entry point stopped with an error", exc_info=error, extra=attrs(entry=entry.name))
            if first is None:
                first = error
    terminated.cancel()
    return first


EXIT_OK = 0
"""Every entry ended without an error."""
EXIT_FAILURE = 1
"""The service failed, or a dependency is unavailable: a retry may help."""
EXIT_REJECTED = 2
"""The request was rejected: its error has a kind other than unavailable."""


def exit_code(error: BaseException | None) -> int:
    """The exit code of the error run_all returned, by its kind (skill
    error-kinds): None is EXIT_OK; unavailable, and an error with no kind - a
    server that could not start - are EXIT_FAILURE; any other kind, which only
    a one-shot command raises, is EXIT_REJECTED.
    """
    if error is None:
        return EXIT_OK
    kind = kind_of(error)
    if kind is None or kind is Kind.UNAVAILABLE:
        return EXIT_FAILURE
    return EXIT_REJECTED
