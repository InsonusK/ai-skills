# Source: skill base-plateau-in-python

"""The path every command takes from an entry point to its handler: the one
Handler contract all handlers satisfy (handler.py), the decorators that wrap a
handler with a cross-cutting step - one module per decorator: logging.py now,
a unit of work or a metric later - and the error kinds an entry point maps to
its response (errors.py).

It holds no command. The commands live one level up, in {package}.command; entry
points import both - {package}.command for the command types, this package for the
contract and the error kinds.
"""

from {package}.command.pipeline.errors import Kind, KindError, kind_of
from {package}.command.pipeline.handler import Handler, HandlerFunc
from {package}.command.pipeline.logging import with_logging

__all__ = ["Handler", "HandlerFunc", "Kind", "KindError", "kind_of", "with_logging"]
