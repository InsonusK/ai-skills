# Source: skill vp-grpc-client-in-python

"""The settings of one gRPC API: the {API} group of skill api-client."""

import os
import re
from dataclasses import dataclass
from datetime import timedelta

MOCK = "mock"
"""The value of {API}_HOST that runs the client against the emulator of its API inside the process."""

DEFAULT_TIMEOUT = timedelta(seconds=5)
"""Bounds a call when {API}_TIMEOUT is not set."""

# A number and ms, s, or m - the form every stack reads.
_TIMEOUT = re.compile(r"^(?P<number>[0-9]+(\.[0-9]+)?)(?P<unit>ms|s|m)$")
_UNITS = {"ms": "milliseconds", "s": "seconds", "m": "minutes"}


@dataclass(frozen=True)
class GrpcConfig:
    name: str
    """How the log names the API: the prefix in lower case with "-", TODO_API is todo-api."""
    host: str
    """{API}_HOST; MOCK runs the client against its emulator."""
    port: int
    """{API}_GRPC_PORT; default 443 with TLS, 80 without."""
    tls: bool = False
    """{API}_TLS: connect over TLS; default false."""
    timeout: timedelta = DEFAULT_TIMEOUT
    """{API}_TIMEOUT: the longest one call may take; default 5s."""

    @property
    def mocked(self) -> bool:
        """Whether the client runs against its emulator."""
        return self.host == MOCK

    @property
    def target(self) -> str:
        """host:port, the address the channel connects to."""
        return f"{self.host}:{self.port}"


def load_config(prefix: str) -> GrpcConfig:
    """Reads the {API} group of prefix ("TODO_API") from the environment; a
    missing or unusable value raises ValueError naming its variable."""
    name = prefix.replace("_", "-").lower()
    host = os.environ.get(f"{prefix}_HOST", "")
    if not host:
        raise ValueError(f"{name} config: {prefix}_HOST is not set")
    tls = False
    if raw := os.environ.get(f"{prefix}_TLS", ""):
        if raw.lower() not in ("true", "false"):
            raise ValueError(f"{name} config: {prefix}_TLS {raw!r} is not a bool")
        tls = raw.lower() == "true"
    port = 443 if tls else 80
    if raw := os.environ.get(f"{prefix}_GRPC_PORT", ""):
        if not raw.isdecimal() or not 0 < int(raw) < 65536:
            raise ValueError(f"{name} config: {prefix}_GRPC_PORT {raw!r} is not a port")
        port = int(raw)
    timeout = DEFAULT_TIMEOUT
    if raw := os.environ.get(f"{prefix}_TIMEOUT", ""):
        match = _TIMEOUT.match(raw)
        parsed = timedelta(**{_UNITS[match["unit"]]: float(match["number"])}) if match else timedelta(0)
        if parsed <= timedelta(0):
            raise ValueError(f"{name} config: {prefix}_TIMEOUT {raw!r} is not a duration like 5s or 500ms")
        timeout = parsed
    return GrpcConfig(name=name, host=host, port=port, tls=tls, timeout=timeout)
