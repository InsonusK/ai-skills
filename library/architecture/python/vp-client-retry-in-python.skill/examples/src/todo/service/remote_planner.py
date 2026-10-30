"""The RemotePlanner service."""

from typing import Protocol

from todo.domain.entity import Item, KindError
from todo.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from todo.infrastructure.client.todo_api import Item as AnsweredItem
from todo.service.errors import TitlesRejectedError, TodoServiceUnavailableError


class TodoApi(Protocol):
    """What RemotePlanner needs of its infrastructure; todo_api.TodoApi has it."""

    async def plan_items(self, titles: list[str]) -> list[AnsweredItem]: ...


class RemotePlanner:
    """Plans items with the todo service. It turns the API's technical outcomes
    into the application's error kinds: an API that does not answer is
    unavailable; titles the API rejects are invalid, with its message; any
    other answer is a failure of this service."""

    def __init__(self, api: TodoApi) -> None:
        self._api = api

    async def plan(self, titles: list[str]) -> list[Item]:
        """One open item per title, planned by the todo service."""
        try:
            answered = await self._api.plan_items(titles)
        except ApiUnavailableError as error:
            raise TodoServiceUnavailableError() from error
        except ApiStatusError as error:
            if error.code is StatusCode.INVALID_ARGUMENT:
                raise TitlesRejectedError(error.message) from error
            raise
        items: list[Item] = []
        for answer in answered:
            try:
                item = Item(answer.title)
                if answer.done:
                    item.complete()
            except KindError as error:
                # from None, not from error: its kind would reach the caller,
                # and an item the other service got wrong is a failure of
                # this one, never the caller's invalid input.
                raise RuntimeError(f"the todo service answered the item {answer.title!r}: {error}") from None
            items.append(item)
        return items
