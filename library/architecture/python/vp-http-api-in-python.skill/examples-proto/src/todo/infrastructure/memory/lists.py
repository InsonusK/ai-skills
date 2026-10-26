"""The item lists in the process memory - the infrastructure of this
example, so it runs with no database. The DB variation point replaces it with
a store over a real database; the service above it keeps its methods, and no
command changes."""

import asyncio
from dataclasses import dataclass


class DuplicateKeyError(Exception):
    """A unique key is taken - the technical outcome a database driver reports."""


class NoRowsError(Exception):
    """No row was found - the technical outcome a database driver reports."""


@dataclass(frozen=True)
class Row:
    """One stored item."""

    title: str
    done: bool


class Lists:
    """Rows by list id, each title unique in its list. Its methods are async,
    as those of a store over a database are."""

    def __init__(self) -> None:
        self._rows: dict[str, list[Row]] = {}
        self._lock = asyncio.Lock()

    async def insert(self, list_id: str, row: Row) -> None:
        """Appends row to the list, creating the list on its first row; a
        title already in the list raises DuplicateKeyError."""
        async with self._lock:
            rows = self._rows.setdefault(list_id, [])
            if any(existing.title == row.title for existing in rows):
                raise DuplicateKeyError(f"list {list_id!r} title {row.title!r}")
            rows.append(row)

    async def select(self, list_id: str) -> list[Row]:
        """The rows of the list in insertion order; a list with no row raises
        NoRowsError."""
        async with self._lock:
            if list_id not in self._rows:
                raise NoRowsError(f"list {list_id!r}")
            return list(self._rows[list_id])
