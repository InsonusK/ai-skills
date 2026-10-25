"""The HTTP entry point of the application: its resources (endpoint/), their
registration (routes.py), and the fixed code (pipeline/)."""

from todo.api.http.routes import Endpoints, routes, run

__all__ = ["Endpoints", "routes", "run"]
