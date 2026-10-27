# Source: skill vp-http-client-in-python

"""The technical outcomes every client of another service's API reports, over
HTTP or gRPC alike, so the service over an adapter catches the same errors
whichever transport the adapter uses (skill api-client)."""

from enum import IntEnum


class StatusCode(IntEnum):
    """The code of a status an API answered: google.rpc.Code, which is also
    the gRPC status code."""

    OK = 0
    CANCELLED = 1
    UNKNOWN = 2
    """Also an HTTP error answer whose body is no google.rpc.Status."""
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


class ApiUnavailableError(Exception):
    """A call the API did not answer: no connection, no answer within the
    timeout, an HTTP 502, 503, or 504, a gRPC UNAVAILABLE or
    DEADLINE_EXCEEDED. The service over the adapter turns it into the kind
    unavailable; a retry may help."""

    def __init__(self, api: str, message: str) -> None:
        super().__init__(f"{api} unavailable: {message}")
        self.api = api


class ApiStatusError(Exception):
    """Any other failed call: the status the API answered. The service over
    the adapter turns the codes it expects into errors of its own; any other
    is a failure of the service."""

    def __init__(self, api: str, code: StatusCode, message: str, http_status: int | None = None) -> None:
        answered = f"{http_status}, " if http_status is not None else ""
        super().__init__(f"{api} answered {answered}{code.name}: {message}")
        self.api = api
        self.code = code
        """The code of the status; UNKNOWN for an HTTP answer with no google.rpc.Status."""
        self.message = message
        """The message of the status, as the API wrote it."""
        self.http_status = http_status
        """The HTTP status of the answer; None over gRPC."""
