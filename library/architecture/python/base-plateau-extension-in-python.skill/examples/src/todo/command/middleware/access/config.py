"""The access variation point's own settings. They are read here, not in
todo.config, so they come and go together with this package."""

import os
from dataclasses import dataclass, field

from todo.command.middleware.access.identity import Identity

DEFAULT_MAX_BATCH = 100
"""The batch limit when ACCESS_MAX_BATCH is not set."""


@dataclass(frozen=True)
class AccessConfig:
    tokens: dict[str, Identity] = field(default_factory=dict)
    """ACCESS_TOKENS: a bearer token to its caller, given as a comma-separated
    list of token=user or token=user:role+role, e.g.
    "t-alice=alice:planner,t-bob=bob". Empty by default: no caller is known."""

    max_batch: int = DEFAULT_MAX_BATCH
    """ACCESS_MAX_BATCH: the most items a command marked batch_limit.Batch may
    hold."""


def load_config() -> AccessConfig:
    """Reads the settings; an unusable value raises ValueError naming its variable."""
    tokens: dict[str, Identity] = {}
    if raw := os.environ.get("ACCESS_TOKENS", ""):
        for item in raw.split(","):
            token, found, given = item.strip().partition("=")
            user, _, roles = given.partition(":")
            if not found or not token or not user:
                raise ValueError(f"access config: ACCESS_TOKENS item {item!r} is not token=user[:role+role]")
            tokens[token] = Identity(user=user, roles=tuple(roles.split("+")) if roles else ())
    max_batch = DEFAULT_MAX_BATCH
    if raw := os.environ.get("ACCESS_MAX_BATCH", ""):
        if not raw.isdigit() or int(raw) < 1:
            raise ValueError(f"access config: ACCESS_MAX_BATCH {raw!r} is not a positive number")
        max_batch = int(raw)
    return AccessConfig(tokens=tokens, max_batch=max_batch)
