// Source: skill error-kinds-in-go

package pipeline

import (
	"errors"

	"github.com/example/todo/internal/domain/entity"
)

// The error kinds of the application, re-exported from the entity package so
// entry points use them without importing the domain. An entry point answers
// an error by its kind - errors.Is(err, pipeline.ErrNotFound) - never by the
// individual error or its text. What each kind means and how every transport
// answers it is the table of skill error-kinds.
var (
	ErrInvalid            = entity.ErrInvalid
	ErrNotFound           = entity.ErrNotFound
	ErrConflict           = entity.ErrConflict
	ErrConcurrentChange   = entity.ErrConcurrentChange
	ErrFailedPrecondition = entity.ErrFailedPrecondition
	ErrUnauthenticated    = entity.ErrUnauthenticated
	ErrPermissionDenied   = entity.ErrPermissionDenied
	ErrResourceExhausted  = entity.ErrResourceExhausted
	ErrUnimplemented      = entity.ErrUnimplemented
	ErrUnavailable        = entity.ErrUnavailable
)

// Kinds lists every error kind, in the order KindOf checks them.
var Kinds = []error{
	ErrInvalid, ErrNotFound, ErrConflict, ErrConcurrentChange, ErrFailedPrecondition,
	ErrUnauthenticated, ErrPermissionDenied, ErrResourceExhausted, ErrUnimplemented, ErrUnavailable,
}

// KindOf returns the kind err wraps, or nil when it wraps none - a failure of
// the service, which a client is answered without its message.
func KindOf(err error) error {
	for _, kind := range Kinds {
		if errors.Is(err, kind) {
			return kind
		}
	}
	return nil
}
