"""What a call of a repository asks for: a specification of the rows of one
table, a projection of them into a result, a key to order them by."""

from abc import ABC, abstractmethod
from dataclasses import dataclass
from typing import Any, Generic, TypeVar

from sqlalchemy import Select
from sqlalchemy.sql.elements import ColumnElement
from sqlalchemy.orm import QueryableAttribute

TRow = TypeVar("TRow")
"""The row of one table: a class SQLAlchemy maps (tables.ItemRow)."""

TResult = TypeVar("TResult")
"""What a projection reads a row as: a dataclass whose fields name the columns."""

TOrdered = TypeVar("TOrdered")
"""What a selector answers, a row or a result, which its keys order."""

type Columns = Select[*tuple[Any, ...]]
"""A select of any columns: what a projection picks."""


class Specification(ABC, Generic[TRow]):
    """A named set of rows of TRow's table: the conditions, and the joins
    along the relations of the rows, that pick adds to the select of the
    table. It holds no order and no page - select_by refuses them - and runs
    no statement, so one specification serves a count, a read, and a delete.
    Its type parameter is its table: mypy stops a specification of another
    table at Repository.select_by."""

    @abstractmethod
    def pick(self, query: Select[TRow]) -> Select[TRow]:
        """Adds the conditions and the joins: query.where(...), query.join(...)."""


class Projection(ABC, Generic[TRow, TResult]):
    """A named set of rows of TRow's table read as TResult: pick adds the
    conditions and the joins, and names the columns, each labelled by a field
    of TResult (query.with_only_columns(OwnerRow.name.label("owner"), ...)).
    Like a specification, it holds no order and no page."""

    @abstractmethod
    def pick(self, query: Select[TRow]) -> Columns:
        """Adds the conditions, the joins, and the columns."""


@dataclass(frozen=True)
class Key(Generic[TOrdered]):
    """A column to order the rows of TOrdered by: a row's attribute
    (ItemPosition: Key[ItemRow] = Key(ItemRow.id)), or a column of a projection."""

    column: QueryableAttribute[Any] | ColumnElement[Any]
