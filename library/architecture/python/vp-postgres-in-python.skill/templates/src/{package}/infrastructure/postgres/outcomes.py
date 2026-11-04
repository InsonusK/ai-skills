"""The outcomes of a repository a service tells apart, and the translation of
the engine's failures into them, from the codes of PostgreSQL."""

from typing import NoReturn

from asyncpg.exceptions import ConnectionDoesNotExistError, InterfaceError
from sqlalchemy.exc import DBAPIError


class RepositoryOutcome(Exception):
    """An outcome of the repository; the service turns each into its kind.
    The engine's own failure is its __cause__: in the log, never in a kind's
    text."""


class DuplicateKeyError(RepositoryOutcome):
    """A unique key is taken (23505)."""


class MissingReferenceError(RepositoryOutcome):
    """A foreign key names no row (23503)."""


class RowChangedError(RepositoryOutcome):
    """The row to update or delete is gone, or was changed meanwhile (with
    vp-row-version)."""


class UnavailableError(RepositoryOutcome):
    """No answer, or a failure a retry may cure: a lost connection, a deadlock
    (40P01), a serialization failure (40001), a lock not available (55P03), a
    server shutting down."""


class RefusedError(Exception):
    """A call the contract does not allow: a specification holding an order or
    a page, take or page below 1. It is a defect of the caller, so it is no
    outcome and keeps no kind."""


# The codes a retry of the same statement may cure.
_TRANSIENT = {
    "40001",  # serialization_failure
    "40P01",  # deadlock_detected
    "55P03",  # lock_not_available
    "53300",  # too_many_connections
    "57P01",  # admin_shutdown
    "57P02",  # crash_shutdown
    "57P03",  # cannot_connect_now
}


def outcome_of(error: BaseException) -> type[RepositoryOutcome] | None:
    """The outcome error is, or None for a failure that is none: a broken
    statement, a defect."""
    if isinstance(error, DBAPIError):
        code = getattr(error.orig, "sqlstate", None)
        if code == "23505":
            return DuplicateKeyError
        if code == "23503":
            return MissingReferenceError
        if code is not None and (code in _TRANSIENT or code.startswith("08")):
            return UnavailableError
        if code is None and error.connection_invalidated:
            return UnavailableError
        return None
    # A connection that cannot be made, or one lost: asyncpg raises these as
    # they are, outside SQLAlchemy's wrapping.
    if isinstance(error, (OSError, TimeoutError, ConnectionDoesNotExistError, InterfaceError)):
        return UnavailableError
    return None


def translate(error: Exception, what: str) -> NoReturn:
    """Raises error as the outcome it is, naming what was done ("insert into
    tasks: duplicate key"), with error as the cause; a failure that is no
    outcome is raised as it is."""
    outcome = outcome_of(error)
    if outcome is None:
        raise error
    raise outcome(f"{what}: {_NAMES[outcome]}") from error


_NAMES: dict[type[RepositoryOutcome], str] = {
    DuplicateKeyError: "duplicate key",
    MissingReferenceError: "missing reference",
    RowChangedError: "row changed",
    UnavailableError: "database unavailable",
}
