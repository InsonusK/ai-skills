"""The errors of the services, each carrying its kind."""

from todo.domain.entity import Kind, KindError


class TodoServiceUnavailableError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.UNAVAILABLE, "the todo service is unavailable")


class TitlesRejectedError(KindError):
    def __init__(self, reason: str) -> None:
        super().__init__(Kind.INVALID, f"the todo service rejected the titles: {reason}")
