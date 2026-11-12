# Source: skill vp-logging-in-python

"""The mask: a payload turned into plain data - dicts, lists, and scalars
that json.dumps writes - with every secret replaced and every body it cannot
read kept out."""

import dataclasses
import datetime
import decimal
import enum
import json
import pathlib
import uuid
from collections.abc import Mapping
from dataclasses import dataclass
from typing import Any, get_type_hints

from google.protobuf import descriptor, descriptor_pool, message_factory
from google.protobuf.message import DecodeError, Message

MASK = "log:mask"
"""Marks a field whose value is secret: `pin: Annotated[str, "log:mask"]`,
in a dataclass or a pydantic model. A plain string, like a Go struct tag, so
the code that marks a field imports nothing of this package."""

OMIT = "log:omit"
"""Marks a field that is never logged: `avatar: Annotated[bytes, "log:omit"]`."""

MASKED = "***"
"""What a masked value is written as."""

TOO_DEEP = "<too deep>"
"""What a value more than MAX_DEPTH levels below the payload is written as:
a cycle of references ends here instead of in a RecursionError."""

MAX_DEPTH = 32
"""How many levels below the payload are written."""

ROOT = "$"
"""The path of the payload itself; a body below it is reported as $.body, $.items[0]."""

SECRET_NAMES = (
    "password", "passwd", "pwd", "passphrase", "secret", "token", "authorization",
    "cookie", "apikey", "privatekey", "credential", "session", "cvv",
)  # fmt: skip
"""Mark a field, a map key, or a JSON key as secret whatever its value and
however it is marked: the safety net for a secret nobody marked. A name
matches when, lower-cased and without "_" and "-", it contains one."""


@dataclass(frozen=True)
class Unparsed:
    """A body the mask could not read - bytes that are not JSON, or a proto
    Any of a type this process does not know - and so wrote as
    "<unparsed body, N bytes>" instead of as it is."""

    path: str
    """Where the body is in the payload: $, $.body, $.items[0].blob."""

    bytes: int


def secret(name: str) -> bool:
    """Whether name is one of SECRET_NAMES, whatever its case, separators, and affixes."""
    normalized = name.lower().replace("_", "").replace("-", "")
    return any(known in normalized for known in SECRET_NAMES)


def mask(value: Any) -> tuple[Any, list[Unparsed]]:
    """value as plain data with every secret replaced by MASKED and every
    omitted field left out, and the bodies it could not read. A value is
    secret when

    - a field of a dataclass or a pydantic model is marked MASK (OMIT, or a
      pydantic field excluded from its JSON form, leaves it out);
    - a field of a proto message has [debug_redact = true];
    - its field name (in the code or in its JSON form), map key, or JSON key
      matches SECRET_NAMES.

    Bytes (a request body) are parsed as JSON and masked by their keys, and
    so is a text that holds a JSON object or array; bytes that are not JSON
    are never written as they are, since nothing in them can be masked.
    """
    walk = _Walk()
    return walk.value(value, ROOT, 0), walk.unparsed


def unmask(value: Any) -> Any:
    """value as plain data like mask, but masking nothing: marks, secret
    names, and debug_redact are ignored, and a body that is not JSON is its
    text. Only for debugging, with LOG_PAYLOAD_UNMASKED: a field marked OMIT
    or left out of its JSON form stays out, and the depth limit holds."""
    return _Walk(unmasked=True).value(value, ROOT, 0)


class _Walk:
    """One walk over one payload, collecting the bodies it could not read."""

    def __init__(self, unmasked: bool = False) -> None:
        self.unmasked = unmasked
        self.unparsed: list[Unparsed] = []

    def secret(self, name: str) -> bool:
        return not self.unmasked and secret(name)

    def value(self, value: Any, path: str, depth: int) -> Any:
        if value is None:
            return None
        if depth > MAX_DEPTH:
            return TOO_DEEP
        if isinstance(value, Message):
            return self.message(value, path, depth)
        if isinstance(value, (bytes, bytearray, memoryview)):
            return self.body(bytes(value), path, depth)
        if isinstance(value, str):
            return self.text(value, path, depth)
        if isinstance(value, enum.Enum):
            return self.value(value.value, path, depth)
        if isinstance(value, (bool, int, float)):
            return value
        if isinstance(value, (datetime.date, datetime.time)):
            return value.isoformat()  # a value that writes itself as text (R05)
        if isinstance(value, (decimal.Decimal, uuid.UUID, pathlib.PurePath, datetime.timedelta)):
            return str(value)
        if isinstance(value, Mapping):
            return self.mapping(value, path, depth)
        if isinstance(value, (list, tuple, set, frozenset)):
            return [self.value(item, f"{path}[{index}]", depth + 1) for index, item in enumerate(value)]
        if dataclasses.is_dataclass(value) and not isinstance(value, type):
            names = [(field.name, field.name) for field in dataclasses.fields(value)]
            return self.object(value, names, _marks(type(value)), path, depth)
        fields = getattr(type(value), "model_fields", None)
        if isinstance(fields, Mapping):  # a pydantic model: its JSON form
            return self.model(value, fields, path, depth)
        if hasattr(value, "__dict__"):  # any other object: its public attributes
            names = [(name, name) for name in vars(value) if not name.startswith("_")]
            return self.object(value, names, _marks(type(value)), path, depth)
        return str(value)

    def mapping(self, value: Mapping[Any, Any], path: str, depth: int) -> dict[str, Any]:
        out: dict[str, Any] = {}
        for key, item in value.items():
            key = str(key)
            out[key] = MASKED if self.secret(key) else self.value(item, f"{path}.{key}", depth + 1)
        return out

    def object(self, value: Any, names: list[tuple[str, str]], marks: Mapping[str, tuple[Any, ...]], path: str, depth: int) -> dict[str, Any]:
        # names: (the attribute, the name it is written under)
        out: dict[str, Any] = {}
        for attribute, name in names:
            marked = marks.get(attribute, ())
            if OMIT in marked:
                continue
            if (not self.unmasked and MASK in marked) or self.secret(name) or self.secret(attribute):
                out[name] = MASKED
            else:
                out[name] = self.value(getattr(value, attribute), f"{path}.{name}", depth + 1)
        return out

    def model(self, value: Any, fields: Mapping[str, Any], path: str, depth: int) -> dict[str, Any]:
        # A pydantic field excluded from the model's JSON form is left out;
        # one with an alias is written under it; its marks are the metadata
        # of its Annotated type.
        names = [(name, info.alias or name) for name, info in fields.items() if not info.exclude]
        marks = {name: tuple(info.metadata) for name, info in fields.items()}
        return self.object(value, names, marks, path, depth)

    def text(self, text: str, path: str, depth: int) -> Any:
        # A text as it is, unless it holds a JSON object or array: then that
        # JSON masked by its keys.
        trimmed = text.strip()
        if not trimmed.startswith(("{", "[")):
            return text
        try:
            parsed = json.loads(trimmed)
        except ValueError:
            return text
        return self.value(parsed, path, depth)

    def body(self, raw: bytes, path: str, depth: int) -> Any:
        # Bytes parsed as JSON and masked by their keys; bytes that are not
        # JSON are reported and written as their size only. Empty bytes are "".
        if not raw:
            return ""
        try:
            parsed = json.loads(raw)
        except ValueError:
            return self.unreadable(raw, path)
        return self.value(parsed, path, depth)

    def unreadable(self, raw: bytes, path: str) -> str:
        # Bytes nothing can be masked in: reported and written as their size
        # only - or, unmasked, as their text.
        if self.unmasked:
            return raw.decode(errors="replace")
        self.unparsed.append(Unparsed(path=path, bytes=len(raw)))
        return f"<unparsed body, {len(raw)} bytes>"

    def message(self, message: Message, path: str, depth: int) -> Any:
        # A proto message by its descriptor: the JSON name of each set field,
        # [debug_redact = true] masked.
        if message.DESCRIPTOR.full_name == "google.protobuf.Any":
            return self.any_message(message, path, depth)
        out: dict[str, Any] = {}
        for field, value in message.ListFields():
            name = field.json_name
            if (not self.unmasked and field.GetOptions().debug_redact) or self.secret(name):
                out[name] = MASKED
            else:
                out[name] = self.field(field, value, f"{path}.{name}", depth + 1)
        return out

    def any_message(self, message: Message, path: str, depth: int) -> Any:
        # An Any whose type is in the default pool - every generated message
        # is - unpacked and written like proto JSON, with its "@type"; an Any
        # of an unknown type is bytes nothing can be masked in.
        url: str = getattr(message, "type_url")
        raw: bytes = getattr(message, "value")
        try:
            inner_type = descriptor_pool.Default().FindMessageTypeByName(url.rpartition("/")[2])
            inner = message_factory.GetMessageClass(inner_type)()
            inner.ParseFromString(raw)
        except (KeyError, DecodeError):
            return self.unreadable(raw, path)
        out = self.message(inner, path, depth)
        out["@type"] = url
        return out

    def field(self, field: descriptor.FieldDescriptor, value: Any, path: str, depth: int) -> Any:
        if depth > MAX_DEPTH:
            return TOO_DEEP
        if field.message_type is not None and field.message_type.GetOptions().map_entry:
            value_field = field.message_type.fields_by_name["value"]
            return {
                str(key): MASKED if self.secret(str(key)) else self.scalar(value_field, item, f"{path}.{key}", depth + 1)
                for key, item in value.items()
            }
        if field.is_repeated:
            return [self.scalar(field, item, f"{path}[{index}]", depth + 1) for index, item in enumerate(value)]
        return self.scalar(field, value, path, depth)

    def scalar(self, field: descriptor.FieldDescriptor, value: Any, path: str, depth: int) -> Any:
        if depth > MAX_DEPTH:
            return TOO_DEEP
        if field.type in (descriptor.FieldDescriptor.TYPE_MESSAGE, descriptor.FieldDescriptor.TYPE_GROUP):
            return self.message(value, path, depth)
        if field.enum_type is not None:  # an enum field
            known = field.enum_type.values_by_number.get(value)
            return known.name if known is not None else value
        if field.type == descriptor.FieldDescriptor.TYPE_BYTES:
            return self.body(value, path, depth)
        if field.type == descriptor.FieldDescriptor.TYPE_STRING:
            return self.text(value, path, depth)
        return value


def _marks(cls: type) -> dict[str, tuple[Any, ...]]:
    # The Annotated extras of each field of a class: its marks.
    try:
        hints = get_type_hints(cls, include_extras=True)
    except Exception:
        return {}
    return {name: tuple(getattr(hint, "__metadata__", ())) for name, hint in hints.items()}


