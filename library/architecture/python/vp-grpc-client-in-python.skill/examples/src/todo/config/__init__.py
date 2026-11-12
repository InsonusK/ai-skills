"""The base plateau's own settings, read from the environment. A variation
point or an entry point never adds fields here: each reads its settings in its
own package, so removing it removes its settings too.
"""

import logging
import os
from dataclasses import dataclass

_LEVELS = {"debug": logging.DEBUG, "info": logging.INFO, "warn": logging.WARNING, "error": logging.ERROR}


@dataclass(frozen=True)
class Config:
    log_level: int = logging.INFO
    """LOG_LEVEL: debug, info (default), warn, or error."""


def load() -> Config:
    """Reads the settings; an unusable value raises ValueError naming its variable."""
    raw = os.environ.get("LOG_LEVEL", "")
    if not raw:
        return Config()
    level = _LEVELS.get(raw.lower())
    if level is None:
        raise ValueError(f"config: LOG_LEVEL {raw!r} is not debug, info, warn, or error")
    return Config(log_level=level)
