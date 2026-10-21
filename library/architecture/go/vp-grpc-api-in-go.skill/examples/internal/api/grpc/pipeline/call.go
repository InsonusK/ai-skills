// Source: skill vp-grpc-api-in-go

package pipeline

import (
	"context"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"github.com/example/todo/internal/api/rpcstatus"
	commandpipeline "github.com/example/todo/internal/command/pipeline"
)

// Call runs one command for one gRPC method. It is written once for every
// method, the same way http.Endpoint is:
//
//  1. decode builds the command (TCommand) from the request message
//     (TRequest, e.g. *todo.PlanItemsRequest); if it cannot, the answer is
//     INVALID_ARGUMENT and the handler is not called;
//  2. the handler runs the command;
//  3. an error is answered by its kind (see statusOf);
//  4. encode turns the result (TResult) into the response message
//     (TResponse, e.g. *todo.PlanItemsResponse).
//
// decode and encode belong to the method (its service file in the endpoint
// package, e.g. endpoint/items.go): the messages are the gRPC contract, the
// command and its result are the command layer's, and neither leaks into the
// other.
func Call[TRequest, TCommand, TResult, TResponse any](
	ctx context.Context,
	handler commandpipeline.Handler[TCommand, TResult],
	request TRequest,
	decode func(request TRequest) (TCommand, error),
	encode func(result TResult) TResponse,
) (TResponse, error) {
	var none TResponse
	cmd, err := decode(request)
	if err != nil {
		return none, status.Error(codes.InvalidArgument, err.Error())
	}
	result, err := handler.Handle(ctx, cmd)
	if err != nil {
		return none, statusOf(err)
	}
	return encode(result), nil
}

// statusOf answers an error by its kind through the one table of rpcstatus:
// a kind's code and message, or INTERNAL "internal error" for an error with
// no kind. The failure itself is logged by the handler's logging decorator.
func statusOf(err error) error {
	code, message := rpcstatus.Of(err)
	return status.Error(codes.Code(code), message)
}

// Unimplemented is the answer of a method the contract keeps but no longer
// serves - deprecated and switched off: UNIMPLEMENTED with message, the way
// the HTTP form of the method answers 501.
func Unimplemented(message string) error {
	return status.Error(codes.Unimplemented, message)
}
