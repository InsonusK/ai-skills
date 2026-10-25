# Source: skill vp-http-api-in-python

"""Every error of the HTTP entry point answered with a google.rpc.Status
body: a command's error by its kind (todo.api.core.rpcstatus), a request that
does not fit its route's types as INVALID_ARGUMENT without calling the
command, and the answers the framework gives itself - an unknown path, a
method a path does not serve - with the code of their status.
"""

import logging
from typing import Any

from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from starlette.exceptions import HTTPException
from starlette.types import ASGIApp, Message, Receive, Scope, Send

from todo.api.core import rpcstatus
from todo.api.core.rpcstatus import Code
from todo.command.pipeline import KindError

# The code of each status the framework answers by itself.
_CODE_OF_STATUS = {
    404: Code.NOT_FOUND,
    405: Code.UNIMPLEMENTED,
    413: Code.RESOURCE_EXHAUSTED,
}


def status_response(code: Code, message: str, http_status: int | None = None) -> JSONResponse:
    """google.rpc.Status in proto-JSON, the body of every error."""
    body = {"code": int(code), "message": message, "details": []}
    return JSONResponse(body, status_code=http_status or rpcstatus.http_status(code))


def add_error_answers(app: FastAPI) -> None:
    """Registers the handlers that answer the errors the framework knows of;
    AnswerFailures answers the rest."""

    @app.exception_handler(KindError)
    async def answer_kind(request: Request, error: KindError) -> JSONResponse:
        return status_response(*rpcstatus.of(error))

    @app.exception_handler(RequestValidationError)
    async def answer_invalid_request(request: Request, error: RequestValidationError) -> JSONResponse:
        return status_response(Code.INVALID_ARGUMENT, _invalid_request_message(request, error.errors()))

    @app.exception_handler(HTTPException)
    async def answer_framework(request: Request, error: HTTPException) -> JSONResponse:
        code = _CODE_OF_STATUS.get(error.status_code, Code.UNKNOWN)
        return status_response(code, f"{error.detail}: {request.method} {request.url.path}", error.status_code)



class AnswerFailures:
    """ASGI middleware answering every exception the handlers above did not
    take - a failure of the service, or a KindError wrapped in another
    exception - by its kind, so the request log around it sees the answer the
    client gets. (Starlette answers such an exception outside every
    middleware, where the request log would see none.)
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


def _invalid_request_message(request: Request, errors: Any) -> str:
    if not request.headers.get("content-type", "").startswith("application/json"):
        return "request body: Content-Type must be application/json"
    first = errors[0] if errors else {}
    if first.get("type") == "json_invalid":
        return f"request body is not valid JSON: {first.get('ctx', {}).get('error', '')}"
    where = ".".join(str(part) for part in first.get("loc", ()))
    return f"request {where}: {first.get('msg', 'invalid')}"
