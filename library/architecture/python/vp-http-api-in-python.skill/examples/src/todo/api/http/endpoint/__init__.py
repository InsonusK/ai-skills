"""The resources of the HTTP API, one module each: {Resource}Handler."""

from todo.api.http.endpoint.items import ItemsHandler

__all__ = ["ItemsHandler"]
