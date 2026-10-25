from dataclasses import dataclass

import pytest
from pytest_bdd import parsers, scenarios, then, when

from todo.api.core import rpcstatus
from todo.api.core.rpcstatus import Code
from todo.command.pipeline import Kind, KindError

scenarios("../features/rpcstatus.feature")


@dataclass
class World:
    code: Code = Code.OK
    message: str = ""


@pytest.fixture
def world() -> World:
    return World()


def _answer(world: World, error: BaseException) -> None:
    world.code, world.message = rpcstatus.of(error)
    print(f"when: of({error!r}) -> {world.code.name}, {world.message!r}")


@when(parsers.parse('a command fails with the kind "{kind}" and the message "{message}"'))
def a_command_fails_with_the_kind(world: World, kind: str, message: str) -> None:
    _answer(world, KindError(Kind(kind), message))


@when(parsers.parse('a command fails with the kind "{kind}" and the message "{message}", wrapped in "{outer}"'))
def a_command_fails_wrapped(world: World, kind: str, message: str, outer: str) -> None:
    try:
        try:
            raise KindError(Kind(kind), message)
        except KindError as error:
            raise RuntimeError(outer) from error
    except RuntimeError as wrapped:
        _answer(world, wrapped)


@when(parsers.parse('a command fails with the plain error "{message}"'))
def a_command_fails_with_a_plain_error(world: World, message: str) -> None:
    _answer(world, RuntimeError(message))


@then(parsers.parse("the rpc code should be {code:d}"))
def the_rpc_code_should_be(world: World, code: int) -> None:
    assert world.code == code


@then(parsers.parse("the rpc code's HTTP status should be {status:d}"))
def the_rpc_codes_http_status_should_be(world: World, status: int) -> None:
    assert rpcstatus.http_status(world.code) == status


@then(parsers.parse('the rpc message should be "{message}"'))
def the_rpc_message_should_be(world: World, message: str) -> None:
    assert world.message == message


@then(parsers.parse("the rpc code {code:d} should have the HTTP status {status:d}"))
def the_rpc_code_should_have_the_http_status(code: int, status: int) -> None:
    assert rpcstatus.http_status(Code(code)) == status
