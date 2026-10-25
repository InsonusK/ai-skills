# Source: skill vp-grpc-api-in-python

"""The fixed middleware answering every failure of a call by its status."""

import logging

import grpc
from google.protobuf.message import Message

from {package}.api.core import rpcstatus
from {package}.api.grpc.middleware.interceptor import CallNext, IncomingCall
from {package}.log import attrs

# grpc.StatusCode by its number: a member's value is (number, name).
_STATUS_CODES = {status.value[0]: status for status in grpc.StatusCode}


def status_of(error: BaseException) -> tuple[grpc.StatusCode, str]:
    """The status a call that raised error is answered with, through the one
    table of rpcstatus: a kind's code and message, or INTERNAL "internal
    error" for an error with no kind."""
    code, message = rpcstatus.of(error)
    return _STATUS_CODES[int(code)], message


class AnswerFailuresMiddleware:
    """Answers every exception of a call by its kind - a command's error, the
    INVALID_ARGUMENT of a request that cannot become the command, an error an
    application's middleware raised to reject a call - and an error with no
    kind, a crash of the service among them, as INTERNAL "internal error",
    logging it with its stack. Without it grpc.aio answers any exception
    UNKNOWN with the exception's text, leaking what the service knows. A call
    already aborted with a status (context.abort) passes unchanged. The
    server puts it inside the call log and outside the application's
    middleware.
    """

    def __init__(self, logger: logging.Logger) -> None:
        self._logger = logger

    async def __call__(self, call: IncomingCall, call_next: CallNext) -> Message:
        try:
            return await call_next(call)
        except grpc.aio.AbortError:
            raise  # aborted with a status already
        except Exception as error:
            status, message = status_of(error)
            if status is grpc.StatusCode.INTERNAL:
                self._logger.error("grpc call failed", exc_info=error, extra=attrs(method=call.method))
            await call.context.abort(status, message)  # raises AbortError for the call log to see
