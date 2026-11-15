# Source: skill base-plateau-in-python

"""The id of the request being served, carried along the async calls of that
request, so every record of it can be told apart from those of the requests
served at the same time. An entry point begins it when a call arrives; the
logging variation point writes it into every record.
"""

import secrets
from collections.abc import Iterator
from contextlib import contextmanager
from contextvars import ContextVar

_current: ContextVar[str | None] = ContextVar("request_id", default=None)


def request_id() -> str | None:
    """The id request_id_scope set for the running call, or None."""
    return _current.get()


def new_request_id() -> str:
    """A random id of 16 hex characters."""
    return secrets.token_hex(8)


@contextmanager
def request_id_scope(id: str | None) -> Iterator[str]:
    """Sets the id of the call that runs inside the with block - an empty id
    is replaced by a new random one - and restores the previous id after it.
    Each asyncio task has a copy of the context, so the id of one request
    never shows in another's records.
    """
    token = _current.set(id or new_request_id())
    try:
        yield _current.get() or ""
    finally:
        _current.reset(token)
