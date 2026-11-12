"""The connection pool of the application's database, and the session each
call of a repository runs in."""

from collections.abc import AsyncIterator
from contextlib import asynccontextmanager
from typing import Protocol

from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncConnection, AsyncEngine, AsyncSession, create_async_engine

from {package}.infrastructure.postgres.config import Config


class ConnectionRouter(Protocol):
    """Who gives a call of a repository its connection, when it is not the
    pool: the unit of work variation point, which holds a transaction for the
    calls of one command. Added by Database.use; this package knows no more of
    it."""

    async def connection_for(self, write: bool) -> AsyncConnection | None:
        """The connection, in a transaction, the call runs on behind a
        savepoint; None for a transaction of the call's own on the pool.
        write tells a call that writes from one that only reads."""
        ...


class Database:
    """The connection pool of one application's database. Opening it runs no
    statement, so the service starts while the database is down and its
    readiness check holds traffic back until it answers."""

    def __init__(self, config: Config, pool_size: int = 5, max_overflow: int = 10) -> None:
        """Prepares the pool of config's database: pool_size connections kept,
        max_overflow more opened under load (SQLAlchemy's defaults)."""
        self._engine = create_async_engine(config.url(), pool_size=pool_size, max_overflow=max_overflow)
        self._router: ConnectionRouter | None = None

    def use(self, router: ConnectionRouter) -> None:
        """Gives every call of a repository the connection router answers: how
        another variation point (the unit of work) reaches the statements of
        every repository with no edit of this package. Called in the
        composition root, before the first statement."""
        self._router = router

    @asynccontextmanager
    async def session(self, write: bool) -> AsyncIterator[AsyncSession]:
        """The session of one call of a repository, in a transaction committed
        when the block ends and rolled back when it raises: a write is saved
        when the call returns, and a failed one leaves nothing pending. On the
        connection of a router, the transaction is a savepoint of the
        router's, so a failure undoes only the call's own writes."""
        connection = await self._router.connection_for(write) if self._router is not None else None
        if connection is None:
            async with AsyncSession(self._engine, expire_on_commit=False) as session, session.begin():
                yield session
            return
        async with (
            AsyncSession(bind=connection, expire_on_commit=False, join_transaction_mode="create_savepoint") as session,
            session.begin(),
        ):
            yield session

    async def ping(self) -> None:
        """The readiness check: the database answers a query. It has the shape
        of a check of the health variation point, an async function that
        raises, and imports nothing of it."""
        async with self._engine.connect() as connection:
            await connection.execute(text("SELECT 1"))

    def engine(self) -> AsyncEngine:
        """The pool itself, for its limits and for a tool that needs
        SQLAlchemy's engine (the migrations); a statement goes through a
        repository."""
        return self._engine

    async def close(self) -> None:
        """Closes every connection of the pool."""
        await self._engine.dispose()
