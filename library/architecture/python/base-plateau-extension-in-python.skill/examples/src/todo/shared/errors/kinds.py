# Source: skill error-kinds-in-python

"""The error kinds of the application (skill error-kinds).

Every error a command raises on purpose is a KindError carrying exactly one
Kind, so an entry point answers it by its kind (a status code, an exit code)
without knowing the individual error. An exception that is no KindError, and
has none in its chain of causes, is a failure of the service.

They live in todo.shared, which every layer may import, because every layer
may raise one: an entity breaks a rule, a service does not find a record, a
middleware rejects a call. Entry points use them through todo.command.pipeline.
"""

from enum import StrEnum


class Kind(StrEnum):
    """What the caller can do about an error; the names of skill error-kinds' table."""

    INVALID = "invalid"
    """The input breaks a rule whatever the state (an empty title)."""
    NOT_FOUND = "not found"
    """The object the input names does not exist."""
    CONFLICT = "conflict"
    """The object to create already exists (the name is taken)."""
    CONCURRENT_CHANGE = "concurrent change"
    """The object was changed meanwhile; the whole operation may be retried."""
    FAILED_PRECONDITION = "failed precondition"
    """The input is valid, but the state does not allow the operation (the item is already done)."""
    UNAUTHENTICATED = "unauthenticated"
    """The caller is not identified."""
    PERMISSION_DENIED = "permission denied"
    """The caller is identified but may not do this."""
    RESOURCE_EXHAUSTED = "resource exhausted"
    """A quota or rate limit of the caller is used up."""
    UNIMPLEMENTED = "unimplemented"
    """The operation is not offered (deprecated, or not yet)."""
    UNAVAILABLE = "unavailable"
    """A dependency is down for now; a retry may succeed."""


class KindError(Exception):
    """An error a caller can act on: its kind says how every entry point
    answers it, its message what happened, for the caller, and its cause -
    the exception it was raised from (raise TodoServiceUnavailableError() from
    error) - the technical failure behind it, for the log only.

    The application raises a subclass per error, declared once in its
    errors module (EmptyTitleError), so the kind and the message of one
    error are written in one place and a test can tell the errors apart.
    """

    def __init__(self, kind: Kind, message: str) -> None:
        super().__init__(message)
        self.kind = kind
        self.message = message


def error_of(error: BaseException | None) -> KindError | None:
    """The first KindError in error or in its chain of causes (raise ... from
    error), so an error wrapped on its way up keeps its kind and its message.
    None when there is none - a failure of the service.
    """
    current = error
    while current is not None:
        if isinstance(current, KindError):
            return current
        current = current.__cause__
    return None


def kind_of(error: BaseException | None) -> Kind | None:
    """The kind of error, of its first KindError (error_of). None when it
    carries none - a failure of the service, which a client is answered
    without its message.
    """
    kinded = error_of(error)
    return kinded.kind if kinded is not None else None


def cause_of(error: BaseException | None) -> str | None:
    """The cause of the first KindError in error, as text for the log: the
    messages of its chain of causes joined by ": " (an exception with no
    message by its type's name). None when error has no kind or its
    KindError was raised from nothing.
    """
    kinded = error_of(error)
    if kinded is None or kinded.__cause__ is None:
        return None
    texts = []
    current: BaseException | None = kinded.__cause__
    while current is not None:
        texts.append(str(current) or type(current).__name__)
        current = current.__cause__
    return ": ".join(texts)
