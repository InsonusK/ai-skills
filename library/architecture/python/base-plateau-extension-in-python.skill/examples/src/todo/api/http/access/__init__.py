"""The HTTP half of the sample access variation point: the middleware that
turns the request's bearer token into the caller's identity. Whether the
caller may run a command is the command layer's decision (todo.command.access),
so another API, added later, gets the same check with a decorator of its own."""

from todo.api.http.access.authenticate import authenticate

__all__ = ["authenticate"]
