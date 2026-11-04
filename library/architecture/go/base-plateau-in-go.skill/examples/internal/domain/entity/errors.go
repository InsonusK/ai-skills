// Package entity holds the entities of the domain: each guards its own state
// through its methods. It performs no I/O and imports no other package of the
// application but the shared error, so the rules here are tested by their
// scenarios alone.
package entity

import (
	"fmt"

	"github.com/example/todo/internal/shared/errorkind"
)

// The errors of the domain's rules, each of its kind (skill error-kinds).
var (
	ErrEmptyTitle     = errorkind.New(errorkind.ErrInvalid, "item title is empty")
	ErrTitleTooLong   = errorkind.New(errorkind.ErrInvalid, fmt.Sprintf("item title is longer than %d characters", MaxTitleLength))
	ErrAlreadyDone    = errorkind.New(errorkind.ErrFailedPrecondition, "item is already done")
	ErrNoItems        = errorkind.New(errorkind.ErrInvalid, "no items to plan")
	ErrDuplicateTitle = errorkind.New(errorkind.ErrInvalid, "item titles repeat")
)
