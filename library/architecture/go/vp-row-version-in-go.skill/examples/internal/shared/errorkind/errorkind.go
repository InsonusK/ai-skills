// Source: skill error-kinds-in-go

// Package errorkind is the error a caller can act on, shared by every layer as
// dotnet's Shared project is: its kind, its message for the caller, and the
// technical cause behind it, which reaches only the log.
package errorkind

import "errors"

// The error kinds of the application (skill error-kinds). Every error a
// caller can act on is an *Error of exactly one of them, so an entry point
// answers it by its kind (status code, exit code) without knowing the
// individual error. An error that carries none is a failure of the service.
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

// Kinds lists every error kind, in the order KindOf checks them.
var Kinds = []error{
	ErrInvalid, ErrNotFound, ErrConflict, ErrConcurrentChange, ErrFailedPrecondition,
	ErrUnauthenticated, ErrPermissionDenied, ErrResourceExhausted, ErrUnimplemented, ErrUnavailable,
}

// Error is an error a caller can act on.
type Error struct {
	// Kind is one of the kinds above: it says what the caller can do.
	Kind error
	// Message says what happened, for the caller: an entry point answers it
	// as it is, so it holds nothing of the service's insides.
	Message string
	// Cause is the technical failure behind the error, or nil: a lost
	// connection, a driver's error. It goes to the log, never to the caller.
	Cause error
}

// New is an error of kind with message and no cause: a rule broken, a record
// missing.
func New(kind error, message string) *Error {
	return &Error{Kind: kind, Message: message}
}

// Wrap is an error of kind with message, cause behind it: a dependency that
// failed, told to the caller in the application's terms.
func Wrap(kind error, message string, cause error) *Error {
	return &Error{Kind: kind, Message: message, Cause: cause}
}

// Error is the message alone, so no print of the error shows its cause.
func (e *Error) Error() string { return e.Message }

// Unwrap gives errors.Is and errors.As the kind and the cause.
func (e *Error) Unwrap() []error {
	if e.Cause == nil {
		return []error{e.Kind}
	}
	return []error{e.Kind, e.Cause}
}

// KindOf returns the kind err carries, or nil when it carries none - a
// failure of the service, which a client is answered without its message.
// The outermost *Error decides: errors.Is would also see the kind of an
// *Error in its cause, which a service told the caller in other terms. A
// kind returned bare (return errorkind.ErrUnimplemented) carries it too.
func KindOf(err error) error {
	if kinded := Of(err); kinded != nil {
		return kinded.Kind
	}
	for _, kind := range Kinds {
		if errors.Is(err, kind) {
			return kind
		}
	}
	return nil
}

// Of returns the *Error err is or wraps, or nil for an error with no kind or
// a kind returned bare: an entry point answers its Message, the log writes
// its Cause.
func Of(err error) *Error {
	var kinded *Error
	if errors.As(err, &kinded) {
		return kinded
	}
	return nil
}
