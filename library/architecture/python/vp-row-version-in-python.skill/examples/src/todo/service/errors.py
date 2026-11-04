"""The errors of the services, each carrying its kind."""

from todo.shared.errors import Kind, KindError


class ItemAlreadyListedError(KindError):
    def __init__(self, list_id: str, title: str) -> None:
        super().__init__(Kind.CONFLICT, f'list "{list_id}" already has an item "{title}"')


class ListNotFoundError(KindError):
    def __init__(self, list_id: str) -> None:
        super().__init__(Kind.NOT_FOUND, f'no list "{list_id}"')


class ItemNotFoundError(KindError):
    def __init__(self, list_id: str, item_id: int) -> None:
        super().__init__(Kind.NOT_FOUND, f'no item {item_id} in list "{list_id}"')


class ItemChangedError(KindError):
    def __init__(self, list_id: str, item_id: int) -> None:
        super().__init__(Kind.CONCURRENT_CHANGE, f'item {item_id} of list "{list_id}" was changed since it was read: read it again')


class ListsUnavailableError(KindError):
    # The message is fixed: the repository's failure may name the database,
    # and a kind's text reaches the caller.
    def __init__(self) -> None:
        super().__init__(Kind.UNAVAILABLE, "the lists are unavailable")
