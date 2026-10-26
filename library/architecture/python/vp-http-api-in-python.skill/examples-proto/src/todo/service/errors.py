"""The errors of the services, each carrying its kind."""

from todo.domain.entity import Kind, KindError


class ItemAlreadyListedError(KindError):
    def __init__(self, list_id: str, title: str) -> None:
        super().__init__(Kind.CONFLICT, f'list "{list_id}" already has an item "{title}"')


class ListNotFoundError(KindError):
    def __init__(self, list_id: str) -> None:
        super().__init__(Kind.NOT_FOUND, f'no list "{list_id}"')
