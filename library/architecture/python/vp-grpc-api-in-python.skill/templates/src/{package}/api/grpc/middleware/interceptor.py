# Source: skill vp-grpc-api-in-python

"""The shape of every gRPC middleware - the fixed ones and an application's.

grpc.aio's own ServerInterceptor sees only the method's name and metadata
before the call, never the request, the response, or how the call ended; so
the server runs every unary call through a chain of plain async functions of
this shape instead, the gRPC counterpart of the HTTP API's middleware.
"""

from collections.abc import Awaitable, Callable
from dataclasses import dataclass
from typing import Any

import grpc
from google.protobuf.message import Message


@dataclass(frozen=True)
class IncomingCall:
    """One unary call as a middleware sees it."""

    method: str
    """The full name of the method, e.g. "/todo.v1.ItemsService/PlanItems"."""

    request: Message
    """The request message, decoded."""

    context: grpc.aio.ServicerContext[Any, Any]
    """The call's context: its metadata (invocation_metadata()), the status it
    was aborted with (code(), details())."""


type CallNext = Callable[[IncomingCall], Awaitable[Message]]
"""The rest of the chain: the next middleware, finally the method itself."""

type Interceptor = Callable[[IncomingCall, CallNext], Awaitable[Message]]
"""A middleware of the calls of the contract's methods - the gRPC form of an
API middleware (skill base-plateau-extension): an async function, or an
object with an async __call__, of the call and call_next, that does its step
around call_next and returns its response, e.g. reads the caller's
credentials from the metadata into a ContextVar. It rejects a call by raising
a KindError, answered by its kind like a command's error; it keeps no state
of a call in itself. Each lives in a package of its own under
api/grpc/middleware/."""
