# Source: skill vp-http-client-in-python

"""The extension point of every client of another service's API, over HTTP or
gRPC alike: the client middleware the composition root lists runs around the
attempts of each call, so a variation point (the retry) adds a step to every
call of every API without editing the client (pipeline.py)."""

from todo.infrastructure.client.core.pipeline.pipeline import Attempt, Call, Middleware, run

__all__ = ["Attempt", "Call", "Middleware", "run"]
