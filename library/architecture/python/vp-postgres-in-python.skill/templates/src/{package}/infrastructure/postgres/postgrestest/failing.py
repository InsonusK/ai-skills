"""A repository that answers a chosen failure to every call."""

from collections.abc import Callable
from dataclasses import dataclass
from typing import Generic, TypeVar

from {package}.infrastructure.postgres.specification import Key, Specification

TRow = TypeVar("TRow")


@dataclass(frozen=True)
class Failing(Generic[TRow]):
    """A repository whose every call raises error: a service's scenario gives
    it the outcome to turn into a kind (DuplicateKeyError, UnavailableError)
    or a failure with none. What the rows are is a scenario on the real
    engine."""

    error: Exception

    async def insert(self, row: TRow) -> None:
        raise self.error

    async def update(self, row: TRow) -> None:
        raise self.error

    async def delete(self, row: TRow) -> None:
        raise self.error

    def select_by(self, specification: Specification[TRow]) -> "_FailingSelector[TRow]":
        return _FailingSelector(self.error)


@dataclass(frozen=True)
class _FailingSelector(Generic[TRow]):
    """Raises error to every call; an order gives the same."""

    error: Exception

    async def all(self) -> list[TRow]:
        raise self.error

    async def single(self) -> TRow | None:
        raise self.error

    async def count(self) -> int:
        raise self.error

    async def any(self) -> bool:
        raise self.error

    async def first(self) -> TRow | None:
        raise self.error

    async def take(self, count: int) -> list[TRow]:
        raise self.error

    async def page(self, number: int, size: int) -> list[TRow]:
        raise self.error

    async def update(self, change: Callable[[TRow], None]) -> int:
        raise self.error

    async def delete(self) -> int:
        raise self.error

    def order_by(self, key: Key[TRow]) -> "_FailingSelector[TRow]":
        return self

    def order_by_descending(self, key: Key[TRow]) -> "_FailingSelector[TRow]":
        return self

    def then_by(self, key: Key[TRow]) -> "_FailingSelector[TRow]":
        return self
