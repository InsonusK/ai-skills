"""The entry decorator that identifies the caller of an HTTP request."""

from fastapi import Request, Response

from todo.api.http.pipeline import CallNext, Middleware
from todo.command.access import Identity, caller
from todo.command.pipeline import Kind, KindError


class UnknownCallerError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.UNAUTHENTICATED, "the bearer token names no known caller")


def authenticate(tokens: dict[str, Identity]) -> Middleware:
    """The middleware that identifies the caller: a request with
    "Authorization: Bearer {token}" of a known token goes on with the token's
    identity in its context; a request without the header goes on with none,
    and the command decides whether it needs one; a header that names no known
    caller is answered unauthenticated at once. tokens maps each token to its
    caller (AccessConfig.tokens)."""

    async def identify(request: Request, call_next: CallNext) -> Response:
        header = request.headers.get("authorization")
        if header is None:
            return await call_next(request)
        scheme, _, token = header.partition(" ")
        identity = tokens.get(token)
        if scheme != "Bearer" or identity is None:
            raise UnknownCallerError()
        with caller(identity):
            return await call_next(request)

    return identify
