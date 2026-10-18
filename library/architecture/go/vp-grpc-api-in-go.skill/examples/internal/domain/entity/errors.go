// Package entity holds the entities of the domain: each guards its own state
// through its methods. It performs no I/O and imports no other package of the
// application, so the rules here are tested by their scenarios alone.
package entity

import (
	"errors"
	"fmt"
)

// ErrInvalid is wrapped by every business-rule violation, so a caller tells a
// rejected request from a failure without knowing each rule.
var ErrInvalid = errors.New("invalid")

var (
	ErrEmptyTitle     = fmt.Errorf("%w: item title is empty", ErrInvalid)
	ErrTitleTooLong   = fmt.Errorf("%w: item title is longer than %d characters", ErrInvalid, MaxTitleLength)
	ErrAlreadyDone    = fmt.Errorf("%w: item is already done", ErrInvalid)
	ErrNoItems        = fmt.Errorf("%w: no items to plan", ErrInvalid)
	ErrDuplicateTitle = fmt.Errorf("%w: item titles repeat", ErrInvalid)
)
