# Source: skill base-plateau-in-python

"""The path every command takes from an entry point to its handler: the one
Handler contract all handlers satisfy (handler.py), the Middleware contract
of a cross-cutting step with decorate, which wraps a handler with a list of
them (middleware.py), and the error kinds an entry point maps to its response
(errors.py). The middleware themselves live one package each in
{package}.command.middleware.

It holds no command. The commands live one level up, in {package}.command; entry
points import both - {package}.command for the command types, this package for the
contract and the error kinds.
"""

from {package}.command.pipeline.errors import Kind, KindError, kind_of
from {package}.command.pipeline.handler import Handler, HandlerFunc
from {package}.command.pipeline.middleware import Middleware, Next, decorate

__all__ = ["Handler", "HandlerFunc", "Kind", "KindError", "Middleware", "Next", "decorate", "kind_of"]
