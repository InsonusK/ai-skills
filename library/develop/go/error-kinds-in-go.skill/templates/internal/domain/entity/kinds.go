// Source: skill error-kinds-in-go

package entity

import "errors"

// The error kinds of the application (skill error-kinds). Every error a
// command returns on purpose wraps exactly one of them with %w -
// fmt.Errorf("%w: item titles repeat", ErrInvalid) - so an entry point answers
// it by its kind (status code, exit code) without knowing the individual
// error. An error that wraps none is a failure of the service.
//
// They live here, in the lowest layer, because every layer may return one:
// an entity breaks a rule, a service does not find a record. Entry points use
// them through package pipeline and never import the domain.
var (
	// ErrInvalid: the input breaks a rule whatever the state (an empty title).
	ErrInvalid = errors.New("invalid")
	// ErrNotFound: the object the input names does not exist.
	ErrNotFound = errors.New("not found")
	// ErrConflict: the object to create already exists (the name is taken).
	ErrConflict = errors.New("conflict")
	// ErrConcurrentChange: the object was changed meanwhile; the whole
	// operation may be retried.
	ErrConcurrentChange = errors.New("concurrent change")
	// ErrFailedPrecondition: the input is valid, but the state does not allow
	// the operation (the item is already done).
	ErrFailedPrecondition = errors.New("failed precondition")
	// ErrUnauthenticated: the caller is not identified.
	ErrUnauthenticated = errors.New("unauthenticated")
	// ErrPermissionDenied: the caller is identified but may not do this.
	ErrPermissionDenied = errors.New("permission denied")
	// ErrResourceExhausted: a quota or rate limit of the caller is used up.
	ErrResourceExhausted = errors.New("resource exhausted")
	// ErrUnimplemented: the operation is not offered (deprecated, or not yet).
	ErrUnimplemented = errors.New("unimplemented")
	// ErrUnavailable: a dependency is down for now; a retry may succeed.
	ErrUnavailable = errors.New("unavailable")
)
