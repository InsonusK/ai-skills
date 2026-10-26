"""The errors of the domain's rules, each carrying its kind (kinds.py)."""

from todo.domain.entity.kinds import Kind, KindError

MAX_TITLE_LENGTH = 100
"""The longest item title, in characters."""


class EmptyTitleError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.INVALID, "item title is empty")


class TitleTooLongError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.INVALID, f"item title is longer than {MAX_TITLE_LENGTH} characters")


class AlreadyDoneError(KindError):
    # The input is valid; the item's state forbids it.
    def __init__(self) -> None:
        super().__init__(Kind.FAILED_PRECONDITION, "item is already done")


class NoItemsError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.INVALID, "no items to plan")


class DuplicateTitleError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.INVALID, "item titles repeat")
