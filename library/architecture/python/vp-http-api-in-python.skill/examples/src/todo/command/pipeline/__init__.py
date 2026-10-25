# Source: skill base-plateau-in-python

"""The path every command takes from an entry point to its handler: the one
Handler contract all handlers satisfy (handler.py), the Decorator contract of
a cross-cutting step with decorate, which wraps a handler with a list of them
(decorator.py), the base plateau's one decorator (logging.py), and the error
kinds an entry point maps to its response (errors.py).

It holds no command. The commands live one level up, in todo.command; entry
points import both - todo.command for the command types, this package for the
contract and the error kinds.
"""

from todo.command.pipeline.decorator import Decorator, Next, decorate
from todo.command.pipeline.errors import Kind, KindError, kind_of
from todo.command.pipeline.handler import Handler, HandlerFunc
from todo.command.pipeline.logging import with_logging

__all__ = ["Decorator", "Handler", "HandlerFunc", "Kind", "KindError", "Next", "decorate", "kind_of", "with_logging"]
