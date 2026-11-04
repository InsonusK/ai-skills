// Source: skill vp-http-api-in-go

package pipeline

import (
	"encoding/json"
	"errors"
	"net/http"

	"google.golang.org/protobuf/encoding/protojson"
	"google.golang.org/protobuf/proto"

	"github.com/example/todo/internal/api/core/rpcstatus"
	"github.com/example/todo/internal/api/http/middleware/answerfailures"
)

// This file is copied only when the contract is a proto file: the routes then
// read and write the generated messages in the standard proto-JSON mapping
// (skill api-develop) instead of structs of their own.

// decodeOptions accept a field by its lowerCamelCase or its proto name and
// ignore an unknown field, so a client built against a newer minor version of
// the contract still works.
var decodeOptions = protojson.UnmarshalOptions{DiscardUnknown: true}

// DecodeProto reads the request body as proto-JSON into msg, the method's
// generated request message. Path parameters are not in the body: the route's
// decode sets them from r.PathValue after this call.
func DecodeProto(r *http.Request, msg proto.Message) error {
	var body json.RawMessage
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		return errors.New("request body is not valid JSON: " + err.Error())
	}
	if err := decodeOptions.Unmarshal(body, msg); err != nil {
		return errors.New("request body does not match the contract: " + err.Error())
	}
	return nil
}

// ProtoJSON is the response body of msg, the method's generated response
// message, in proto-JSON: lowerCamelCase keys, a field at its default value
// left out. An encode function of a route returns it.
func ProtoJSON(msg proto.Message) any {
	body, err := protojson.Marshal(msg)
	if err != nil {
		panic("a generated message does not encode: " + err.Error())
	}
	return json.RawMessage(body)
}

// Unimplemented is the handler of a method the contract keeps but no longer
// serves - deprecated and switched off: it answers 501 UNIMPLEMENTED with
// message, the way the gRPC form of the method answers.
func Unimplemented(message string) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		answerfailures.WriteStatus(w, rpcstatus.Unimplemented, message)
	})
}
