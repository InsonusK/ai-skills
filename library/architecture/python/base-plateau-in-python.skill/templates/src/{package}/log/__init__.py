# Source: skill base-plateau-in-python

"""The one way the application writes logs (records.py), and the request id
of the call being served (request_id.py)."""

from {package}.log.records import (
    FAILED_KEY,
    PAYLOAD_KEY,
    JsonFormatter,
    attributes_of,
    attrs,
    json_value,
    level_name,
    new_logger,
)
from {package}.log.request_id import new_request_id, request_id, request_id_scope

__all__ = [
    "FAILED_KEY",
    "PAYLOAD_KEY",
    "JsonFormatter",
    "attributes_of",
    "attrs",
    "json_value",
    "level_name",
    "new_logger",
    "new_request_id",
    "request_id",
    "request_id_scope",
]
