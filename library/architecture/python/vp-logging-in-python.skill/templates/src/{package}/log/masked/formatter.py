# Source: skill vp-logging-in-python

"""The formatter of the logging variation point: the base formatter's line
with the payload kept - masked, cut, by LOG_PAYLOAD - and the request id,
and the warnings the payload calls for."""

import contextvars
import json
import logging
from typing import Any, TextIO

from {package}.log import FAILED_KEY, PAYLOAD_KEY, JsonFormatter, attributes_of, attrs, new_logger, request_id
from {package}.log.masked.config import PAYLOAD_ERRORS, PAYLOAD_OFF, MaskedConfig
from {package}.log.masked.mask import Unparsed, mask, unmask

REQUEST_ID_KEY = "request_id"
"""The attribute the request id of the call is written under."""

NOT_PARSED_MESSAGE = "log payload not parsed"
"""The WARN record written the first time a record of one message holds a
body the mask could not read at one place of its payload."""

UNMASKED_MESSAGE = "log payload unmasked"
"""The WARN record written when the logger is built with masking off, then
once per request - before its first record with a payload - and before every
such record outside a request."""

RECENT_REQUESTS = 4096
"""How many request ids are remembered as warned about unmasking; an older
request may be warned about again."""


class MaskedFormatter(JsonFormatter):
    """Formats a record like the base JsonFormatter, but keeps the payload:
    masked (see mask), cut to MaskedConfig.max_bytes, and only on the records
    MaskedConfig.payload allows. It writes the request id of the call, and
    a record given arguments as its message as written, never with them, so
    no value reaches the log through the message. One record may become
    several lines: a "log payload unmasked" warning before it, and a "log
    payload not parsed" warning after it for each body not yet warned about.
    The handler calls format under its lock, so its memory needs no lock of
    its own.
    """

    def __init__(self, config: MaskedConfig, level: int) -> None:
        super().__init__()
        self._config = config
        self._level = level
        self._warned: set[tuple[str, str]] = set()
        self._unmasked_requests: dict[str, None] = {}  # in the order added

    def format(self, record: logging.LogRecord) -> str:
        given = attributes_of(record)
        message = str(record.msg)  # the message as written: never formatted with its arguments
        values: dict[str, Any] = {}
        if (id := request_id()) is not None:
            values[REQUEST_ID_KEY] = id
        unparsed: list[Unparsed] = []
        for key, value in given.items():
            if key != PAYLOAD_KEY:
                values[key] = value
            elif self._keeps(given.get(FAILED_KEY) is True):
                values[key], unparsed = self._payload(value)
        lines = []
        if self._config.unmasked and PAYLOAD_KEY in values and self._first_unmasked():
            lines.append(self._warning(UNMASKED_MESSAGE))
        lines.append(super().format(_with(record, message, values)))
        if self._level <= logging.WARNING:
            for found in unparsed:
                if (message, found.path) not in self._warned:
                    self._warned.add((message, found.path))
                    lines.append(self._warning(NOT_PARSED_MESSAGE, record=message, field=found.path, bytes=found.bytes))
        return "\n".join(lines)

    def attributes(self, record: logging.LogRecord) -> dict[str, Any]:
        """Every attribute, the payload too: format has masked it already."""
        return attributes_of(record)

    def _keeps(self, failed: bool) -> bool:
        # Whether a record of this outcome keeps its payload.
        if self._config.payload == PAYLOAD_OFF:
            return False
        if self._config.payload == PAYLOAD_ERRORS:
            return failed
        return True

    def _payload(self, value: Any) -> tuple[Any, list[Unparsed]]:
        # The payload masked and, past max_bytes, cut: then the beginning of
        # its JSON as a text, saying how long the whole was. The mask runs
        # before the cut, so a cut never leaves a secret unmasked.
        unparsed: list[Unparsed] = []
        if self._config.unmasked:
            written = unmask(value)
        else:
            written, unparsed = mask(value)
        # Counted in UTF-8 bytes, every character as it is, as every stack counts.
        encoded = json.dumps(written, ensure_ascii=False, separators=(",", ":")).encode()
        if len(encoded) > self._config.max_bytes:
            head = encoded[: self._config.max_bytes].decode(errors="ignore")  # a character cut in two is dropped
            return f"{head}... (cut, {len(encoded)} bytes)", unparsed
        return written, unparsed

    def _first_unmasked(self) -> bool:
        # Whether a record with an unmasked payload is to be warned about:
        # the first of its request, or any outside a request.
        id = request_id()
        if id is None:
            return True
        if id in self._unmasked_requests:
            return False
        self._unmasked_requests.setdefault(id)
        if len(self._unmasked_requests) > RECENT_REQUESTS:
            del self._unmasked_requests[next(iter(self._unmasked_requests))]  # the oldest
        return True

    def _warning(self, message: str, **values: Any) -> str:
        warning = logging.makeLogRecord({"levelno": logging.WARNING})
        given: dict[str, Any] = {}
        if (id := request_id()) is not None:
            given[REQUEST_ID_KEY] = id
        given.update(values)
        return super().format(_with(warning, message, given))


def _with(record: logging.LogRecord, message: str, values: dict[str, Any]) -> logging.LogRecord:
    # A copy of record with message and these attributes: the record itself
    # is shared with every other handler of the logger and stays as it was.
    copy = logging.makeLogRecord(record.__dict__)
    copy.msg, copy.args = message, None
    copy.__dict__.update(attrs(**values))
    return copy


def new_masked_logger(stream: TextIO, level: int, config: MaskedConfig) -> logging.Logger:
    """The application's one logger, as the base new_logger builds it, with
    the MaskedFormatter; with masking off it says so at once, outside any
    request (an empty context), so the warning carries no request id."""
    logger = new_logger(stream, level, MaskedFormatter(config, level))
    if config.unmasked:
        contextvars.Context().run(logger.warning, UNMASKED_MESSAGE)
    return logger
