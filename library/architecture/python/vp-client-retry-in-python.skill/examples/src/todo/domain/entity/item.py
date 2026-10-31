"""The to-do item entity."""

from todo.domain.entity.errors import MAX_TITLE_LENGTH, AlreadyDoneError, EmptyTitleError, TitleTooLongError


class Item:
    """One to-do item. Its state changes only through its methods, which
    guard the item's own rules; there is no setter to go around them.
    """

    __slots__ = ("_done", "_title")

    def __init__(self, title: str) -> None:
        """Creates an open item; the title is trimmed and must be 1..MAX_TITLE_LENGTH characters."""
        title = title.strip()
        if not title:
            raise EmptyTitleError()
        if len(title) > MAX_TITLE_LENGTH:
            raise TitleTooLongError()
        self._title = title
        self._done = False

    @property
    def title(self) -> str:
        return self._title

    @property
    def done(self) -> bool:
        return self._done

    def complete(self) -> None:
        """Marks the item done; an item is completed once."""
        if self._done:
            raise AlreadyDoneError()
        self._done = True
