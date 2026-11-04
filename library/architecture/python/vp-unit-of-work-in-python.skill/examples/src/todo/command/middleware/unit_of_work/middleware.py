# Source: skill vp-unit-of-work-in-python
"""The command middleware that keeps a command's writes together: the
outermost command begins the unit of work and ends it, a commit when its
handler returned and a rollback when it raised; a nested command only joins."""

from typing import Protocol

from todo.command.pipeline import Kind, KindError, Next


class Unit(Protocol):
    """One command's unit of work, as the middleware sees it."""

    async def commit(self) -> None:
        """Keeps every write of the unit; the caller's cancellation does not stop it."""
        ...

    async def rollback(self) -> None:
        """Drops every write of the unit; it raises nothing."""
        ...


class Units(Protocol):
    """The unit of work as the middleware sees it, with no package of the
    database: unit_of_work.UnitOfWork of the PostgreSQL variation point has it."""

    def begin(self) -> Unit | None:
        """A new unit for the outermost command; None when one has begun."""
        ...


class ChangesNotCommittedError(KindError):
    """The answer of a failed commit, the kind unavailable with a fixed
    message: a kind's text reaches the caller, and the cause may name the
    database. The commit may have reached the database, so a command a caller
    retries after it is idempotent."""

    def __init__(self) -> None:
        super().__init__(Kind.UNAVAILABLE, "the changes could not be committed: the database does not answer")


class UnitOfWorkMiddleware:
    """Ends the unit of work of every outermost command. Listed last, so a
    command the middleware before it reject opens nothing and a failed commit
    reaches the log as the command's failure."""

    def __init__(self, units: Units) -> None:
        self._units = units

    async def __call__(self, name: str, command: object, call_next: Next) -> object:
        unit = self._units.begin()
        if unit is None:
            return await call_next(command)
        try:
            result = await call_next(command)
        except BaseException:
            # A cancelled caller too: the rollback runs, shielded, and the
            # cancellation goes on.
            await unit.rollback()
            raise
        try:
            await unit.commit()
        except Exception as error:
            await unit.rollback()
            raise ChangesNotCommittedError() from error
        return result
