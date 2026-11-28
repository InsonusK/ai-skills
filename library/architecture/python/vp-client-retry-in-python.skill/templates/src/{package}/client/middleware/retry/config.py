# Source: skill vp-client-retry-in-python

"""The retry settings of one API, beside the {API} group of skill api-client."""

import os
import re
from dataclasses import dataclass
from datetime import timedelta

DEFAULT_ATTEMPTS = 3
"""The attempts a call gets in all when {API}_RETRY_ATTEMPTS is not set."""

DEFAULT_BACKOFF = timedelta(milliseconds=100)
"""The wait before the second attempt when {API}_RETRY_BACKOFF is not set."""

# A number and ms, s, or m - the form every stack reads.
_DURATION = re.compile(r"^(?P<number>[0-9]+(\.[0-9]+)?)(?P<unit>ms|s|m)$")
_UNITS = {"ms": "milliseconds", "s": "seconds", "m": "minutes"}


@dataclass(frozen=True)
class RetryConfig:
    attempts: int = DEFAULT_ATTEMPTS
    """{API}_RETRY_ATTEMPTS: the attempts a call gets in all, the first included; 1 sends no repeat."""
    backoff: timedelta = DEFAULT_BACKOFF
    """{API}_RETRY_BACKOFF: the wait before the second attempt, doubled before
    each next one, each wait between half and one and a half of it."""


def load_config(prefix: str) -> RetryConfig:
    """Reads {prefix}_RETRY_ATTEMPTS and {prefix}_RETRY_BACKOFF; an unusable
    value raises ValueError naming its variable."""
    name = prefix.replace("_", "-").lower()
    attempts = DEFAULT_ATTEMPTS
    if raw := os.environ.get(f"{prefix}_RETRY_ATTEMPTS", ""):
        if not raw.isdecimal() or int(raw) < 1:
            raise ValueError(f"{name} config: {prefix}_RETRY_ATTEMPTS {raw!r} is not a count of 1 or more")
        attempts = int(raw)
    backoff = DEFAULT_BACKOFF
    if raw := os.environ.get(f"{prefix}_RETRY_BACKOFF", ""):
        match = _DURATION.match(raw)
        parsed = timedelta(**{_UNITS[match["unit"]]: float(match["number"])}) if match else timedelta(0)
        if parsed <= timedelta(0):
            raise ValueError(f"{name} config: {prefix}_RETRY_BACKOFF {raw!r} is not a duration like 100ms or 1s")
        backoff = parsed
    return RetryConfig(attempts=attempts, backoff=backoff)
