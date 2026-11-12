# Source: skill base-plateau-in-python

"""The base plateau's only entry point: the process runs one command when it
starts. Like every entry point, it knows {package}.command (the command types) and
{package}.command.pipeline (the Handler contract and the error kinds), and nothing
below them.

It exists only in the base plateau. When an API variation point is attached
(HTTP, gRPC, CLI, a listener), its Entry replaces the startup Entry in the
"# [entries]" section of main.py and this package is deleted. The exit code
of the process is {package}.api.core.entry's, which stays. It stays only in an
application whose whole job is one command per run (a job started by a
scheduler), or, rarely, beside a server that must run one command once at
start.
"""

import dataclasses
import json
import os
from typing import Any, TextIO

from {package}.command.pipeline import Handler


def env_list(name: str) -> list[str]:
    """The comma-separated environment variable name as a list - the way the
    startup command's input is given, since the process has no other input.
    Unset or empty is an empty list. The startup entry owns this setting, so it
    disappears together with the entry.
    """
    raw = os.environ.get(name, "")
    return raw.split(",") if raw else []


async def run[TCommand, TResult](handler: Handler[TCommand, TResult], command: TCommand, out: TextIO) -> None:
    """Executes command once through handler and writes the result to out as
    one JSON line. It works with any command: TCommand and TResult are taken
    from the handler it is given. On error nothing is written and the error is
    raised.
    """
    result = await handler.handle(command)
    out.write(json.dumps(_plain(result), ensure_ascii=False, separators=(",", ":")) + "\n")
    out.flush()


def _plain(value: Any) -> Any:
    # A result is a dataclass (PlanItemsResult); json writes its fields.
    if dataclasses.is_dataclass(value) and not isinstance(value, type):
        return dataclasses.asdict(value)
    return value
