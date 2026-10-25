"""The batch limit: the command middleware that decides by a value of the
command."""

from typing import Protocol, runtime_checkable

from todo.command.pipeline import Kind, KindError, Next


@runtime_checkable
class Batch(Protocol):
    """The mark of a command that works on many items at once and tells how
    many. A command carries it by having the method, in its own module:

        def batch_size(self) -> int:
            return len(self.titles)
    """

    def batch_size(self) -> int: ...


class BatchTooLargeError(KindError):
    def __init__(self, name: str, limit: int, size: int) -> None:
        super().__init__(Kind.RESOURCE_EXHAUSTED, f"{name} takes at most {limit} items at once, got {size}")


class BatchLimitMiddleware:
    """The batch limit, the command middleware that decides by a value of the
    command: a command marked Batch whose size is above limit is rejected as
    resource exhausted before its handler runs; a batch within the limit and
    every unmarked command pass unchanged. limit is AccessConfig.max_batch."""

    def __init__(self, limit: int) -> None:
        self._limit = limit

    async def __call__(self, name: str, command: object, call_next: Next) -> object:
        if isinstance(command, Batch) and command.batch_size() > self._limit:
            raise BatchTooLargeError(name, self._limit, command.batch_size())
        return await call_next(command)
