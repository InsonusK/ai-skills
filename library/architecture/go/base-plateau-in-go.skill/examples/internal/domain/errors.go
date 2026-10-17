// Package domain holds the entities and domain services: the business rules,
// with no knowledge of how an operation is triggered or where data is kept.
package domain

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
