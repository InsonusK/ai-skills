# Source: skill vp-grpc-api-in-python

"""The fixed middleware answering every failure of a call (middleware.py)."""

from todo.api.grpc.middleware.answer_failures.middleware import AnswerFailuresMiddleware, status_of

__all__ = ["AnswerFailuresMiddleware", "status_of"]
