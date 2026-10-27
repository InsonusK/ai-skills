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
from todo.api import startup
from todo.api.core.entry import Entry, exit_code, run_all
from todo.command import PlanRemoteItemsCommand, build_handlers
from todo.command.middleware.logging import LoggingMiddleware
from todo.infrastructure.client import todo_api
from todo.log import attrs, new_logger
from todo.service import RemotePlanner


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

    # [services] - infrastructure and the services over it. The todo API of
    # another service (TODO_API_HOST=mock answers from its emulator) and the
    # service that plans items with it; settings it cannot use stop the start.
    try:
        todo_api_client = await todo_api.new(logger)
    except ValueError as error:
        logger.error("startup failed", exc_info=error)
        return 1
    remote_planner = RemotePlanner(todo_api_client)

    # [handlers] - the handler of every command, built by the command registry
    # and wrapped by this command middleware, the first the outermost. A
    # variation point adds its middleware to this list at the place its skill
    # names.
    handlers = build_handlers(
        [
            LoggingMiddleware(logger),
        ],
        remote_planner=remote_planner,
    )

    # [entries] - every entry point, one Entry each, run together by run_all
    # until the first one ends. The startup command plans its titles with the
    # todo service; an API variation point replaces its Entry. The clients
    # close when every entry has ended.
    try:
        ended_by = await run_all(
            logger,
            stopping,
            Entry(
                "startup",
                lambda _: startup.run(
                    handlers.plan_remote_items, PlanRemoteItemsCommand(titles=startup.env_list("STARTUP_TITLES")), stdout
                ),
            ),
        )
    finally:
        await todo_api_client.aclose()
    return exit_code(ended_by)
