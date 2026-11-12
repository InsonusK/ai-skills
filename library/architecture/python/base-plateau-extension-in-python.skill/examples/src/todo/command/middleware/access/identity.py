"""The caller of a command, carried in the call's context."""

from collections.abc import Iterator
from contextlib import contextmanager
from contextvars import ContextVar
from dataclasses import dataclass


@dataclass(frozen=True)
class Identity:
    """The caller of a command as plain values, the way the API layer passes
    it to the command layer (skill app-layers, "Identity resolved in the
    command layer")."""

    user: str
    roles: tuple[str, ...] = ()

    def has_role(self, role: str) -> bool:
        return role in self.roles


_identity: ContextVar[Identity | None] = ContextVar("identity", default=None)


@contextmanager
def caller(identity: Identity) -> Iterator[None]:
    """Runs the block with the caller's identity in the context; an API
    middleware wraps the rest of the call in it once it has identified the
    caller."""
    token = _identity.set(identity)
    try:
        yield
    finally:
        _identity.reset(token)


def identity_of() -> Identity | None:
    """The caller's identity, or None when the call came without credentials."""
    return _identity.get()
