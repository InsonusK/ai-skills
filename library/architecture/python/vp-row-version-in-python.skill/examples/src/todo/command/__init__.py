"""The commands of the application: one module per command, holding the
command, its result types, and its handler; and the command registry
(handlers.py) that builds every handler."""

from todo.command.add_item import AddItemCommand, AddItemHandler, AddItemResult
from todo.command.complete_item import CompleteItemCommand, CompleteItemHandler, CompleteItemResult
from todo.command.handlers import Handlers, ItemLists, build_handlers
from todo.command.list_items import ListItemsCommand, ListItemsHandler, ListItemsResult
from todo.command.plan_items import ItemView, PlanItemsCommand, PlanItemsHandler, PlanItemsResult

__all__ = [
    "AddItemCommand",
    "AddItemHandler",
    "AddItemResult",
    "CompleteItemCommand",
    "CompleteItemHandler",
    "CompleteItemResult",
    "Handlers",
    "ItemLists",
    "ItemView",
    "ListItemsCommand",
    "ListItemsHandler",
    "ListItemsResult",
    "PlanItemsCommand",
    "PlanItemsHandler",
    "PlanItemsResult",
    "build_handlers",
]
