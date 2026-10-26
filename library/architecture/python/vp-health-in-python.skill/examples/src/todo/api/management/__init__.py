# Source: skill vp-health-in-python

"""The service's management entry point: a small HTTP server on a port of
its own that answers the orchestrator's probes - /health/live,
/health/ready, /health - whatever the service's other entry points are
(HTTP, gRPC, a listener, none). Kept off the API port, it is never published
through the ingress. It calls no command, and it is written on asyncio alone,
so attaching it adds no dependency. Settings in config.py, the server in
server.py; copied from templates and never edited."""

from todo.api.management.config import ManagementConfig, load_config
from todo.api.management.server import Check, Server, entry, run

__all__ = ["Check", "ManagementConfig", "Server", "entry", "load_config", "run"]
