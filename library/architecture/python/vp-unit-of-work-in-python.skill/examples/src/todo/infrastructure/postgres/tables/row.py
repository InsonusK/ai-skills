"""The base of every row of the application."""

from sqlalchemy.orm import DeclarativeBase


class Row(DeclarativeBase):
    """The base of every row: its metadata is every table of the application,
    which `make migration` and the check of the migrations read."""
