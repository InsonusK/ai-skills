# Source: skill vp-grpc-api-in-python

"""The gRPC entry point's own settings. They are read here, not in
{package}.config, so they come and go together with this package."""

import os
from dataclasses import dataclass


@dataclass(frozen=True)
class GrpcConfig:
    listen_port: int = 9090
    """GRPC_LISTEN_PORT, the port the server listens on; default 9090. 0 picks
    a free port, which the scenarios use."""

    reflection_enabled: bool = False
    """GRPC_REFLECTION_ENABLED: serve gRPC server reflection, so grpcurl or
    Postman can list and call the methods without the proto file - the gRPC
    counterpart of Swagger UI. Off by default."""


_BOOLS = {"true": True, "1": True, "false": False, "0": False}


def load_config() -> GrpcConfig:
    """Reads the settings; an unusable value raises ValueError naming its variable."""
    port = 9090
    if raw := os.environ.get("GRPC_LISTEN_PORT", ""):
        if not raw.isdigit() or int(raw) > 65535:
            raise ValueError(f"grpc config: GRPC_LISTEN_PORT {raw!r} is not a port")
        port = int(raw)
    reflection = False
    if raw := os.environ.get("GRPC_REFLECTION_ENABLED", ""):
        if raw.lower() not in _BOOLS:
            raise ValueError(f"grpc config: GRPC_REFLECTION_ENABLED {raw!r} is not a bool")
        reflection = _BOOLS[raw.lower()]
    return GrpcConfig(listen_port=port, reflection_enabled=reflection)
