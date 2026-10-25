"""The access check: a decorator that filters by the command's mark."""

from typing import Protocol, runtime_checkable

from todo.command.access.identity import identity_of
from todo.command.pipeline import Decorator, Kind, KindError, Next


@runtime_checkable
class Restricted(Protocol):
    """The mark of a command only a caller with a role may run. A command
    carries it by having the method, in its own module:

        def required_role(self) -> str:
            return "planner"
    """

    def required_role(self) -> str:
        """The role the caller needs."""
        ...


class NoCallerError(KindError):
    def __init__(self, name: str) -> None:
        super().__init__(Kind.UNAUTHENTICATED, f"{name} needs a caller")


class MissingRoleError(KindError):
    def __init__(self, name: str, role: str) -> None:
        super().__init__(Kind.PERMISSION_DENIED, f"{name} needs the role {role}")


def with_access_check() -> Decorator:
    """The decorator that filters by the command's mark: a command marked
    Restricted runs only for a caller with its role - no identity is
    unauthenticated, an identity without the role is permission denied;
    every other command passes unchanged. List it after with_logging, so a
    rejected call is logged as failed."""

    async def check_access(name: str, command: object, call_next: Next) -> object:
        if not isinstance(command, Restricted):
            return await call_next(command)
        identity = identity_of()
        if identity is None:
            raise NoCallerError(name)
        if not identity.has_role(command.required_role()):
            raise MissingRoleError(name, command.required_role())
        return await call_next(command)

    return check_access
