# Source: skill vp-http-client-in-python

"""HttpClient.call: one request to an HTTP API and the reading of its answer."""

import asyncio
import dataclasses
import json
import logging
import time
from collections.abc import Callable
from datetime import timedelta
from typing import Any

import httpx2

from todo.infrastructure.client.core.httpclient.config import MOCK, HttpConfig
from todo.infrastructure.client.core.outcomes import ApiStatusError, ApiUnavailableError, StatusCode
from todo.log import attrs, request_id

REQUEST_ID_HEADER = "X-Request-Id"
"""Carries the request id of the call being served to the API."""

_MAX_BODY = 10 << 20
"""The most of an answer read; a longer one is an error."""

_CODES = {code.value for code in StatusCode}

Emulator = Callable[[httpx2.Request], httpx2.Response]
"""The emulator of an API: answers every request inside the process, as the
API would - the mock of one adapter (infrastructure/client/{api}/mock)."""


class HttpClient:
    """Calls one HTTP API: the fixed part of every adapter of such an API,
    which writes each of its operations as one call() with its path, its
    request, and the function that reads its answer."""

    def __init__(self, config: HttpConfig, logger: logging.Logger, emulator: Emulator) -> None:
        """Builds the client of the API config names. Mocked, every call is
        answered by emulator inside the process - the same request, log, and
        reading of the answer, only no network - and a WARN record says so."""
        self._config = config
        self._logger = logger
        if config.mocked:
            # The timeout of a call is the client's own, per call, in call().
            self._http = httpx2.AsyncClient(base_url=f"http://{MOCK}", transport=httpx2.MockTransport(emulator), timeout=None)
            logger.warning("api client mocked", extra=attrs(api=config.name))
        else:
            self._http = httpx2.AsyncClient(base_url=config.base_url, timeout=None)

    async def aclose(self) -> None:
        """Closes the connections to the API."""
        await self._http.aclose()

    async def call[T](self, method: str, path: str, request: Any, decode: Callable[[Any], T]) -> T:
        """Sends one request - request (a dataclass, or anything JSON has a
        form for) as its JSON body, none when None - and returns decode of
        the JSON of a 2xx answer. It ends after the timeout at the latest,
        sends the request id of the call being served, and writes two
        records: "http client request sent" and "http client response
        received". It raises ApiUnavailableError, ApiStatusError, or
        ValueError when decode cannot read the answer; a cancelled task is
        cancelled as it is.
        """
        called = {"api": self._config.name, "method": method, "path": path}
        self._logger.info("http client request sent", extra=attrs(**called, payload={"request": request}))
        started = time.monotonic()
        status = 0
        received: Any = None
        failed = True
        try:
            headers = {"Accept": "application/json"}
            if id := request_id():
                headers[REQUEST_ID_HEADER] = id
            body = None
            if request is not None:
                body = json.dumps(dataclasses.asdict(request) if dataclasses.is_dataclass(request) and not isinstance(request, type) else request)
                headers["Content-Type"] = "application/json"
            try:
                async with asyncio.timeout(self._config.timeout.total_seconds()):
                    answer = await self._http.request(method, path, content=body, headers=headers)
                    status = answer.status_code
                    raw = await answer.aread()
            except httpx2.TransportError as error:
                raise ApiUnavailableError(self._config.name, f"{method} {path}: {error!r}") from error
            except TimeoutError as error:
                raise ApiUnavailableError(self._config.name, f"{method} {path}: no answer within {self._config.timeout}") from error
            if len(raw) > _MAX_BODY:
                raise ValueError(f"{method} {path}: the answer is longer than {_MAX_BODY} bytes")
            received = raw.decode("utf-8", errors="replace")
            if status in (502, 503, 504):
                raise ApiUnavailableError(self._config.name, f"{method} {path}: answered {status}")
            if status >= 400:
                raise self._status_error(status, received)
            try:
                response = decode(json.loads(received))
            except (ValueError, KeyError, TypeError) as error:
                raise ValueError(f"{method} {path}: the answer does not match the contract: {error!r}") from error
            received = response
            failed = False
            return response
        finally:
            # A failed call's record repeats the request, so it is in the log
            # even when only failed records keep their payload.
            payload: dict[str, Any] = {"response": received}
            if failed:
                payload["request"] = request
            self._logger.info(
                "http client response received",
                extra=attrs(
                    **called,
                    status=status,
                    duration=timedelta(seconds=time.monotonic() - started),
                    failed=failed,
                    payload=payload,
                ),
            )

    def _status_error(self, status: int, body: str) -> ApiStatusError:
        """Reads a google.rpc.Status body, or keeps the status alone with the code UNKNOWN."""
        try:
            parsed = json.loads(body)
        except ValueError:
            parsed = None
        code = parsed.get("code") if isinstance(parsed, dict) else None
        if isinstance(code, int) and code in _CODES and code != StatusCode.OK:
            return ApiStatusError(self._config.name, StatusCode(code), str(parsed.get("message", "")), status)
        return ApiStatusError(self._config.name, StatusCode.UNKNOWN, "", status)
