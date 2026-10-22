// Source: skill error-kinds-in-go

// Package rpcstatus answers an error of a command by its kind, for every
// entry point at once: the google.rpc.Code a gRPC call returns, the same code
// in the google.rpc.Status body of an HTTP error, and the HTTP status. It is
// the table of skill error-kinds in code; a new kind is a row here and in
// pipeline.Kinds, never a change of an entry point.
//
// It imports no gRPC module, so an HTTP-only service does not pull one in.
package rpcstatus

import (
	"net/http"

	"github.com/example/todo/internal/command/pipeline"
)

// Code is a google.rpc.Code; the numbers are those of grpc-go's codes.Code,
// so codes.Code(code) converts it.
type Code int

const (
	OK                 Code = 0
	Cancelled          Code = 1
	Unknown            Code = 2
	InvalidArgument    Code = 3
	DeadlineExceeded   Code = 4
	NotFound           Code = 5
	AlreadyExists      Code = 6
	PermissionDenied   Code = 7
	ResourceExhausted  Code = 8
	FailedPrecondition Code = 9
	Aborted            Code = 10
	OutOfRange         Code = 11
	Unimplemented      Code = 12
	Internal           Code = 13
	Unavailable        Code = 14
	DataLoss           Code = 15
	Unauthenticated    Code = 16
)

// codeOfKind is the error-kinds table: the code each kind is answered with.
var codeOfKind = map[error]Code{
	pipeline.ErrInvalid:            InvalidArgument,
	pipeline.ErrNotFound:           NotFound,
	pipeline.ErrConflict:           AlreadyExists,
	pipeline.ErrConcurrentChange:   Aborted,
	pipeline.ErrFailedPrecondition: FailedPrecondition,
	pipeline.ErrUnauthenticated:    Unauthenticated,
	pipeline.ErrPermissionDenied:   PermissionDenied,
	pipeline.ErrResourceExhausted:  ResourceExhausted,
	pipeline.ErrUnimplemented:      Unimplemented,
	pipeline.ErrUnavailable:        Unavailable,
}

// InternalMessage is all a client learns of an error with no kind.
const InternalMessage = "internal error"

// Of returns the code and the message a client is answered with for an error
// a command returned: the kind's code with the error's message, or Internal
// with InternalMessage when the error has no kind - its own message may hold
// connection strings or stack details and goes only to the log.
func Of(err error) (Code, string) {
	if code, ok := codeOfKind[pipeline.KindOf(err)]; ok {
		return code, err.Error()
	}
	return Internal, InternalMessage
}

// HTTPStatus is the HTTP status of the code by the canonical gRPC-to-HTTP
// mapping (skill error-kinds).
func (c Code) HTTPStatus() int {
	switch c {
	case OK:
		return http.StatusOK
	case InvalidArgument, FailedPrecondition, OutOfRange:
		return http.StatusBadRequest
	case Unauthenticated:
		return http.StatusUnauthorized
	case PermissionDenied:
		return http.StatusForbidden
	case NotFound:
		return http.StatusNotFound
	case AlreadyExists, Aborted:
		return http.StatusConflict
	case ResourceExhausted:
		return http.StatusTooManyRequests
	case Cancelled:
		return 499 // Client Closed Request; net/http has no constant for it
	case Unimplemented:
		return http.StatusNotImplemented
	case Unavailable:
		return http.StatusServiceUnavailable
	case DeadlineExceeded:
		return http.StatusGatewayTimeout
	default: // Internal, Unknown, DataLoss
		return http.StatusInternalServerError
	}
}
