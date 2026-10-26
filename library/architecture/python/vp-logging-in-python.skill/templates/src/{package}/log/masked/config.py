# Source: skill vp-logging-in-python

"""The logging variation point's own settings. They are read here, not in
{package}.config, so they come and go together with this package."""

import logging
import os
from dataclasses import dataclass

PAYLOAD_ALWAYS = "always"
"""Every record keeps its payload."""
PAYLOAD_ERRORS = "errors"
"""Only a record with failed=True keeps it."""
PAYLOAD_OFF = "off"
"""No record keeps it, as with the base formatter."""

_BOOLS = {"true": True, "1": True, "false": False, "0": False}


@dataclass(frozen=True)
class MaskedConfig:
    payload: str = PAYLOAD_ALWAYS
    """LOG_PAYLOAD: always (default), errors, or off."""

    max_bytes: int = 4096
    """LOG_PAYLOAD_MAX_BYTES, the size a payload is cut to once written as
    JSON; default 4096, at least 64."""

    unmasked: bool = False
    """LOG_PAYLOAD_UNMASKED: write every payload as it is, for debugging
    only; allowed only with LOG_LEVEL=debug, warned at start and once per
    request. Default false."""


def load_config(level: int) -> MaskedConfig:
    """Reads the settings; an unusable value raises ValueError naming its
    variable. level is the application's LOG_LEVEL: masking is switched off
    only at debug level, so one variable forgotten in a deployment cannot
    write every secret."""
    payload = os.environ.get("LOG_PAYLOAD") or PAYLOAD_ALWAYS
    if payload not in (PAYLOAD_ALWAYS, PAYLOAD_ERRORS, PAYLOAD_OFF):
        raise ValueError(f"logging config: LOG_PAYLOAD {payload!r} is not always, errors, or off")
    max_bytes = 4096
    if raw := os.environ.get("LOG_PAYLOAD_MAX_BYTES"):
        if not raw.isdigit() or int(raw) < 64:
            raise ValueError(f"logging config: LOG_PAYLOAD_MAX_BYTES {raw!r} is not a number of 64 or more")
        max_bytes = int(raw)
    unmasked = False
    if raw := os.environ.get("LOG_PAYLOAD_UNMASKED"):
        if raw.lower() not in _BOOLS:
            raise ValueError(f"logging config: LOG_PAYLOAD_UNMASKED {raw!r} is not true or false")
        unmasked = _BOOLS[raw.lower()]
        if unmasked and level > logging.DEBUG:
            raise ValueError("logging config: LOG_PAYLOAD_UNMASKED is true, which needs LOG_LEVEL=debug")
    return MaskedConfig(payload=payload, max_bytes=max_bytes, unmasked=unmasked)
