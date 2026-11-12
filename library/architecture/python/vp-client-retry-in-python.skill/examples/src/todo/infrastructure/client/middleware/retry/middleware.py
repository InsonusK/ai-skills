# Source: skill vp-client-retry-in-python

"""RetryMiddleware: a call the API did not answer, sent again."""

import asyncio
import logging
import random
from datetime import timedelta

from todo.infrastructure.client.core.outcomes import ApiUnavailableError
from todo.infrastructure.client.core.pipeline import Attempt, Call
from todo.infrastructure.client.middleware.retry.config import RetryConfig
from todo.log import attrs


class RetryMiddleware:
    """Sends a retryable call again while its attempt raises
    ApiUnavailableError, up to config.attempts attempts in all. Each attempt
    has the whole {API}_TIMEOUT of the client. The caller's cancellation ends
    a wait as it is: asyncio.CancelledError passes through, because a task or
    an asyncio.timeout that cancels must see it. Before each repeat a WARN
    record "api client call retried" names the API, the operation, the attempt
    that failed, the wait, and its error. A call that is not retryable, or
    that ends any other way, passes on unchanged - a repeat of a call the API
    answered could do its work twice."""

    def __init__(self, config: RetryConfig, logger: logging.Logger) -> None:
        self._config = config
        self._logger = logger

    async def __call__(self, call: Call, call_next: Attempt) -> None:
        if not call.retryable:
            await call_next()
            return
        backoff = self._config.backoff
        attempt = 1
        while True:
            try:
                await call_next()
                return
            except ApiUnavailableError as error:
                if attempt >= self._config.attempts:
                    raise
                wait = _jitter(backoff)
                self._logger.warning(
                    "api client call retried",
                    extra=attrs(api=call.api, operation=call.operation, attempt=attempt, wait=wait, error=str(error)),
                )
            await asyncio.sleep(wait.total_seconds())
            backoff *= 2
            attempt += 1


def _jitter(backoff: timedelta) -> timedelta:
    """A wait between half and one and a half of backoff, so the clients an
    outage stopped together do not all call again at once."""
    return backoff * random.uniform(0.5, 1.5)
