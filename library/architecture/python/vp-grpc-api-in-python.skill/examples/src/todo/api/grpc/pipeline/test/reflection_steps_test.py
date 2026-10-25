import asyncio
from typing import Protocol

import grpc
from grpc_reflection.v1alpha import reflection_pb2
from pytest_bdd import given, parsers, scenarios, then, when

from todo.api.grpc.pipeline import GrpcConfig

scenarios("../features/reflection.feature")


class World(Protocol):
    """What these steps use of conftest's World."""

    config: GrpcConfig
    loop: asyncio.AbstractEventLoop

    def serving(self) -> int: ...


class Listed:
    """What reflection answered: the services, or the error of the call."""

    services: list[str] | None = None
    error: grpc.aio.AioRpcError | None = None


async def _list_services(port: int) -> list[str]:
    async with grpc.aio.insecure_channel(f"127.0.0.1:{port}") as channel:
        # The method by name: the type stub of the generated client is incomplete.
        info = channel.stream_stream(
            "/grpc.reflection.v1alpha.ServerReflection/ServerReflectionInfo",
            request_serializer=reflection_pb2.ServerReflectionRequest.SerializeToString,
            response_deserializer=reflection_pb2.ServerReflectionResponse.FromString,
        )()
        await info.write(reflection_pb2.ServerReflectionRequest(list_services=""))
        await info.done_writing()
        answer = await info.read()
        assert isinstance(answer, reflection_pb2.ServerReflectionResponse), answer
        return [service.name for service in answer.list_services_response.service]


@given("reflection is enabled")
def reflection_is_enabled(world: World) -> None:
    world.config = GrpcConfig(listen_port=0, reflection_enabled=True)


@when("I list the services by reflection", target_fixture="listed")
def i_list_the_services(world: World) -> Listed:
    listed = Listed()
    try:
        listed.services = world.loop.run_until_complete(_list_services(world.serving()))
    except grpc.aio.AioRpcError as error:
        listed.error = error
    print(f"when: list services -> {listed.services}, error {listed.error and listed.error.code()}")
    return listed


@then(parsers.parse('the listed services should include "{service}"'))
def the_listed_services_should_include(listed: Listed, service: str) -> None:
    assert listed.services is not None and service in listed.services, listed.error


@then(parsers.parse("the reflection call should fail with {code}"))
def the_reflection_call_should_fail_with(listed: Listed, code: str) -> None:
    assert listed.error is not None and listed.error.code() is grpc.StatusCode[code], listed.services
