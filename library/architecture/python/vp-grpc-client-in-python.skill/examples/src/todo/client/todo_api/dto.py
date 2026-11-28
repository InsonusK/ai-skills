"""The logical API DTO contract shared by HTTP and gRPC implementations."""

from dataclasses import dataclass


@dataclass(frozen=True)
class ItemDTO:
    """Provider fields only; Domain Model adaptation belongs to service."""

    title: str
    done: bool
