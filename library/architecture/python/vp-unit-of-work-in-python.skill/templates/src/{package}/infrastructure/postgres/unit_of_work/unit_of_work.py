"""The unit of work as the router of the database's connections: attach adds
it to the database, so no repository changes. A call of a repository in a
command whose context carries a begun unit runs on the unit's connection,
behind a savepoint of its own (Database.session); a write opens the unit's
transaction, a read runs there once it is open, and on the pool before.

The unit lives in a context variable: the outermost command begins it, every
call awaited in that context - nested commands, services, repositories -
joins it, and only the command middleware ends it. The calls of one unit run
one at a time, on one connection."""

import asyncio
from contextvars import ContextVar

from sqlalchemy.ext.asyncio import AsyncConnection, AsyncEngine

from {package}.infrastructure.postgres import Database, UnavailableError

END_TIMEOUT = 30.0
"""Seconds a commit or a rollback may take, which no caller's cancellation
reaches: the database's own timeout, as Npgsql's 30 seconds in dotnet."""


class UnitEndedError(Exception):
    """A write in a unit its command has ended: a task the command left
    behind. It belongs to no transaction, so it is refused."""


class Unit:
    """One command's transaction, opened by its first write."""

    def __init__(self, engine: AsyncEngine) -> None:
        self._engine = engine
        self._lock = asyncio.Lock()
        self._connection: AsyncConnection | None = None
        self.ended = False

    async def connection(self, write: bool) -> AsyncConnection | None:
        """The unit's connection in its transaction, begun by the first write;
        None for a read before it, which runs on the pool."""
        async with self._lock:
            if self._connection is None:
                if not write:
                    return None
                if self.ended:
                    raise UnitEndedError("unit of work: the command has ended its unit; a write after it belongs to no transaction")
                connection = await self._engine.connect()
                try:
                    await connection.begin()
                except BaseException:
                    await connection.close()
                    raise
                self._connection = connection
            return self._connection

    async def commit(self) -> None:
        """Keeps every write of the unit; a unit with no write does nothing.
        The caller's cancellation does not stop it, END_TIMEOUT does. A failure
        is UnavailableError: the connection may have been lost after the commit
        reached the database, so the outcome is unknown."""
        try:
            await asyncio.shield(self._end(commit=True))
        except asyncio.CancelledError:
            raise
        except Exception as error:
            raise UnavailableError("commit: database unavailable") from error

    async def rollback(self) -> None:
        """Drops every write of the unit. It raises nothing: a rollback that
        fails has lost its connection, and the server drops the transaction
        with it; a unit already ended or with no write does nothing."""
        try:
            await asyncio.shield(self._end(commit=False))
        except Exception:
            pass

    async def _end(self, commit: bool) -> None:
        async with self._lock:
            connection, self._connection, self.ended = self._connection, None, True
        if connection is None:
            return
        try:
            async with asyncio.timeout(END_TIMEOUT):
                if commit:
                    await connection.commit()
                else:
                    await connection.rollback()
        finally:
            # A connection lost is discarded by the pool, not given to the next caller.
            await connection.close()


_current: ContextVar[Unit | None] = ContextVar("unit_of_work", default=None)


class UnitOfWork:
    """Begins the units of the commands over one database, and gives each call
    of a repository the connection of the unit its context carries: the
    database's ConnectionRouter."""

    def __init__(self, engine: AsyncEngine) -> None:
        self._engine = engine

    def begin(self) -> Unit | None:
        """A new unit for the outermost command, which then ends it, made the
        unit of the current context; None when the context carries one that
        has not ended, for a nested command, which joins it. A unit opens no
        transaction until its first write. An ended unit stays in its context,
        so a write there after the end is refused, and the next command there
        begins a unit of its own."""
        current = _current.get()
        if current is not None and not current.ended:
            return None
        unit = Unit(self._engine)
        _current.set(unit)
        return unit

    async def connection_for(self, write: bool) -> AsyncConnection | None:
        unit = _current.get()
        if unit is None:
            return None
        return await unit.connection(write)


def attach(database: Database) -> UnitOfWork:
    """Adds the unit of work to every call of a repository over database.
    Called once, in the composition root, right after the database is built
    and before the first statement."""
    units = UnitOfWork(database.engine())
    database.use(units)
    return units
