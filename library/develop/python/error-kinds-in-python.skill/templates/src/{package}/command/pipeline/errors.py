# Source: skill error-kinds-in-python

"""The error kinds of the application, re-exported from the entity package
so entry points use them without importing the domain. An entry point answers
an error by its kind - kind_of(error) is Kind.NOT_FOUND - never by the
individual error or its text. What each kind means and how every transport
answers it is the table of skill error-kinds.
"""

from {package}.domain.entity.kinds import Kind, KindError, kind_of

__all__ = ["Kind", "KindError", "kind_of"]
