// Source: skill error-kinds-in-go

package pipeline

import "{module-path}/internal/shared/errorkind"

// The error kinds of the application, re-exported from package errorkind so
// entry points take everything of a command from the pipeline. An entry point
// answers an error by its kind - errors.Is(err, pipeline.ErrNotFound) - never
// by the individual error or its text. What each kind means and how every
// transport answers it is the table of skill error-kinds.
var (
	ErrInvalid            = errorkind.ErrInvalid
	ErrNotFound           = errorkind.ErrNotFound
	ErrConflict           = errorkind.ErrConflict
	ErrConcurrentChange   = errorkind.ErrConcurrentChange
	ErrFailedPrecondition = errorkind.ErrFailedPrecondition
	ErrUnauthenticated    = errorkind.ErrUnauthenticated
	ErrPermissionDenied   = errorkind.ErrPermissionDenied
	ErrResourceExhausted  = errorkind.ErrResourceExhausted
	ErrUnimplemented      = errorkind.ErrUnimplemented
	ErrUnavailable        = errorkind.ErrUnavailable
)

// Kinds lists every error kind, in the order KindOf checks them.
var Kinds = errorkind.Kinds

// KindOf returns the kind err carries, or nil when it carries none - a
// failure of the service, which a client is answered without its message.
func KindOf(err error) error { return errorkind.KindOf(err) }
