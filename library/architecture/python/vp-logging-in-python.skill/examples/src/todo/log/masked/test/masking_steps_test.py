import json
from dataclasses import dataclass, field
from typing import Any

import pytest
from google.protobuf.any_pb2 import Any as AnyMessage
from pytest_bdd import given, parsers, scenarios, then, when

from todo.log.masked import Unparsed, mask, unmask

from .support import Link, encode, fixture, nested, nested_nodes, parsed_message, proto_message, same_json, without_comments

scenarios("../features/masking.feature")


@dataclass
class World:
    unmasked: bool = False
    rendered: str = ""
    unparsed: list[Unparsed] = field(default_factory=list)

    def render(self, value: Any) -> None:
        if self.unmasked:
            written, self.unparsed = unmask(value), []
        else:
            written, self.unparsed = mask(value)
        self.rendered = encode(written)
        print(f"when: mask -> {self.rendered}, unparsed {self.unparsed}")


@pytest.fixture
def world() -> World:
    return World()


@given("masking is switched off")
def masking_is_switched_off(world: World) -> None:
    world.unmasked = True


@when(parsers.re(r"the payload is an? (?P<name>Person|Team|Login|Account|Upload):"))
def the_payload_is_a_type(world: World, name: str, docstring: str) -> None:
    world.render(fixture(name, without_comments(docstring)))


@when("the payload is the body:")
def the_payload_is_the_body_doc(world: World, docstring: str) -> None:
    world.render(without_comments(docstring).strip().encode())


@when("the payload is a map:")
def the_payload_is_a_map(world: World, docstring: str) -> None:
    world.render(json.loads(without_comments(docstring)))


@when("the payload is a map of lists:")
def the_payload_is_a_map_of_lists(world: World, docstring: str) -> None:
    world.render(json.loads(without_comments(docstring)))


@when(parsers.re(r"the payload is a map with \"(?P<key>[^\"]*)\" holding the bodies '(?P<b1>[^']*)', '(?P<b2>[^']*)', and '(?P<b3>[^']*)'"))
def the_payload_holds_bodies(world: World, key: str, b1: str, b2: str, b3: str) -> None:
    world.render({key: [b1.encode(), b2.encode(), b3.encode()]})


@when(parsers.re(r'the payload is a map with "(?P<k1>[^"]*)" = "(?P<v1>[^"]*)" and "(?P<k2>[^"]*)" = "(?P<v2>[^"]*)"'))
def the_payload_is_a_map_of_two(world: World, k1: str, v1: str, k2: str, v2: str) -> None:
    world.render({k1: v1, k2: v2})


@when(parsers.re(r"the payload is the body '(?P<body>[^']*)'$"))
def the_payload_is_the_body(world: World, body: str) -> None:
    world.render(body.encode())


@when(parsers.re(r"the payload is a map with \"(?P<key>[^\"]*)\" holding the body '(?P<body>[^']*)'$"))
def the_payload_holds_a_body(world: World, key: str, body: str) -> None:
    world.render({key: body.encode()})


@when(parsers.re(r"the payload is a (?P<name>\w+) message given as '(?P<given>[^']*)'"))
def the_payload_is_a_message_given_as(world: World, name: str, given: str) -> None:
    world.render(parsed_message(name, given))


@when(parsers.re(r"the payload is a (?P<name>\w+) message:"))
def the_payload_is_a_message(world: World, name: str, docstring: str) -> None:
    world.render(parsed_message(name, without_comments(docstring)))


@when(parsers.re(r'the payload is a Wallet message whose extra is an Any of the type "(?P<url>[^"]*)" holding (?P<size>\d+) bytes'))
def the_payload_holds_an_unknown_any(world: World, url: str, size: str) -> None:
    wallet = proto_message("Wallet")
    getattr(wallet, "extra").CopyFrom(AnyMessage(type_url=url, value=b"x" * int(size)))
    world.render(wallet)


@when(parsers.re(r"the payload is (?P<shape>maps|lists) nested (?P<levels>\d+) levels deep"))
def the_payload_is_nested(world: World, shape: str, levels: str) -> None:
    world.render(nested(shape, int(levels)))


@when(parsers.re(r"the payload is a JSON body of (?P<shape>objects|lists) nested (?P<levels>\d+) levels deep"))
def the_payload_is_a_nested_body(world: World, shape: str, levels: str) -> None:
    opening, closing = ('{"next":', "}") if shape == "objects" else ("[", "]")
    world.render((opening * int(levels) + '"end"' + closing * int(levels)).encode())


@when(parsers.re(r"the payload is Node messages (?:linked by|each in) (?P<field>next|children|named) nested (?P<levels>\d+) levels deep"))
def the_payload_is_nested_nodes(world: World, field: str, levels: str) -> None:
    world.render(parsed_message("Node", nested_nodes(field, int(levels))))


@when(parsers.parse('the payload is a link "{name}" whose next is itself'))
def the_payload_refers_to_itself(world: World, name: str) -> None:
    link = Link(name=name)
    link.next = link
    world.render(link)


@then("the payload should be written as:")
def the_payload_should_be_written_as(world: World, docstring: str) -> None:
    assert same_json(world.rendered, docstring), world.rendered


@then("the masked payload should be:")
def the_masked_payload_should_be(world: World, docstring: str) -> None:
    assert same_json(world.rendered, docstring), world.rendered


@then(parsers.re(r"the masked payload should be '(?P<want>[^']*)'"))
def the_masked_payload_should_be_inline(world: World, want: str) -> None:
    assert same_json(world.rendered, want), world.rendered


@then(parsers.re(r'the masked payload should (?P<how>mention|not mention) "(?P<text>[^"]*)"'))
def the_masked_payload_should_mention(world: World, how: str, text: str) -> None:
    assert (text in world.rendered) == (how == "mention"), world.rendered


@then("the mask should report no unparsed body")
def the_mask_should_report_no_unparsed_body(world: World) -> None:
    assert world.unparsed == []


@then("the mask should report these unparsed bodies:")
def the_mask_should_report_these(world: World, datatable: list[list[str]]) -> None:
    want = sorted((Unparsed(path=row[0], bytes=int(row[1])) for row in datatable[1:]), key=lambda found: found.path)
    assert sorted(world.unparsed, key=lambda found: found.path) == want, world.unparsed  # a message is walked in no fixed order


@then(parsers.re(r'the mask should report the unparsed body "(?P<path>[^"]*)" of (?P<size>\d+) bytes'))
def the_mask_should_report_the_unparsed_body(world: World, path: str, size: str) -> None:
    assert world.unparsed == [Unparsed(path=path, bytes=int(size))]
