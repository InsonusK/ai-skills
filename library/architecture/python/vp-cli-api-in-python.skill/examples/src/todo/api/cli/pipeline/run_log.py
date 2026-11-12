# Source: skill vp-cli-api-in-python

"""The records of a run: received when its arguments are parsed, answered
when it ends."""

import logging
import time
from datetime import timedelta
from typing import Any

from todo.log import attrs


def log_received(logger: logging.Logger, action: str, flags: dict[str, Any]) -> None:
    """The first record of a run once its arguments are parsed - "cli call
    received" with the action and the flags given as the payload."""
    logger.info("cli call received", extra=attrs(action=action, payload={"flags": flags}))


def log_answered(logger: logging.Logger, action: str, started: int, failed: bool, output: str, args: list[str]) -> None:
    """The last record of a run - "cli call answered" with the action run,
    the duration, failed, and as the payload the output as printed; a failed
    run repeats the arguments given, so they are in the log even when only
    failed records keep their payload, and even when they could not be
    parsed and no "cli call received" was written."""
    sent: dict[str, Any] = {"output": output}
    if failed:
        sent["args"] = args
    duration = timedelta(microseconds=(time.perf_counter_ns() - started) / 1_000)
    logger.info("cli call answered", extra=attrs(action=action, duration=duration, failed=failed, payload=sent))
