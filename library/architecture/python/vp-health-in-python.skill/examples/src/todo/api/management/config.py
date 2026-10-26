# Source: skill vp-health-in-python

"""The management entry point's own settings. They are read here, not in
todo.config, so they come and go together with this package."""

import os
from dataclasses import dataclass


@dataclass(frozen=True)
class ManagementConfig:
    listen_port: int = 8081
    """MANAGEMENT_LISTEN_PORT, the port the probes call; default 8081. 0
    picks a free port, which the scenarios use."""


def load_config() -> ManagementConfig:
    """Reads the settings; an unusable value raises ValueError naming its variable."""
    port = 8081
    if raw := os.environ.get("MANAGEMENT_LISTEN_PORT", ""):
        if not raw.isdigit() or int(raw) > 65535:
            raise ValueError(f"management config: MANAGEMENT_LISTEN_PORT {raw!r} is not a port")
        port = int(raw)
    return ManagementConfig(listen_port=port)
