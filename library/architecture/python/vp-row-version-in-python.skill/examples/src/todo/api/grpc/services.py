"""The gRPC entry point's registration: the services it serves, one module
each in endpoint/, and the entry factory the composition root lists. The
fixed code is in pipeline/ and middleware/."""

import logging
from collections.abc import Sequence
from dataclasses import dataclass

from todo.api.core.entry import Entry
from todo.api.grpc import pipeline
from todo.api.grpc.endpoint import ItemsHandler
from todo.api.grpc.middleware import Interceptor
from todo.api.grpc.pipeline import Service
from todo.command import Handlers


@dataclass(frozen=True)
class Endpoints:
    """The services of the proto contract this API serves, one field per
    module of endpoint/. A new service adds a field here and a line to
    from_handlers and to services(); a new method of an existing service
    changes only that service's module."""

    items: ItemsHandler

    @classmethod
    def from_handlers(cls, handlers: Handlers) -> "Endpoints":
        """Gives every service the handlers of its commands from the command registry."""
        return cls(
            items=ItemsHandler.from_handlers(handlers),
        )


def services(endpoints: Endpoints) -> list[Service]:
    """The services of the published contract
    (docs/integration/api/todo/todo.proto). The contract and the services are
    kept in step by one scenario."""
    return [endpoints.items.service()]


def entry(handlers: Handlers, logger: logging.Logger, interceptors: Sequence[Interceptor]) -> Entry:
    """The gRPC entry point as the composition root lists it for run_all:
    every service over the handlers of the registry, every call wrapped by
    the interceptors - the first the outermost, all inside the call log -
    served until the process stops (see pipeline.run)."""
    contract = services(Endpoints.from_handlers(handlers))
    return Entry("grpc", lambda stop: pipeline.run(contract, logger, stop, interceptors))
