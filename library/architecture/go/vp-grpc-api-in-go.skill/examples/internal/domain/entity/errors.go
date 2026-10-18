// Package entity holds the entities of the domain: each guards its own state
// through its methods. It performs no I/O and imports no other package of the
// application, so the rules here are tested by their scenarios alone.
package entity

import "fmt"

// The errors of the domain's rules, each wrapping its kind (kinds.go).
var (
	ErrEmptyTitle     = fmt.Errorf("%w: item title is empty", ErrInvalid)
	ErrTitleTooLong   = fmt.Errorf("%w: item title is longer than %d characters", ErrInvalid, MaxTitleLength)
	ErrAlreadyDone    = fmt.Errorf("%w: item is already done", ErrFailedPrecondition)
	ErrNoItems        = fmt.Errorf("%w: no items to plan", ErrInvalid)
	ErrDuplicateTitle = fmt.Errorf("%w: item titles repeat", ErrInvalid)
)
