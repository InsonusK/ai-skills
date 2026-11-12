"""The table list_items: the items of every list."""

from dataclasses import dataclass

from sqlalchemy import Identity, Select, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column

from todo.infrastructure.postgres.specification import Key, Specification
from todo.infrastructure.postgres.tables.row import Row


class ItemRow(Row):
    """One item of a list; a title is unique in its list."""

    __tablename__ = "list_items"
    __table_args__ = (UniqueConstraint("list_id", "title", name="list_items_list_id_title_key"),)

    id: Mapped[int] = mapped_column(Identity(), primary_key=True)
    list_id: Mapped[str]
    title: Mapped[str]
    done: Mapped[bool]


ItemPosition: Key[ItemRow] = Key(ItemRow.id)
"""The order the items were added in."""


@dataclass(frozen=True)
class ItemsOfList(Specification[ItemRow]):
    """The items of the list list_id."""

    list_id: str

    def pick(self, query: Select[ItemRow]) -> Select[ItemRow]:
        return query.where(ItemRow.list_id == self.list_id)
