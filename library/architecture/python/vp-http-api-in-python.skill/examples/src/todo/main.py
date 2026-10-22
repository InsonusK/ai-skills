"""The composition root: the one place that builds concrete types and wires
them together. A variation point joins the application by adding lines to the
marked sections below - never by editing another module.
"""

import asyncio
import signal
import sys
from typing import TextIO

from todo import config
from todo.api import http as http_api
from todo.api.core.entry import Entry, exit_code, run_all
from todo.api.http.endpoint import ItemsHandler
from todo.command import PlanItemsCommand, PlanItemsHandler, PlanItemsResult
from todo.command.pipeline import Handler, with_logging
from todo.log import attrs, new_logger
from todo.version import VERSION


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
    logger.info("starting", extra=attrs(version=VERSION))

    # [services] - infrastructure and the services over it; none in the base plateau.

    # [handlers] - one per command, wrapped by its decorators.
    plan_items: Handler[PlanItemsCommand, PlanItemsResult] = PlanItemsHandler()
    plan_items = with_logging("PlanItems", logger, plan_items)

    # [entries] - every entry point, one Entry each, run together by run_all
    # until the first one ends. The HTTP variation point replaced the startup
    # Entry with its own; the health variation point adds one.
    endpoints = http_api.Endpoints(items=ItemsHandler(plan_items=plan_items))
    ended_by = await run_all(
        logger,
        stopping,
        Entry("http", lambda stop: http_api.run(endpoints, logger, stop)),
    )
    return exit_code(ended_by)
