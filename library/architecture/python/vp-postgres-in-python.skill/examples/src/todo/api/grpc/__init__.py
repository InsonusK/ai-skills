"""The gRPC entry point of the application: its services (endpoint/), their
registration (services.py), the middleware (middleware/), and the fixed code
(pipeline/)."""

from todo.api.grpc.middleware import Interceptor
from todo.api.grpc.services import Endpoints, entry, services

__all__ = ["Endpoints", "Interceptor", "entry", "services"]
