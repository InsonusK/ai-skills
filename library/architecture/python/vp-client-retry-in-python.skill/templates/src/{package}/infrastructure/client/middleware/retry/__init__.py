# Source: skill vp-client-retry-in-python

"""The client middleware that sends a call again when the API did not answer:
only a call of an operation its adapter marked retryable, only on
ApiUnavailableError, at most {API}_RETRY_ATTEMPTS times in all, waiting
{API}_RETRY_BACKOFF and twice as long before each next attempt."""

from {package}.infrastructure.client.middleware.retry.config import DEFAULT_ATTEMPTS, DEFAULT_BACKOFF, RetryConfig, load_config
from {package}.infrastructure.client.middleware.retry.middleware import RetryMiddleware

__all__ = ["DEFAULT_ATTEMPTS", "DEFAULT_BACKOFF", "RetryConfig", "RetryMiddleware", "load_config"]
