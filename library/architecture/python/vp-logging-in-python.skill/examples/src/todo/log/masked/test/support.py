"""What the steps of the logging variation point share: the types the
shared scenarios give as a payload, the messages of masking.proto compiled
from the copy beside the scenarios, and the helpers that read the doc
strings."""

import json
import tempfile
from dataclasses import dataclass
from importlib import resources
from pathlib import Path
from typing import Annotated, Any

from google.protobuf import descriptor_pb2, descriptor_pool, json_format, message_factory
from google.protobuf.message import Message
from grpc_tools import protoc
from pydantic import BaseModel, Field

from todo.log.masked import MASK, OMIT

# The types masking.feature gives as a payload: pydantic models, the Python
# types with a JSON form - an alias, a field excluded from it.


class Person(BaseModel):
    name: str = ""
    pin: Annotated[str, MASK] = ""
    note: Annotated[str, OMIT] = ""
    draft: str = Field(default="", exclude=True)


class Vault(BaseModel):
    code: str = ""


class Team(BaseModel):
    title: str = ""
    lead: Person = Person()
    members: list[Person] = []
    vault: Annotated[Vault, MASK] = Vault()


class Login(BaseModel):
    user: str = ""
    password: str = ""


class Account(BaseModel):
    user: str = ""
    password: str = Field(default="", alias="p")


class Upload(BaseModel):
    name: str = ""
    content: bytes = b""


@dataclass
class Link:
    name: str
    next: "Link | None" = None


def fixture(name: str, given: str) -> Any:
    """The type a scenario names, built from the JSON of its fields; a body
    (Upload.content) is the bytes of its text."""
    fields = json.loads(given)
    if name == "Upload":
        return Upload(name=fields.get("name", ""), content=str(fields.get("content", "")).encode())
    types: dict[str, type[BaseModel]] = {"Person": Person, "Team": Team, "Account": Account, "Login": Login}
    return types[name].model_validate(fields)


def without_comments(text: str) -> str:
    """Drops what follows "//" outside a JSON string on each line of a doc
    string: the scenarios say there what is special about a field."""
    lines = []
    for line in text.splitlines():
        in_string = escaped = False
        for index, char in enumerate(line):
            if escaped:
                escaped = False
            elif in_string and char == "\\":
                escaped = True
            elif char == '"':
                in_string = not in_string
            elif not in_string and line.startswith("//", index):
                line = line[:index]
                break
        lines.append(line.rstrip())
    return "\n".join(lines)


def same_json(got: str, want: str) -> bool:
    """Whether two JSON texts say the same, whatever the order of their keys."""
    return bool(json.loads(got) == json.loads(want))


def encode(value: Any) -> str:
    """JSON as the formatter writes it."""
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"))


_messages: dict[str, type[Message]] = {}


def proto_message(name: str) -> Message:
    """A new message of masking.proto, compiled from the copy beside the
    scenarios and added to the default pool, so an Any of it unpacks as a
    generated message's would."""
    if not _messages:
        features = Path(__file__).resolve().parent.parent / "features"
        includes = resources.files("grpc_tools") / "_proto"
        with tempfile.TemporaryDirectory() as out:
            descriptors = Path(out) / "masking.pb"
            code = protoc.main(["protoc", f"-I{features}", f"-I{includes}", "--include_imports", f"--descriptor_set_out={descriptors}", "masking.proto"])
            assert code == 0, f"protoc failed: {code}"
            files = descriptor_pb2.FileDescriptorSet.FromString(descriptors.read_bytes())
        pool = descriptor_pool.Default()
        for file in files.file:
            try:
                pool.FindFileByName(file.name)
            except KeyError:
                pool.Add(file)
        for message in ("Pin", "Wallet", "Node"):
            _messages[message] = message_factory.GetMessageClass(pool.FindMessageTypeByName(f"conformance.masking.{message}"))
    return _messages[name]()


def parsed_message(name: str, given: str) -> Message:
    """A message of masking.proto from its proto JSON."""
    return json_format.Parse(given, proto_message(name), descriptor_pool=descriptor_pool.Default())


def nested(shape: str, levels: int) -> Any:
    """levels maps or lists, each holding the next, the innermost the text "end"."""
    value: Any = "end"
    for _ in range(levels):
        value = {"next": value} if shape == "maps" else [value]
    return value


def nested_nodes(field: str, levels: int) -> str:
    """levels Node messages as proto JSON, each in the field of the one
    before, the innermost with the title "end"."""
    value = '{"title":"end"}'
    for _ in range(levels - 1):
        if field == "next":
            value = '{"next":' + value + "}"
        elif field == "children":
            value = '{"children":[' + value + "]}"
        else:
            value = '{"named":{"n":' + value + "}}"
    return value
