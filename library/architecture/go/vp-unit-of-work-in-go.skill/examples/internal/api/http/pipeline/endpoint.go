// Source: skill vp-http-api-in-go

package pipeline

import (
	"encoding/json"
	"errors"
	"net/http"

	"github.com/example/todo/internal/api/core/rpcstatus"
	"github.com/example/todo/internal/api/http/middleware/answerfailures"
	commandpipeline "github.com/example/todo/internal/command/pipeline"
)

// Endpoint turns one command handler into an HTTP handler. It is written once
// for every command, the same way startup.Run was:
//
//  1. decode builds the command (TCommand) from the request; if it cannot,
//     the answer is 400 INVALID_ARGUMENT and the handler is not called;
//  2. the handler runs the command;
//  3. an error is answered by its kind (see answerfailures.WriteError);
//  4. encode turns the result (TResult) into the response body, written as
//     JSON with successStatus (201 created, 202 accepted, 200 otherwise).
//
// decode and encode belong to the route (its resource file in the endpoint
// package, e.g. endpoint/items.go): the request and response
// shapes are the HTTP contract, owned by this package, never the command's types.
func Endpoint[TCommand, TResult any](
	handler commandpipeline.Handler[TCommand, TResult],
	decode func(r *http.Request) (TCommand, error),
	encode func(result TResult) any,
	successStatus int,
) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		cmd, err := decode(r)
		if err != nil {
			answerfailures.WriteStatus(w, rpcstatus.InvalidArgument, err.Error())
			return
		}
		result, err := handler.Handle(r.Context(), cmd)
		if err != nil {
			answerfailures.WriteError(w, err)
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

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}
