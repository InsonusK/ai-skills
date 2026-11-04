# Source: skill vp-http-api-in-python

"""Every error of the HTTP entry point answered with a google.rpc.Status
body: a command's error by its kind (todo.api.core.rpcstatus), a request that
does not fit its route's types as INVALID_ARGUMENT without calling the
command, and the answers the framework gives itself - an unknown path, a
method a path does not serve - with the code of their status.
"""

from typing import Any

from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from starlette.exceptions import HTTPException

from todo.api.core import rpcstatus
from todo.api.core.rpcstatus import Code
from todo.api.http.middleware.answer_failures import status_response
from todo.command.pipeline import KindError

# The code of each status the framework answers by itself.
_CODE_OF_STATUS = {
    404: Code.NOT_FOUND,
    405: Code.UNIMPLEMENTED,
    413: Code.RESOURCE_EXHAUSTED,
}


def add_error_answers(app: FastAPI) -> None:
    """Registers the handlers that answer the errors the framework knows of;
    AnswerFailuresMiddleware answers the rest."""

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



def _invalid_request_message(request: Request, errors: Any) -> str:
    if not request.headers.get("content-type", "").startswith("application/json"):
        return "request body: Content-Type must be application/json"
    first = errors[0] if errors else {}
    if first.get("type") == "json_invalid":
        return f"request body is not valid JSON: {first.get('ctx', {}).get('error', '')}"
    where = ".".join(str(part) for part in first.get("loc", ()))
    return f"request {where}: {first.get('msg', 'invalid')}"
