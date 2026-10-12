package http

import (
	"io"
	"net/http"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
	"google.golang.org/protobuf/encoding/protojson"
	"google.golang.org/protobuf/proto"
)

// Standard proto-JSON: a request is read with either form of a field name and
// its unknown fields are ignored; a response names every field in lowerCamelCase,
// also one that holds its default value.
var (
	unmarshal = protojson.UnmarshalOptions{DiscardUnknown: true}
	marshal   = protojson.MarshalOptions{EmitUnpopulated: true}
)

// readMessage decodes the request body into the contract's message.
func readMessage(r *http.Request, message proto.Message) error {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		return err
	}
	return unmarshal.Unmarshal(body, message)
}

// writeMessage encodes the contract's message as the response body.
func writeMessage(w http.ResponseWriter, httpStatus int, message proto.Message) {
	body, err := marshal.Marshal(message)
	if err != nil {
		writeError(w, status.New(codes.Internal, err.Error()))
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(httpStatus)
	_, _ = w.Write(body)
}

// httpStatuses is the canonical gRPC-to-HTTP mapping.
var httpStatuses = map[codes.Code]int{
	codes.InvalidArgument:    http.StatusBadRequest,
	codes.FailedPrecondition: http.StatusBadRequest,
	codes.OutOfRange:         http.StatusBadRequest,
	codes.Unauthenticated:    http.StatusUnauthorized,
	codes.PermissionDenied:   http.StatusForbidden,
	codes.NotFound:           http.StatusNotFound,
	codes.AlreadyExists:      http.StatusConflict,
	codes.Aborted:            http.StatusConflict,
	codes.ResourceExhausted:  http.StatusTooManyRequests,
	codes.Canceled:           499,
	codes.Internal:           http.StatusInternalServerError,
	codes.Unknown:            http.StatusInternalServerError,
	codes.DataLoss:           http.StatusInternalServerError,
	codes.Unimplemented:      http.StatusNotImplemented,
	codes.Unavailable:        http.StatusServiceUnavailable,
	codes.DeadlineExceeded:   http.StatusGatewayTimeout,
}

// writeError answers with google.rpc.Status as the body and the HTTP status its
// code maps to.
func writeError(w http.ResponseWriter, st *status.Status) {
	httpStatus, ok := httpStatuses[st.Code()]
	if !ok {
		httpStatus = http.StatusInternalServerError
	}
	body, err := marshal.Marshal(st.Proto())
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(httpStatus)
	_, _ = w.Write(body)
}
