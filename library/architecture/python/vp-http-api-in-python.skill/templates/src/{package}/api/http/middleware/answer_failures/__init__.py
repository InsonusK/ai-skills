# Source: skill vp-http-api-in-python

"""The fixed middleware answering every failure of a request (middleware.py)."""

from {package}.api.http.middleware.answer_failures.middleware import AnswerFailuresMiddleware, status_response

__all__ = ["AnswerFailuresMiddleware", "status_response"]
