# Source: skill base-plateau-in-python

"""The one way the application writes logs.

Every part that logs - the command logging middleware, an entry point, later a
client - writes the same records through a logging.Logger with fixed text and
its values as attributes: logger.info("command called",
extra=attrs(command="PlanItems", payload=command)). Which attributes reach the
output is decided only by the handler the composition root builds:

- new_logger, the base plateau's: one JSON line per record, the payload
  attribute dropped - who, what, how long, how it ended; never the data;
- the logging variation point's: the payload too, masked, with the request id
  of the call.

So a part never knows which handler runs, and attaching the logging
variation point changes what is written, not the code that writes it.
"""

import json
import logging
import traceback
from datetime import datetime, timedelta
from typing import Any, TextIO

from todo.domain.entity import kind_of

PAYLOAD_KEY = "payload"
"""The data of a call: a command's parameters, a request's query, headers, and
body, a call's arguments. The base handler drops it, so no data - and no
secret in it - reaches the output unless the logging variation point masks it.
"""

FAILED_KEY = "failed"
"""True on a record of a call that ended with an error, so a handler can keep
the payload of failed calls only."""

ATTRS = "attrs"
"""The one field of a LogRecord that holds a record's attributes, in order."""


def attrs(**values: Any) -> dict[str, Any]:
    """The extra= of a record: its attributes, in the order given -
    attrs(command="PlanItems", failed=False, payload=result).
    """
    return {ATTRS: values}


def attributes_of(record: logging.LogRecord) -> dict[str, Any]:
    """The attributes a part gave the record with attrs(); none for a record
    of a library, which logs without them."""
    given = getattr(record, ATTRS, None)
    return given if isinstance(given, dict) else {}


# The level names of the Go base plateau (log/slog).
_LEVEL_NAMES = {logging.DEBUG: "DEBUG", logging.INFO: "INFO", logging.WARNING: "WARN"}


def level_name(level: int) -> str:
    return _LEVEL_NAMES.get(level, "DEBUG" if level < logging.INFO else "ERROR")


def json_value(value: Any) -> Any:
    """A value of an attribute as JSON: a duration in nanoseconds, as every
    stack writes it; an exception as its message; anything else that JSON has
    no form for as its text, so a value never loses the record."""
    if isinstance(value, timedelta):
        return (value.days * 86_400 + value.seconds) * 1_000_000_000 + value.microseconds * 1_000
    if isinstance(value, BaseException):
        return str(value)
    return value


class JsonFormatter(logging.Formatter):
    """One record as one JSON line: time, level, msg, the attributes, then
    the error - error with its message, and exception with its stack when it
    carries no kind (a failure of the service is debugged from its stack; a
    rejected request needs none). The fields are those of every stack, so one
    log pipeline reads them all.

    The base plateau's formatter leaves the payload attribute out; the
    logging variation point's formatter overrides attributes() to write it
    masked, and keeps every other field as written here.
    """

    def format(self, record: logging.LogRecord) -> str:
        line: dict[str, Any] = {
            "time": datetime.fromtimestamp(record.created).astimezone().isoformat(timespec="milliseconds"),
            "level": level_name(record.levelno),
            "msg": record.getMessage(),
        }
        for key, value in self.attributes(record).items():
            line[key] = json_value(value)
        if record.exc_info and record.exc_info[1] is not None:
            error = record.exc_info[1]
            line["error"] = str(error)
            if kind_of(error) is None:
                line["exception"] = "".join(traceback.format_exception(error)).rstrip()
        return json.dumps(line, ensure_ascii=False, separators=(",", ":"), default=str)

    def attributes(self, record: logging.LogRecord) -> dict[str, Any]:
        """The attributes written: the base plateau drops the payload."""
        return {key: value for key, value in attributes_of(record).items() if key != PAYLOAD_KEY}


def new_logger(stream: TextIO, level: int, formatter: logging.Formatter | None = None) -> logging.Logger:
    """The application's one logger, writing to stream (standard error) at
    level and above through formatter - the base plateau's JsonFormatter
    unless one is given. It is a logger of its own, not a child of the root
    logger, so every part writes through the composition root's handler and
    nothing else.
    """
    handler = logging.StreamHandler(stream)
    handler.setFormatter(formatter or JsonFormatter())
    logger = logging.Logger(__name__.partition(".")[0], level)  # named after the application's top package
    logger.addHandler(handler)
    return logger
