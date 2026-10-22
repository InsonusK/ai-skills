# Source: skill base-plateau-in-python

"""The logging decorator of every command handler."""

import logging
import time
from datetime import timedelta

from {package}.command.pipeline.handler import Handler, HandlerFunc
from {package}.log import attrs


def with_logging[TCommand, TResult](
    name: str, logger: logging.Logger, next_handler: Handler[TCommand, TResult]
) -> Handler[TCommand, TResult]:
    """A decorator: returns a Handler that calls next_handler and writes two
    records per call, so the log reads top-down as the call ran - "command
    called" with the command as the payload, then "command handled" with the
    duration and the result as the payload, or "command failed" at warning
    level with the error and the command again, so the input of a failure is
    in the log even when only failed records keep their payload. name is the
    command's name in the log (PlanItems). Whether the payload reaches the
    output is the handler's choice ({package}.log), never this decorator's.

    Every cross-cutting step (a unit of work, a metric, ...) follows the same
    pattern, one module per step: take a Handler, return a Handler of the same
    TCommand and TResult. The composition root wraps a handler with as many
    decorators as it needs.
    """

    async def handle(command: TCommand) -> TResult:
        logger.info("command called", extra=attrs(command=name, payload=command))
        started = time.perf_counter_ns()
        try:
            result = await next_handler.handle(command)
        except Exception as error:
            logger.warning(
                "command failed",
                exc_info=error,
                extra=attrs(command=name, duration=_since(started), failed=True, payload=command),
            )
            raise
        logger.info("command handled", extra=attrs(command=name, duration=_since(started), failed=False, payload=result))
        return result

    return HandlerFunc(handle)


def _since(started_ns: int) -> timedelta:
    return timedelta(microseconds=(time.perf_counter_ns() - started_ns) / 1_000)
