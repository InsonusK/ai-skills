"""The services of the gRPC API, one module each: {Service}Handler."""

from todo.api.grpc.endpoint.items import ItemsHandler

__all__ = ["ItemsHandler"]
