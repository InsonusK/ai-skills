"""The API middleware that identifies the caller of an HTTP request."""

from fastapi import Request, Response

from todo.api.http.pipeline import CallNext
from todo.command.middleware.access import Identity, caller
from todo.command.pipeline import Kind, KindError


class UnknownCallerError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.UNAUTHENTICATED, "the bearer token names no known caller")


class AuthenticationMiddleware:
    """Identifies the caller: a request with
    "Authorization: Bearer {token}" of a known token goes on with the token's
    identity in its context; a request without the header goes on with none,
    and the command decides whether it needs one; a header that names no known
    caller is answered unauthenticated at once. tokens maps each token to its
    caller (AccessConfig.tokens). An HTTP Middleware of the contract's routes."""

    def __init__(self, tokens: dict[str, Identity]) -> None:
        self._tokens = tokens

    async def __call__(self, request: Request, call_next: CallNext) -> Response:
        header = request.headers.get("authorization")
        if header is None:
            return await call_next(request)
        scheme, _, token = header.partition(" ")
        identity = self._tokens.get(token)
        if scheme != "Bearer" or identity is None:
            raise UnknownCallerError()
        with caller(identity):
            return await call_next(request)
