"""An item kept in a list."""

from dataclasses import dataclass

from todo.domain.entity.item import Item


@dataclass
class KeptItem:
    """An item kept in a list: its id, the version of it the caller read,
    which the caller gives back with a change, and the item itself. The
    version is opaque (skill vp-row-version): it is carried and given back,
    never read or computed with."""

    id: int
    version: str
    item: Item
