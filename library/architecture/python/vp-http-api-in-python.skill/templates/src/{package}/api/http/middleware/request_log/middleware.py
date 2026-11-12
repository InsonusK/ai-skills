# Source: skill vp-http-api-in-python

"""The fixed middleware logging every request: two records per request, and
the request id."""

import logging
import time
from datetime import timedelta
from typing import Any
from urllib.parse import parse_qs

from starlette.datastructures import Headers, MutableHeaders
from starlette.types import ASGIApp, Message, Receive, Scope, Send

from {package}.log import attrs, request_id_scope

REQUEST_ID_HEADER = "X-Request-Id"
"""Carries the id of a request: taken from the client when it sends one, else
made here, and always sent back in the response."""


class RequestLogMiddleware:
    """ASGI middleware writing two records per request, so the log reads
    top-down as the request was served: "http request received" when it
    arrives, with the method, the path, and as the payload what the client
    sent - the query, the headers, and the body; then "http response sent"
    once it is answered, with the status, the duration, failed (a status of
    400 or above), and as the payload the body as written, so a fault in
    turning a result into the response shows in the log. A failed request's
    second record repeats what the client sent, so it is in the log even when
    only failed records keep their payload. Every record of the request -
    the command's too - runs inside its request id. The request body is read
    here and given to the routes unchanged.
    """

    def __init__(self, app: ASGIApp, logger: logging.Logger) -> None:
        self._app = app
        self._logger = logger

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        if scope["type"] != "http":
            await self._app(scope, receive, send)
            return
        started = time.perf_counter_ns()
        method, path = scope["method"], scope["path"]
        body = await _read_body(receive)
        received: dict[str, Any] = {
            "query": parse_qs(scope["query_string"].decode("latin-1"), keep_blank_values=True),
            "headers": {key: Headers(scope=scope).getlist(key) for key in Headers(scope=scope).keys()},
            "body": body,
        }
        with request_id_scope(Headers(scope=scope).get(REQUEST_ID_HEADER)) as request_id:
            self._logger.info("http request received", extra=attrs(method=method, path=path, payload=received))
            status, written = 500, bytearray()

            async def recording_send(message: Message) -> None:
                nonlocal status
                if message["type"] == "http.response.start":
                    status = message["status"]
                    MutableHeaders(scope=message)[REQUEST_ID_HEADER] = request_id
                elif message["type"] == "http.response.body":
                    written.extend(message.get("body", b""))
                await send(message)

            await self._app(scope, _replay(body, receive), recording_send)
            failed = status >= 400
            sent: dict[str, Any] = {"response": {"body": bytes(written)}}
            if failed:
                sent["request"] = received
            duration = timedelta(microseconds=(time.perf_counter_ns() - started) / 1_000)
            self._logger.info(
                "http response sent",
                extra=attrs(method=method, path=path, status=status, duration=duration, failed=failed, payload=sent),
            )


async def _read_body(receive: Receive) -> bytes:
    body = bytearray()
    while True:
        message = await receive()
        body.extend(message.get("body", b""))
        if not message.get("more_body", False):
            return bytes(body)


def _replay(body: bytes, receive: Receive) -> Receive:
    # The body read for the log, handed to the route once; then the client's own messages (a disconnect).
    sent = False

    async def replay() -> Message:
        nonlocal sent
        if not sent:
            sent = True
            return {"type": "http.request", "body": body, "more_body": False}
        return await receive()

    return replay
