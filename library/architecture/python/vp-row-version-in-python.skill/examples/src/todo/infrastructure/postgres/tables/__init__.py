"""The application's rows: one class per table, mapped by SQLAlchemy, with
the keys to order it by and the specifications that pick its rows. Every row
derives from Row, whose metadata the migrations are checked against."""

from todo.infrastructure.postgres.tables.items import ItemOfList, ItemPosition, ItemRow, ItemsOfList
from todo.infrastructure.postgres.tables.marks import NeverChanges
from todo.infrastructure.postgres.tables.row import Row

__all__ = ["ItemOfList", "ItemPosition", "ItemRow", "ItemsOfList", "NeverChanges", "Row"]
