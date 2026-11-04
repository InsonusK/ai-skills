# Source: skill error-kinds-in-python

"""The error kinds of the application and the error a caller can act on
(skill error-kinds): KindError, with its kind, its message for the caller,
and its cause for the log - the exception it was raised from."""

from todo.shared.errors.kinds import Kind, KindError, cause_of, error_of, kind_of

__all__ = ["Kind", "KindError", "cause_of", "error_of", "kind_of"]
