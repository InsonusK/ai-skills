// Source: skill vp-http-client-in-go

// Package outcome holds the technical outcomes every client of another
// service's API reports, over HTTP or gRPC alike, so the service over an
// adapter checks the same errors whichever transport the adapter uses (skill
// api-client). Copied from templates, never edited; httpclient and
// grpcclient return them.
package outcome

import (
	"errors"
	"fmt"
)

// ErrUnavailable is a call the API did not answer: no connection, no answer
// within the timeout, an HTTP 502, 503, or 504, a gRPC UNAVAILABLE or
// DEADLINE_EXCEEDED. The service over the adapter turns it into the kind
// unavailable; a retry may help.
var ErrUnavailable = errors.New("api unavailable")

// Code is the code of a status an API answered: google.rpc.Code, which is
// also the gRPC status code.
type Code int

const (
	CodeOK                 Code = 0
	CodeCancelled          Code = 1
	CodeUnknown            Code = 2 // also an HTTP error answer whose body is no google.rpc.Status
	CodeInvalidArgument    Code = 3
	CodeDeadlineExceeded   Code = 4
	CodeNotFound           Code = 5
	CodeAlreadyExists      Code = 6
	CodePermissionDenied   Code = 7
	CodeResourceExhausted  Code = 8
	CodeFailedPrecondition Code = 9
	CodeAborted            Code = 10
	CodeOutOfRange         Code = 11
	CodeUnimplemented      Code = 12
	CodeInternal           Code = 13
	CodeUnavailable        Code = 14
	CodeDataLoss           Code = 15
	CodeUnauthenticated    Code = 16
)

var codeNames = [...]string{
	"OK", "CANCELLED", "UNKNOWN", "INVALID_ARGUMENT", "DEADLINE_EXCEEDED", "NOT_FOUND",
	"ALREADY_EXISTS", "PERMISSION_DENIED", "RESOURCE_EXHAUSTED", "FAILED_PRECONDITION",
	"ABORTED", "OUT_OF_RANGE", "UNIMPLEMENTED", "INTERNAL", "UNAVAILABLE", "DATA_LOSS",
	"UNAUTHENTICATED",
}

// String is the code's name in the contract: INVALID_ARGUMENT.
func (c Code) String() string {
	if c >= 0 && int(c) < len(codeNames) {
		return codeNames[c]
	}
	return fmt.Sprintf("CODE_%d", int(c))
}

// StatusError is any other failed call: the status the API answered. The
// service over the adapter turns the codes it expects into errors of its
// own; any other is a failure of the service.
type StatusError struct {
	Code    Code
	Message string
	// HTTPStatus is the HTTP status of the answer; 0 over gRPC.
	HTTPStatus int
}

func (e *StatusError) Error() string {
	if e.HTTPStatus != 0 {
		return fmt.Sprintf("api answered %d, %s: %s", e.HTTPStatus, e.Code, e.Message)
	}
	return fmt.Sprintf("api answered %s: %s", e.Code, e.Message)
}
