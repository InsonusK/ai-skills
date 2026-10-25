# Source: skill base-plateau-in-python

"""The logging decorator of every command handler."""

import logging
import time
from datetime import timedelta

from {package}.command.pipeline.decorator import Decorator, Next
from {package}.log import attrs


def with_logging(logger: logging.Logger) -> Decorator:
    """The decorator that logs every command: two records per call, so the log
    reads top-down as the call ran - "command called" with the command as the
    payload, then "command handled" with the duration and the result as the
    payload, or "command failed" at warning level with the error and the
    command again, so the input of a failure is in the log even when only
    failed records keep their payload. The command's name is the "command"
    attribute. Whether the payload reaches the output is the handler's choice
    ({package}.log), never this decorator's.

    Every cross-cutting step (a unit of work, a metric, an access check, ...)
    follows the same pattern: a function that takes what the step needs and
    returns a Decorator. The composition root lists them; the command registry
    wraps every handler with the list (see decorate).
    """

    async def log_calls(name: str, command: object, call_next: Next) -> object:
        logger.info("command called", extra=attrs(command=name, payload=command))
        started = time.perf_counter_ns()
        try:
            result = await call_next(command)
        except Exception as error:
            logger.warning(
                "command failed",
                exc_info=error,
                extra=attrs(command=name, duration=_since(started), failed=True, payload=command),
            )
            raise
        logger.info("command handled", extra=attrs(command=name, duration=_since(started), failed=False, payload=result))
        return result

    return log_calls


def _since(started_ns: int) -> timedelta:
    return timedelta(microseconds=(time.perf_counter_ns() - started_ns) / 1_000)
