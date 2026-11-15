"""The resources of the HTTP API, one module each: {Resource}Handler."""

from todo.api.http.endpoint.items import ItemsHandler
from todo.api.http.endpoint.lists import ListsHandler

__all__ = ["ItemsHandler", "ListsHandler"]
