"""The composition root: the one place that builds concrete types and wires
them together. A variation point joins the application by adding lines to the
marked sections below - never by editing another module.
"""

import asyncio
import signal
import sys
from importlib.metadata import version
from typing import TextIO

from todo import config
from todo.api import grpc as grpc_api
from todo.api import http as http_api
from todo.api.core.entry import exit_code, run_all
from todo.command import build_handlers
from todo.command.middleware.logging import LoggingMiddleware
from todo.infrastructure.memory import Lists
from todo.log import attrs, new_logger
from todo.service import ItemLists


def main() -> None:
    sys.exit(asyncio.run(_serve()))


async def _serve() -> int:
    # stopping is set on SIGINT or SIGTERM, so a long-running entry point
    # (an HTTP server, a listener) stops gracefully.
    stopping = asyncio.Event()
    loop = asyncio.get_running_loop()
    for signal_number in (signal.SIGINT, signal.SIGTERM):
        loop.add_signal_handler(signal_number, stopping.set)
    return await run(sys.stdout, sys.stderr, stopping)


async def run(stdout: TextIO, stderr: TextIO, stopping: asyncio.Event) -> int:
    try:
        settings = config.load()
    except ValueError as error:
        new_logger(stderr, config.Config().log_level).error("startup failed", exc_info=error)
        return 1
    # The handler decides what reaches the log: the base plateau's leaves out
    # every payload; the logging variation point replaces this line.
    logger = new_logger(stderr, settings.log_level)
    # The version of the installed package: project.version of pyproject.toml
    # (skill project-version-in-python).
    logger.info("starting", extra=attrs(version=version("todo")))

    # [services] - infrastructure and the services over it. The lists live in
    # memory in this example; the DB variation point replaces this line.
    item_lists = ItemLists(Lists())

    # [handlers] - the handler of every command, built by the command registry
    # over the services and wrapped by this command middleware, the first the
    # outermost. A variation point adds its middleware to this list at the
    # place its skill names.
    handlers = build_handlers(
        [
            LoggingMiddleware(logger),
        ],
        lists=item_lists,
    )

    # [entries] - every entry point, one Entry each, run together by run_all
    # until the first one ends. The gRPC and HTTP forms of one proto contract
    # serve the same handlers; the health variation point adds one more.
    ended_by = await run_all(
        logger,
        stopping,
        # the gRPC API: its calls wrapped by these interceptors, the first the
        # outermost, inside the call log
        grpc_api.entry(handlers, logger, []),
        # its HTTP form: its contract routes wrapped by these middlewares, the
        # first the outermost, inside the request log
        http_api.entry(handlers, logger, []),
    )
    return exit_code(ended_by)
