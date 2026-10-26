# Source: skill vp-grpc-api-in-python

"""The fixed middleware logging every call: two records per call, and the
request id."""

import asyncio
import logging
import time
from datetime import timedelta
from typing import Any

import grpc
from google.protobuf.json_format import MessageToDict
from google.protobuf.message import Message

from todo.api.grpc.middleware.interceptor import CallNext, IncomingCall
from todo.log import attrs, request_id_scope

REQUEST_ID_METADATA = "x-request-id"
"""The metadata key that carries the id of a call: taken from the client when
it sends one, else made here, and sent back in the response's initial
metadata."""


class CallLogMiddleware:
    """Writes two records per call, so the log reads top-down as the call was
    served: "grpc call received" when it arrives, with the method and the
    request message as the payload; then "grpc call answered" with the status
    code, the duration, failed (any code but OK), and the response message as
    returned as the payload, so a fault in turning a result into the response
    shows in the log. A failed call's second record repeats the request, so
    it is in the log even when only failed records keep their payload. Every
    record of the call - the command's too - runs inside its request id. The
    server puts it outside everything else, so a call an application's
    middleware rejected, or one that crashed, is logged with the status it
    was answered with.
    """

    def __init__(self, logger: logging.Logger) -> None:
        self._logger = logger

    async def __call__(self, call: IncomingCall, call_next: CallNext) -> Message:
        started = time.perf_counter_ns()
        given = dict(call.context.invocation_metadata() or ()).get(REQUEST_ID_METADATA)
        with request_id_scope(given if isinstance(given, str) else None) as request_id:
            await call.context.send_initial_metadata(((REQUEST_ID_METADATA, request_id),))
            received = {"request": _json(call.request)}
            self._logger.info("grpc call received", extra=attrs(method=call.method, payload=received))
            response: Message | None = None
            code = grpc.StatusCode.UNKNOWN
            try:
                response = await call_next(call)
                code = grpc.StatusCode.OK
                return response
            except grpc.aio.AbortError:
                code = call.context.code() or grpc.StatusCode.UNKNOWN  # the status it was aborted with
                raise
            except asyncio.CancelledError:
                code = grpc.StatusCode.CANCELLED  # the client went away, or the server stopped
                raise
            finally:
                failed = code is not grpc.StatusCode.OK
                sent: dict[str, Any] = {"response": None if response is None else _json(response)}
                if failed:
                    sent.update(received)
                duration = timedelta(microseconds=(time.perf_counter_ns() - started) / 1_000)
                self._logger.info(
                    "grpc call answered",
                    extra=attrs(method=call.method, code=code.name, duration=duration, failed=failed, payload=sent),
                )


def _json(message: Message) -> Any:
    # A message in its proto-JSON form - the form of its HTTP mapping - so the
    # log, and the masking of the logging variation point, see plain values.
    return MessageToDict(message)
