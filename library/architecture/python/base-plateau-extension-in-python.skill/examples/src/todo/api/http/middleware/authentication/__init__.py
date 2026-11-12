"""The HTTP middleware of the sample access variation point: it turns the
request's bearer token into the caller's identity (middleware.py). Whether the
caller may run a command is the command layer's decision
(todo.command.middleware.access), so another API, added later, gets the same
check with a middleware of its own."""

from todo.api.http.middleware.authentication.middleware import AuthenticationMiddleware

__all__ = ["AuthenticationMiddleware"]
