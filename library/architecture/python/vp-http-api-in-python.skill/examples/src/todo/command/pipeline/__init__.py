# Source: skill base-plateau-in-python

"""The path every command takes from an entry point to its handler: the one
Handler contract all handlers satisfy (handler.py), the Middleware contract
of a cross-cutting step with decorate, which wraps a handler with a list of
them (middleware.py), and the error kinds an entry point maps to its response
(errors.py). The middleware themselves live one package each in
todo.command.middleware.

It holds no command. The commands live one level up, in todo.command; entry
points import both - todo.command for the command types, this package for the
contract and the error kinds.
"""

from todo.command.pipeline.errors import Kind, KindError, kind_of
from todo.command.pipeline.handler import Handler, HandlerFunc
from todo.command.pipeline.middleware import Middleware, Next, decorate

__all__ = ["Handler", "HandlerFunc", "Kind", "KindError", "Middleware", "Next", "decorate", "kind_of"]
