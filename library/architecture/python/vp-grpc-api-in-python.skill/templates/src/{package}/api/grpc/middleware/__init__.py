# Source: skill vp-grpc-api-in-python

"""The gRPC middleware: the shape every one has (interceptor.py), and one
package each - the fixed call_log and answer_failures, which the server puts
around every call, and an application's, which the composition root lists for
the contract's calls."""

from {package}.api.grpc.middleware.interceptor import CallNext, IncomingCall, Interceptor

__all__ = ["CallNext", "IncomingCall", "Interceptor"]
