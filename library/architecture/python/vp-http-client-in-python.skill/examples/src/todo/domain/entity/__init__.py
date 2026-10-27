"""The entities of the domain: each guards its own state through its
methods. It performs no I/O and imports no other package of the application,
so the rules here are tested by their scenarios alone.
"""

from todo.domain.entity.errors import (
    MAX_TITLE_LENGTH,
    AlreadyDoneError,
    DuplicateTitleError,
    EmptyTitleError,
    NoItemsError,
    TitleTooLongError,
)
from todo.domain.entity.item import Item
from todo.domain.entity.kinds import Kind, KindError, kind_of

__all__ = [
    "MAX_TITLE_LENGTH",
    "AlreadyDoneError",
    "DuplicateTitleError",
    "EmptyTitleError",
    "Item",
    "Kind",
    "KindError",
    "NoItemsError",
    "TitleTooLongError",
    "kind_of",
]
