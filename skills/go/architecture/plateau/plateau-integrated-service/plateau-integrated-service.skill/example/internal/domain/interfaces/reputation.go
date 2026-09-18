// Package interfaces declares the domain's outbound ports: what the
// business logic needs from the outside world, without knowing which
// infrastructure implements it.
package interfaces

import (
	"context"
	"errors"
)

// ErrUnavailable is returned when the reputation service cannot be reached.
var ErrUnavailable = errors.New("interfaces: reputation service unavailable")

// Reputation is the external service's verdict on one URL.
type Reputation struct {
	Flagged bool
	Reason  string
}

// ReputationChecker is the outbound port to the external reputation service.
type ReputationChecker interface {
	CheckReputation(ctx context.Context, url string) (Reputation, error)
}
