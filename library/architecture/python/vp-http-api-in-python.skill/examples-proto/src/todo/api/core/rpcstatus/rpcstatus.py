# Source: skill error-kinds-in-python

"""Answers an error of a command by its kind, for every entry point at once:
the google.rpc.Code a gRPC call returns, the same code in the
google.rpc.Status body of an HTTP error, and the HTTP status. It is the table
of skill error-kinds in code; a new kind is a row here, never a change of an
entry point.

It imports no gRPC module, so an HTTP-only service does not pull one in.
"""

from enum import IntEnum

from todo.command.pipeline import Kind, KindError, kind_of


class Code(IntEnum):
    """A google.rpc.Code; the numbers are those of grpc.StatusCode."""

    OK = 0
    CANCELLED = 1
    UNKNOWN = 2
    INVALID_ARGUMENT = 3
    DEADLINE_EXCEEDED = 4
    NOT_FOUND = 5
    ALREADY_EXISTS = 6
    PERMISSION_DENIED = 7
    RESOURCE_EXHAUSTED = 8
    FAILED_PRECONDITION = 9
    ABORTED = 10
    OUT_OF_RANGE = 11
    UNIMPLEMENTED = 12
    INTERNAL = 13
    UNAVAILABLE = 14
    DATA_LOSS = 15
    UNAUTHENTICATED = 16


CODE_OF_KIND: dict[Kind, Code] = {
    Kind.INVALID: Code.INVALID_ARGUMENT,
    Kind.NOT_FOUND: Code.NOT_FOUND,
    Kind.CONFLICT: Code.ALREADY_EXISTS,
    Kind.CONCURRENT_CHANGE: Code.ABORTED,
    Kind.FAILED_PRECONDITION: Code.FAILED_PRECONDITION,
    Kind.UNAUTHENTICATED: Code.UNAUTHENTICATED,
    Kind.PERMISSION_DENIED: Code.PERMISSION_DENIED,
    Kind.RESOURCE_EXHAUSTED: Code.RESOURCE_EXHAUSTED,
    Kind.UNIMPLEMENTED: Code.UNIMPLEMENTED,
    Kind.UNAVAILABLE: Code.UNAVAILABLE,
}
"""The error-kinds table: the code each kind is answered with."""

INTERNAL_MESSAGE = "internal error"
"""All a client learns of an error with no kind."""


def of(error: BaseException) -> tuple[Code, str]:
    """The code and the message a client is answered with: the kind's code
    with the message of the error that carries the kind - not of a wrapper
    around it - or INTERNAL with INTERNAL_MESSAGE when the error has no kind:
    its own message may hold connection strings or stack details and goes only
    to the log.
    """
    kind = kind_of(error)
    if kind is None:
        return Code.INTERNAL, INTERNAL_MESSAGE
    current: BaseException | None = error
    while not isinstance(current, KindError):
        assert current is not None  # kind_of found a KindError along __cause__
        current = current.__cause__
    return CODE_OF_KIND[kind], current.message


_HTTP_STATUS = {
    Code.OK: 200,
    Code.INVALID_ARGUMENT: 400,
    Code.FAILED_PRECONDITION: 400,
    Code.OUT_OF_RANGE: 400,
    Code.UNAUTHENTICATED: 401,
    Code.PERMISSION_DENIED: 403,
    Code.NOT_FOUND: 404,
    Code.ALREADY_EXISTS: 409,
    Code.ABORTED: 409,
    Code.RESOURCE_EXHAUSTED: 429,
    Code.CANCELLED: 499,  # Client Closed Request
    Code.UNIMPLEMENTED: 501,
    Code.UNAVAILABLE: 503,
    Code.DEADLINE_EXCEEDED: 504,
}


def http_status(code: Code) -> int:
    """The HTTP status of code by the canonical gRPC-to-HTTP mapping (skill
    error-kinds); INTERNAL, UNKNOWN, and DATA_LOSS are 500."""
    return _HTTP_STATUS.get(code, 500)
