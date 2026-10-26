# Source: skill vp-http-api-in-python

"""The fixed middleware answering every failure of a request as
google.rpc.Status: status_response, the body of every error, and
AnswerFailuresMiddleware, which answers what no exception handler took."""

import logging

from fastapi.responses import JSONResponse
from starlette.types import ASGIApp, Message, Receive, Scope, Send

from todo.api.core import rpcstatus
from todo.api.core.rpcstatus import Code


def status_response(code: Code, message: str, http_status: int | None = None) -> JSONResponse:
    """google.rpc.Status in proto-JSON, the body of every error."""
    body = {"code": int(code), "message": message, "details": []}
    return JSONResponse(body, status_code=http_status or rpcstatus.http_status(code))


class AnswerFailuresMiddleware:
    """ASGI middleware answering every exception the exception handlers of
    the application did not take - a failure of the service, a KindError
    wrapped in another exception, or one an application's middleware raised -
    by its kind, so the request log around it sees the answer the client
    gets. (Starlette answers such an exception outside every middleware,
    where the request log would see none.) The server adds it inside the
    request log and outside the application's middleware.
    """

    def __init__(self, app: ASGIApp, logger: logging.Logger) -> None:
        self._app = app
        self._logger = logger

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        started = False

        async def tracking_send(message: Message) -> None:
            nonlocal started
            started = started or message["type"] == "http.response.start"
            await send(message)

        try:
            await self._app(scope, receive, tracking_send)
        except Exception as error:
            code, message = rpcstatus.of(error)
            if code is Code.INTERNAL:
                self._logger.error("http request failed", exc_info=error)
            if started:
                raise  # the answer is already on its way; nothing more can be sent
            await status_response(code, message)(scope, receive, send)
