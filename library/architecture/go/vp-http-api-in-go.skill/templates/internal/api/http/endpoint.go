// Source: skill vp-http-api-in-go

package http

import (
	"encoding/json"
	"errors"
	"net/http"

	"{module-path}/internal/command"
)

// Endpoint turns one command handler into an HTTP handler. It is written once
// for every command, the same way startup.Run was:
//
//  1. decode builds the command (TCommand) from the request; if it cannot,
//     the answer is 400 INVALID_ARGUMENT and the handler is not called;
//  2. the handler runs the command;
//  3. an error is answered by its kind (see writeError);
//  4. encode turns the result (TResult) into the response body, written as
//     JSON with successStatus (201 created, 202 accepted, 200 otherwise).
//
// decode and encode belong to the route (routes.go): the request and response
// shapes are the HTTP contract, owned by this package, never the command's types.
func Endpoint[TCommand, TResult any](
	handler command.Handler[TCommand, TResult],
	decode func(r *http.Request) (TCommand, error),
	encode func(result TResult) any,
	successStatus int,
) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		cmd, err := decode(r)
		if err != nil {
			writeStatus(w, codeInvalidArgument, err.Error())
			return
		}
		result, err := handler.Handle(r.Context(), cmd)
		if err != nil {
			writeError(w, err)
			return
		}
		writeJSON(w, successStatus, encode(result))
	})
}

// DecodeJSON reads the request body as JSON into a value of type TBody.
// Unknown fields are ignored, so a client built against a newer minor version
// of the contract still works.
func DecodeJSON[TBody any](r *http.Request) (TBody, error) {
	var body TBody
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		return body, errors.New("request body is not valid JSON: " + err.Error())
	}
	return body, nil
}

// The grpc status codes this entry point answers with. The numbers are those
// of google.rpc.Code, so an error body reads the same as in a service with a
// grpc contract.
const (
	codeInvalidArgument = 3
	codeInternal        = 13
)

// httpStatusOf is the canonical gRPC-to-HTTP mapping for the codes above.
var httpStatusOf = map[int]int{
	codeInvalidArgument: http.StatusBadRequest,
	codeInternal:        http.StatusInternalServerError,
}

// statusBody is google.rpc.Status in proto-JSON, the body of every error.
type statusBody struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
	Details []any  `json:"details"`
}

// writeError answers an error by its kind: a command rejected by a business
// rule (command.ErrInvalid) is 400 INVALID_ARGUMENT with the rule's message;
// any other error is 500 INTERNAL, without the internal message.
func writeError(w http.ResponseWriter, err error) {
	if errors.Is(err, command.ErrInvalid) {
		writeStatus(w, codeInvalidArgument, err.Error())
		return
	}
	writeStatus(w, codeInternal, "internal error")
}

func writeStatus(w http.ResponseWriter, code int, message string) {
	writeJSON(w, httpStatusOf[code], statusBody{Code: code, Message: message, Details: []any{}})
}

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}
