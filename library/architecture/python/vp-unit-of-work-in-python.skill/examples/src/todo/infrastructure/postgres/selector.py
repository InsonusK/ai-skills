"""The selector: what a repository answers for a specification - the reads,
the writes, the order, and the page of the rows it picks. Every call is a new
question to the database: a selector is no snapshot."""

from collections.abc import Awaitable, Callable, Sequence
from typing import Any, Generic, Protocol, Self, TypeVar

from sqlalchemy import Select, delete, func, select
from sqlalchemy.exc import MultipleResultsFound
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import InstrumentedAttribute

from todo.infrastructure.postgres.database import Database
from todo.infrastructure.postgres.outcomes import RefusedError, RowChangedError, translate
from todo.infrastructure.postgres.specification import Columns, Key

T = TypeVar("T")
R = TypeVar("R")
TRow = TypeVar("TRow")
TResult = TypeVar("TResult")


class Reads(Protocol[T]):
    """The reads of every selector."""

    async def all(self) -> list[T]:
        """Every picked row; an empty list when none is picked."""
        ...

    async def single(self) -> T | None:
        """The one picked row, or None; more than one is a failure with no
        outcome (sqlalchemy.exc.MultipleResultsFound)."""
        ...

    async def count(self) -> int:
        """How many rows are picked; the order is ignored."""
        ...

    async def any(self) -> bool:
        """Whether a row is picked."""
        ...


class OrderedReads(Protocol[T]):
    """The reads only an order makes sense of."""

    async def first(self) -> T | None:
        """The first row of the order, or None."""
        ...

    async def take(self, count: int) -> list[T]:
        """Up to count rows of the order, every row when fewer exist; count
        below 1 raises RefusedError."""
        ...

    async def page(self, number: int, size: int) -> list[T]:
        """The rows of page number (from 1) of size; an empty list past the
        end; number or size below 1 raises RefusedError."""
        ...


class Selector(Reads[TRow], Protocol[TRow]):
    """The selector of a specification: the reads, the writes of the picked
    rows, and the order. It stays in the service that made it."""

    async def update(self, change: Callable[[TRow], None]) -> int:
        """Applies change to every picked row in one save, all or nothing;
        answers how many rows it changed."""
        ...

    async def delete(self) -> int:
        """Deletes every picked row in one statement; answers how many."""
        ...

    def order_by(self, key: Key[TRow]) -> "OrderedSelector[TRow]":
        """A new ordered selector; this one is unchanged."""
        ...

    def order_by_descending(self, key: Key[TRow]) -> "OrderedSelector[TRow]":
        """A new ordered selector, the key descending."""
        ...


class OrderedSelector(Selector[TRow], OrderedReads[TRow], Protocol[TRow]):
    """A selector with an order: First, Take, and Page."""

    def then_by(self, key: Key[TRow]) -> "OrderedSelector[TRow]":
        """A new ordered selector with one more key; order_by begins a new order."""
        ...


class ResultSelector(Reads[TResult], Protocol[TResult]):
    """The selector of a projection: reads only."""

    def order_by(self, key: Key[TResult]) -> "OrderedResultSelector[TResult]": ...

    def order_by_descending(self, key: Key[TResult]) -> "OrderedResultSelector[TResult]": ...


class OrderedResultSelector(ResultSelector[TResult], OrderedReads[TResult], Protocol[TResult]):
    """A selector of a projection with an order."""

    def then_by(self, key: Key[TResult]) -> "OrderedResultSelector[TResult]": ...


Picked = Callable[[], Columns]
"""Builds the select of the specification, with no order: a new one per call."""

Fetch = Callable[[AsyncSession, Columns], Awaitable[list[Any]]]
"""Runs a select and answers its rows as row objects or results."""


class _Selection(Generic[T]):
    """What every selector holds, and the reads, which a row and a result
    selector share. refused is the failure of a specification the contract
    refuses, raised by every call, so select_by itself runs nothing."""

    def __init__(
        self,
        database: Database,
        what: str,
        picked: Picked,
        fetch: Fetch,
        refused: RefusedError | None,
        order: tuple[tuple[Any, bool], ...] = (),
    ) -> None:
        self._database = database
        self._what = what
        self._picked = picked
        self._fetch = fetch
        self._refused = refused
        self._order = order

    def _by(self, key: Key[Any], descending: bool, restart: bool) -> tuple[tuple[Any, bool], ...]:
        """The order with key added, or begun again with key."""
        return ((key.column, descending),) if restart else (*self._order, (key.column, descending))

    def _ordered(self) -> Columns:
        return self._picked().order_by(*(column.desc() if descending else column.asc() for column, descending in self._order))

    async def _read(self, run: Callable[[AsyncSession], Awaitable[R]], write: bool = False) -> R:
        """Runs run in the session of one call, its failure translated."""
        if self._refused is not None:
            raise self._refused
        try:
            async with self._database.session(write) as session:
                return await run(session)
        except RefusedError:
            raise
        except Exception as error:
            translate(error, f"select {self._what}")

    async def all(self) -> list[T]:
        return await self._read(lambda session: self._fetch(session, self._ordered()))

    async def single(self) -> T | None:
        async def run(session: AsyncSession) -> T | None:
            # Two rows tell "more than one" from "one", with no order needed.
            rows = await self._fetch(session, self._picked().limit(2))
            if len(rows) > 1:
                raise MultipleResultsFound(f"select {self._what}: more than one row is picked")
            return rows[0] if rows else None

        return await self._read(run)

    async def count(self) -> int:
        async def run(session: AsyncSession) -> int:
            return (await session.execute(select(func.count()).select_from(self._picked().subquery()))).scalar_one()

        return await self._read(run)

    async def any(self) -> bool:
        async def run(session: AsyncSession) -> bool:
            return bool((await session.execute(select(self._picked().exists()))).scalar_one())

        return await self._read(run)

    async def first(self) -> T | None:
        rows = await self._read(lambda session: self._fetch(session, self._ordered().limit(1)))
        return rows[0] if rows else None

    async def take(self, count: int) -> list[T]:
        if count < 1:
            raise RefusedError(f"take {count}: the count starts at 1")
        return await self._read(lambda session: self._fetch(session, self._ordered().limit(count)))

    async def page(self, number: int, size: int) -> list[T]:
        if number < 1 or size < 1:
            raise RefusedError(f"page {number} of size {size}: both start at 1")
        return await self._read(lambda session: self._fetch(session, self._ordered().limit(size).offset((number - 1) * size)))


class RowSelector(_Selection[TRow]):
    """The selector of a specification, Selector and OrderedSelector at once;
    a Selector, never an OrderedSelector, until it is ordered."""

    def __init__(
        self,
        database: Database,
        what: str,
        picked: Picked,
        fetch: Fetch,
        refused: RefusedError | None,
        key: Sequence[InstrumentedAttribute[Any]],
        saved_by: Callable[[AsyncSession, Any], Awaitable[bool]] | None = None,
        order: tuple[tuple[Any, bool], ...] = (),
    ) -> None:
        super().__init__(database, what, picked, fetch, refused, order)
        self._key = key
        # The repository's save of one versioned row, or None for a table
        # that never changes, whose rows one flush saves.
        self._saved_by = saved_by

    def _with(self, order: tuple[tuple[Any, bool], ...]) -> Self:
        return type(self)(self._database, self._what, self._picked, self._fetch, self._refused, self._key, self._saved_by, order)

    def order_by(self, key: Key[TRow]) -> "RowSelector[TRow]":
        return self._with(self._by(key, descending=False, restart=True))

    def order_by_descending(self, key: Key[TRow]) -> "RowSelector[TRow]":
        return self._with(self._by(key, descending=True, restart=True))

    def then_by(self, key: Key[TRow]) -> "RowSelector[TRow]":
        return self._with(self._by(key, descending=False, restart=False))

    async def update(self, change: Callable[[TRow], None]) -> int:
        async def run(session: AsyncSession) -> int:
            rows: list[TRow] = await self._fetch(session, self._picked())
            for row in rows:
                change(row)
            modified = [row for row in rows if session.is_modified(row)]
            if self._saved_by is None:
                # One flush in the session's transaction: every row or none.
                await session.flush()
                return len(modified)
            # Each row saved by its version, in the session's transaction: a
            # row changed since the read above fails the whole call.
            for row in modified:
                session.expunge(row)
                if not await self._saved_by(session, row):
                    raise RowChangedError(f"update {self._what}: a picked row was changed meanwhile")
            return len(modified)

        return await self._read(run, write=True)

    async def delete(self) -> int:
        async def run(session: AsyncSession) -> int:
            # One statement: the rows whose key the specification picks, so a
            # specification with a join deletes the same rows it reads.
            (column,) = self._key
            picked = self._picked().with_only_columns(column)
            result = await session.execute(
                delete(column.class_).where(column.in_(picked)).execution_options(synchronize_session=False)
            )
            return int(getattr(result, "rowcount", 0))

        return await self._read(run, write=True)


class ResultRowSelector(_Selection[TResult]):
    """The selector of a projection, ResultSelector and OrderedResultSelector at once."""

    def _with(self, order: tuple[tuple[Any, bool], ...]) -> Self:
        return type(self)(self._database, self._what, self._picked, self._fetch, self._refused, order)

    def order_by(self, key: Key[TResult]) -> "ResultRowSelector[TResult]":
        return self._with(self._by(key, descending=False, restart=True))

    def order_by_descending(self, key: Key[TResult]) -> "ResultRowSelector[TResult]":
        return self._with(self._by(key, descending=True, restart=True))

    def then_by(self, key: Key[TResult]) -> "ResultRowSelector[TResult]":
        return self._with(self._by(key, descending=False, restart=False))
