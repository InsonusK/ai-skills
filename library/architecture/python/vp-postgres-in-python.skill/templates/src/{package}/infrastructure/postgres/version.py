"""The version of a row whose table changes (skill vp-row-version):
PostgreSQL's xmin, which the engine moves on every write, by whatever code.
A row declares it as

    version: Mapped[str | None] = version_column()

read with the row, never written by a statement, never made by a migration
(xmin is a system column). The repository compares it in update and delete
and puts the new one on the row it saved; a row that declares none is
written by its key alone. It is opaque: compared for equality and carried as
text, nothing computes with it."""

from typing import Any

from sqlalchemy import Column, FetchedValue
from sqlalchemy.engine import Dialect
from sqlalchemy.orm import MappedColumn, Mapper, mapped_column
from sqlalchemy.types import TypeDecorator, UserDefinedType

VERSION_COLUMN = "xmin"
"""The system column a version is read from."""


class _Xid(UserDefinedType[int]):
    """PostgreSQL's xid, so a statement compares xmin with an xid, which
    asyncpg sends from an int."""

    cache_ok = True

    def get_col_spec(self, **kw: Any) -> str:
        return "xid"


class VersionType(TypeDecorator[str]):
    """xmin read as text and written back as the xid it is."""

    impl = _Xid
    cache_ok = True

    def process_bind_param(self, value: Any, dialect: Dialect) -> Any:
        return None if value is None else int(value)

    def process_result_value(self, value: Any, dialect: Dialect) -> Any:
        return None if value is None else str(value)


def version_column() -> MappedColumn[Any]:
    """The column of a row's version: xmin, a system column the engine fills
    (server_default, server_onupdate), which no CREATE TABLE names and the
    check of the migrations leaves out."""
    return mapped_column(
        VERSION_COLUMN, VersionType(), system=True, server_default=FetchedValue(), server_onupdate=FetchedValue()
    )


def version_attribute(mapper: Mapper[Any]) -> str | None:
    """The attribute of the row's version, or None for a row that declares none."""
    for attribute in mapper.column_attrs:
        column = attribute.columns[0]
        if isinstance(column, Column) and column.name == VERSION_COLUMN and isinstance(column.type, VersionType):
            return attribute.key
    return None
