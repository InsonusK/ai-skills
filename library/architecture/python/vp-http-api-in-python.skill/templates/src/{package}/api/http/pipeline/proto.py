# Source: skill vp-http-api-in-python

"""Copied only when the contract is a proto file: the routes then read and
write the contract's generated messages in the standard proto-JSON mapping
(skill api-develop) instead of pydantic models of their own."""

import json
from typing import NoReturn

from fastapi import Request, Response
from google.protobuf import json_format
from google.protobuf.message import Message

from {package}.command.pipeline import Kind, KindError


async def decode_proto[TMessage: Message](request: Request, message: TMessage) -> TMessage:
    """Reads the request body as proto-JSON into message, the method's
    generated request message, and returns it. A field is accepted by its
    lowerCamelCase or its proto name, and an unknown field is ignored, so a
    client built against a newer minor version of the contract still works. A
    body that is not JSON, or not the message, raises a KindError of kind
    invalid - 400 INVALID_ARGUMENT - before the command is called. Path
    parameters are not in the body: the route sets them from its own
    parameters."""
    body = await request.body()
    try:
        document = json.loads(body)
    except ValueError as error:
        raise KindError(Kind.INVALID, f"request body is not valid JSON: {error}") from error
    try:
        json_format.ParseDict(document, message, ignore_unknown_fields=True)
    except json_format.ParseError as error:
        raise KindError(Kind.INVALID, f"request body does not match the contract: {error}") from error
    return message


def proto_json(message: Message, status_code: int) -> Response:
    """The response of message, the method's generated response message, in
    proto-JSON - lowerCamelCase keys, a field at its default value left out -
    with the success status the rpc's openapiv2_operation names."""
    body = json.dumps(json_format.MessageToDict(message), ensure_ascii=False, separators=(",", ":"))
    return Response(body, status_code=status_code, media_type="application/json")


def unimplemented(message: str) -> NoReturn:
    """The answer of a method the contract keeps but no longer serves -
    deprecated and switched off: 501 UNIMPLEMENTED with message, the way the
    gRPC form of the method answers."""
    raise KindError(Kind.UNIMPLEMENTED, message)
