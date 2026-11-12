# Source: skill error-kinds-in-python

"""The error kinds of the application, re-exported from {package}.shared.errors
so an entry point takes everything of a command from the pipeline. An entry point answers
an error by its kind - kind_of(error) is Kind.NOT_FOUND - never by the
individual error or its text. What each kind means and how every transport
answers it is the table of skill error-kinds.
"""

from {package}.shared.errors import Kind, KindError, kind_of

__all__ = ["Kind", "KindError", "kind_of"]
