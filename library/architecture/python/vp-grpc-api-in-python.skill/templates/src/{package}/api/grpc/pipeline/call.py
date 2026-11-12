# Source: skill vp-grpc-api-in-python

"""The one function every method of a service calls to run its command."""

from collections.abc import Callable

from google.protobuf.message import Message

from {package}.command.pipeline import Handler, Kind, KindError


async def call[TRequest: Message, TCommand, TResult, TResponse: Message](
    handler: Handler[TCommand, TResult],
    request: TRequest,
    decode: Callable[[TRequest], TCommand],
    encode: Callable[[TResult], TResponse],
) -> TResponse:
    """Runs one command for one gRPC method. It is written once for every
    method:

    1. decode builds the command (TCommand) from the request message
       (TRequest, e.g. PlanItemsRequest); a ValueError it raises - the
       request cannot become the command - is answered INVALID_ARGUMENT with
       its message, and the handler is not called;
    2. the handler runs the command;
    3. encode turns the result (TResult) into the response message
       (TResponse, e.g. PlanItemsResponse).

    An error of the handler propagates: AnswerFailuresMiddleware answers it by
    its kind. decode and encode belong to the method (its service module in
    endpoint/, e.g. endpoint/items.py): the messages are the gRPC contract,
    the command and its result are the command layer's, and neither leaks
    into the other.
    """
    try:
        command = decode(request)
    except ValueError as error:
        raise KindError(Kind.INVALID, str(error)) from error
    return encode(await handler.handle(command))
