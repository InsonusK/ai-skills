"""The repository: one contract for every table - Insert, Update, Delete of
one row, saved at once, and SelectBy of a specification - and the projections
of a table into results."""

from collections.abc import Awaitable, Callable
from typing import Any, Generic, TypeVar

from sqlalchemy import Select, Table, delete, insert, select, update
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import InstrumentedAttribute, Mapper, class_mapper

from todo.infrastructure.postgres.database import Database
from todo.infrastructure.postgres.outcomes import RefusedError, RowChangedError, translate
from todo.infrastructure.postgres.selector import ResultRowSelector, ResultSelector, RowSelector, Selector
from todo.infrastructure.postgres.specification import Columns, Projection, Specification
from todo.infrastructure.postgres.version import version_attribute

TRow = TypeVar("TRow")
TResult = TypeVar("TResult")
R = TypeVar("R")


class Statements:
    """The statements of one table, for a method a table's own repository adds
    past the contract: an upsert, a bulk statement, a grouping."""

    def __init__(self, database: Database, table: str) -> None:
        self._database = database
        self._table = table

    async def run(self, what: str, statement: Callable[[AsyncSession], Awaitable[R]], write: bool = True) -> R:
        """Runs statement in the session of one call, as the contract's own
        calls run: the failure translated into the outcomes, and the session's
        transaction joined to a begun unit of work. A statement that only
        reads passes write=False, so it opens no unit's transaction."""
        try:
            async with self._database.session(write) as session:
                return await statement(session)
        except Exception as error:
            translate(error, what)


class Repository(Statements, Generic[TRow]):
    """The repository of the table of TRow, a class SQLAlchemy maps. Every
    write is saved when it returns; select_by answers a selector, which runs
    nothing yet. A table's own repository extends it with methods past the
    contract, written over Statements.run."""

    def __init__(self, database: Database, row_type: type[TRow]) -> None:
        mapper: Mapper[TRow] = class_mapper(row_type)
        if len(mapper.primary_key) != 1:
            raise ValueError(f"postgres: {row_type.__name__} must have a primary key of one column")
        super().__init__(database, _table_of(mapper))
        self._row_type = row_type
        self._mapper = mapper
        self._key: InstrumentedAttribute[Any] = getattr(row_type, mapper.get_property_by_column(mapper.primary_key[0]).key)
        # The attribute of the row's version (version.py), or None for a row
        # of a table that never changes.
        version = version_attribute(mapper)
        self._version: InstrumentedAttribute[Any] | None = getattr(row_type, version) if version else None

    async def insert(self, row: TRow) -> None:
        """Inserts row, its relations left as they are, and sets its key: the
        one the database generates, when row has none; and its version, when
        it declares one."""
        given = getattr(row, self._key.key) is not None
        answered = [self._key] if self._version is None else [self._key, self._version]

        async def run(session: AsyncSession) -> None:
            generated = await session.execute(insert(self._row_type).values(self._values(row, with_key=given)).returning(*answered))
            for attribute, value in zip(answered, generated.one(), strict=True):
                setattr(row, attribute.key, value)

        await self.run(f"insert into {self._table}", run)

    async def update(self, row: TRow) -> None:
        """Saves every column of row by its key, and by its version when the
        row declares one, whose new value it puts on row; no row of that key,
        or one whose version moved, is RowChangedError."""

        async def run(session: AsyncSession) -> bool:
            return await self.save(session, row)

        if not await self.run(f"update {self._table}", run):
            raise RowChangedError(f"update {self._table}: row changed")

    async def save(self, session: AsyncSession, row: TRow) -> bool:
        """The UPDATE of row in session: by its key and its version, the new
        version put on row; answers whether a row was saved. A versioned row
        with no version is a defect of a caller that skipped the read."""
        statement = (
            update(self._row_type)
            .where(self._key == getattr(row, self._key.key))
            .values(self._values(row, with_key=False))
            .execution_options(synchronize_session=False)
        )
        if self._version is None:
            result = await session.execute(statement)
            return int(getattr(result, "rowcount", 0)) > 0
        version = getattr(row, self._version.key)
        if not version:
            raise RefusedError(f"update {self._table}: the row has no version: read it before you write it")
        saved = (await session.execute(statement.where(self._version == version).returning(self._version))).scalar_one_or_none()
        if saved is None:
            return False
        setattr(row, self._version.key, saved)
        return True

    async def delete(self, row: TRow) -> None:
        """Deletes row by its key, and by its version when the row declares
        one; no row of that key, or one whose version moved, is
        RowChangedError."""
        statement = delete(self._row_type).where(self._key == getattr(row, self._key.key))
        if self._version is not None:
            version = getattr(row, self._version.key)
            if not version:
                raise RefusedError(f"delete from {self._table}: the row has no version: read it before you write it")
            statement = statement.where(self._version == version)

        async def run(session: AsyncSession) -> int:
            result = await session.execute(statement.execution_options(synchronize_session=False))
            return int(getattr(result, "rowcount", 0))

        if await self.run(f"delete from {self._table}", run) == 0:
            raise RowChangedError(f"delete from {self._table}: row changed")

    def select_by(self, specification: Specification[TRow]) -> Selector[TRow]:
        """The selector of the rows specification picks. A specification
        holding an order or a page is refused by every call of the selector."""

        def picked() -> Columns:
            return specification.pick(select(self._row_type))

        async def fetch(session: AsyncSession, query: Columns) -> list[Any]:
            return list((await session.scalars(query)).all())

        # A versioned row is saved by save(), so a selector's update compares
        # the version of every row it writes.
        saved_by = self.save if self._version is not None else None
        return RowSelector(self._database, self._table, picked, fetch, refusal(picked()), [self._key], saved_by)

    def _values(self, row: TRow, with_key: bool) -> dict[str, Any]:
        """The value of every column of row, by its attribute; the key only
        when asked for."""
        values = {}
        for attribute in self._mapper.column_attrs:
            if not with_key and attribute.key == self._key.key:
                continue
            # The version is the engine's: compared, never written.
            if self._version is not None and attribute.key == self._version.key:
                continue
            values[attribute.key] = getattr(row, attribute.key)
        return values


class Projections(Generic[TRow, TResult]):
    """The projections of the table of TRow into TResult, a dataclass whose
    fields are the labels of a projection's columns."""

    def __init__(self, database: Database, row_type: type[TRow], result_type: type[TResult]) -> None:
        self._database = database
        self._row_type = row_type
        self._result_type = result_type
        self._table = _table_of(class_mapper(row_type))

    def select_by(self, projection: Projection[TRow, TResult]) -> ResultSelector[TResult]:
        """The selector of the results projection picks; reads only."""

        def picked() -> Columns:
            return projection.pick(select(self._row_type))

        async def fetch(session: AsyncSession, query: Columns) -> list[Any]:
            return [self._result_type(**mapping) for mapping in (await session.execute(query)).mappings()]

        return ResultRowSelector(self._database, self._table, picked, fetch, refusal(picked()))


def _table_of(mapper: Mapper[Any]) -> str:
    """The name of the table a row maps to."""
    table = mapper.local_table
    return table.name if isinstance(table, Table) else str(table)


def refusal(query: Columns) -> RefusedError | None:
    """The refusal of a specification that put an order or a page into its
    select: a count, an update, or a delete would work on other rows than a
    read. Read from the select a specification built, which runs nothing."""
    if query._order_by_clauses:
        return RefusedError("the specification holds an order: order the selector")
    if query._limit_clause is not None or query._offset_clause is not None or query._fetch_clause is not None:
        return RefusedError("the specification holds a page: take or page the ordered selector")
    return None
