import datetime
import decimal
import enum
import io
import json
import logging
from collections.abc import Iterator
from dataclasses import dataclass, field
from typing import Annotated, Any

import pytest
from google.protobuf.any_pb2 import Any as AnyMessage
from pytest_bdd import given, parsers, scenarios, then, when

from todo.log import attrs
from todo.log.masked import MASK, OMIT, MaskedConfig, Unparsed, mask, new_masked_logger, unmask

from .support import encode, nested, nested_nodes, parsed_message, proto_message, same_json

scenarios("../features/python-payload.feature")

_LEVELS = {"DEBUG": logging.DEBUG, "INFO": logging.INFO, "WARN": logging.WARNING, "ERROR": logging.ERROR}


@dataclass(frozen=True, slots=True)
class AddUser:
    """A command as the application declares one, its secrets marked; with
    slots, so it has no __dict__ to fall back to."""

    pin: Annotated[str, MASK]
    note: Annotated[str, OMIT]
    token: str
    title: str


class Draft:
    """An object of no known kind: its public attributes are its data, its
    class annotations its marks."""

    pin: Annotated[str, MASK]

    def __init__(self, title: str, pin: str, cache: str) -> None:
        self.title = title
        self.pin = pin
        self._cache = cache


@dataclass(frozen=True, slots=True)
class Attachment:
    name: str
    content: bytes


class Envelope:
    def __init__(self, name: str, content: bytes) -> None:
        self.name = name
        self.content = content


class Label:
    """No attributes of its own: it is written as its text."""

    __slots__ = ("text",)

    def __init__(self, text: str) -> None:
        self.text = text

    def __str__(self) -> str:
        return self.text


class Kind(enum.Enum):
    KIND_GOLD = "gold"


@dataclass
class Link:
    name: str
    next: "Link | None" = None


@dataclass
class World:
    level: int = logging.INFO
    max_bytes: int = 4096
    out: io.StringIO = field(default_factory=io.StringIO)
    rendered: str = ""
    unparsed: list[Unparsed] = field(default_factory=list)

    def logger(self) -> logging.Logger:
        return new_masked_logger(self.out, self.level, MaskedConfig(max_bytes=self.max_bytes))

    def render(self, value: Any, unmasked: bool = False) -> None:
        if unmasked:
            written, self.unparsed = unmask(value), []
        else:
            written, self.unparsed = mask(value)
        self.rendered = encode(written)
        print(f"when: mask -> {self.rendered}, unparsed {self.unparsed}")

    def lines(self) -> list[str]:
        written = self.out.getvalue().splitlines()
        print("then: log\n" + "\n".join(written))
        return written


@pytest.fixture
def world() -> Iterator[World]:
    yield World()


@given(parsers.parse('the log is written from the level "{level}"'))
def the_log_is_written_from(world: World, level: str) -> None:
    world.level = _LEVELS[level]


@when(parsers.parse('a record "{message}" is logged with the argument "{argument}"'))
def a_record_with_an_argument(world: World, message: str, argument: str) -> None:
    world.logger().info(message, argument)


@when(parsers.parse('a LoggerAdapter with the payload password "{password}" and title "{title}" logs a record'))
def a_logger_adapter_logs(world: World, password: str, title: str) -> None:
    adapter = logging.LoggerAdapter(world.logger(), attrs(command="PlanItems", payload={"password": password, "title": title}))
    adapter.info("command called")


@when(parsers.re(r"a record \"(?P<message>[^\"]*)\" is logged at WARN with the payload body '(?P<body>[^']*)'"))
def a_record_at_warn(world: World, message: str, body: str) -> None:
    world.logger().warning(message, extra=attrs(failed=True, payload={"body": body.encode()}))


@when(parsers.parse('a record is logged with the payload n = {n:d} and a title of {count:d} "{char}" with LOG_PAYLOAD_MAX_BYTES {limit:d}'))
def a_record_with_wide_characters(world: World, n: int, count: int, char: str, limit: int) -> None:
    world.max_bytes = limit
    world.logger().info("command called", extra=attrs(payload={"n": n, "title": char * count}))


@when(parsers.parse('the payload is a dataclass with the pin "{pin}" marked to mask, the note "{note}" marked to omit, the token "{token}", and the title "{title}"'))
def the_payload_is_a_dataclass(world: World, pin: str, note: str, token: str, title: str) -> None:
    world.render(AddUser(pin=pin, note=note, token=token, title=title))


@when(parsers.parse('the payload is an object with the title "{title}", the pin "{pin}" marked to mask, and the private cache "{cache}"'))
def the_payload_is_an_object(world: World, title: str, pin: str, cache: str) -> None:
    world.render(Draft(title, pin, cache))


@when(parsers.re(r"the payload is (?P<holder>a dataclass|an object) with the name \"(?P<name>[^\"]*)\" and the content '(?P<content>[^']*)'"))
def the_payload_holds_content(world: World, holder: str, name: str, content: str) -> None:
    world.render(Attachment(name, content.encode()) if holder == "a dataclass" else Envelope(name, content.encode()))


@when(parsers.parse('the payload is the time "{at}"'))
def the_payload_is_a_time(world: World, at: str) -> None:
    world.render(datetime.datetime.fromisoformat(at))


@when(parsers.parse('the payload is the enum member {name} = "{value}"'))
def the_payload_is_an_enum_member(world: World, name: str, value: str) -> None:
    world.render(Kind[name])


@when(parsers.parse('the payload is the decimal "{value}"'))
def the_payload_is_a_decimal(world: World, value: str) -> None:
    world.render(decimal.Decimal(value))


@when(parsers.parse('the payload is the object of no attributes written "{text}"'))
def the_payload_is_a_label(world: World, text: str) -> None:
    world.render(Label(text))


@when(parsers.re(r"the payload is a map with \"(?P<key>[^\"]*)\" = '(?P<text>[^']*)'"))
def the_payload_is_a_map_with_a_text(world: World, key: str, text: str) -> None:
    world.render({key: text})


@when(parsers.re(r"the payload is a (?P<name>\w+) message given as '(?P<given>.*)'$"))
def the_payload_is_a_message_given_as(world: World, name: str, given: str) -> None:
    world.render(parsed_message(name, given))


@when(parsers.re(r"the payload is a Wallet whose extra is an Any of the type \"(?P<url>[^\"]*)\" holding a Wallet with the blob '(?P<blob>[^']*)'"))
def the_payload_holds_a_wallet_in_an_any(world: World, url: str, blob: str) -> None:
    inner = proto_message("Wallet")
    setattr(inner, "blob", blob.encode())
    wallet = proto_message("Wallet")
    getattr(wallet, "extra").CopyFrom(AnyMessage(type_url=url, value=inner.SerializeToString()))
    world.render(wallet)


@when(parsers.re(r"the payload is (?P<shape>dataclass links|Node messages each in children|Node messages each in named|maps, masking off,) nested (?P<levels>\d+) levels deep"))
def the_payload_is_nested(world: World, shape: str, levels: str) -> None:
    count = int(levels)
    if shape == "dataclass links":
        link = Link(name="end")
        for _ in range(count - 1):
            link = Link(name="link", next=link)
        world.render(link)
    elif shape.startswith("Node"):
        world.render(parsed_message("Node", nested_nodes(shape.rpartition(" ")[2], count)))
    else:
        world.render(nested("maps", count), unmasked=True)


@when(parsers.parse('masking is off and the payload is the bytes "{hex}"'))
def the_payload_is_bytes_unmasked(world: World, hex: str) -> None:
    world.render(bytes.fromhex(hex), unmasked=True)


@then(parsers.re(r"the masked payload should be '(?P<want>[^']*)'"))
def the_masked_payload_should_be(world: World, want: str) -> None:
    assert same_json(world.rendered, want), world.rendered


@then(parsers.re(r'the masked payload should (?P<how>mention|not mention) "(?P<text>[^"]*)"'))
def the_masked_payload_should_mention(world: World, how: str, text: str) -> None:
    assert (text in world.rendered) == (how == "mention"), world.rendered


@then(parsers.re(r'the mask should report the unparsed body "(?P<path>[^"]*)" of (?P<size>\d+) bytes'))
def the_mask_should_report(world: World, path: str, size: str) -> None:
    assert world.unparsed == [Unparsed(path=path, bytes=int(size))]


@then(parsers.re(r'the record should (?P<how>mention|not mention) "(?P<text>.*)"$'))
def the_record_should_mention(world: World, how: str, text: str) -> None:
    line = world.lines()[0]
    assert (text in json.dumps(json.loads(line), ensure_ascii=False, separators=(",", ":"))) == (how == "mention"), line


@then(parsers.parse('the log should not mention "{text}"'))
def the_log_should_not_mention(world: World, text: str) -> None:
    assert text not in "\n".join(world.lines())


@then(parsers.parse('the log should hold the warning "{message}"'))
def the_log_should_hold_the_warning(world: World, message: str) -> None:
    assert [json.loads(line)["msg"] for line in world.lines()].count(message) == 1
