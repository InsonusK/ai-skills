package interfaces

import "context"

// ReputationCache caches CheckReputation results by URL. A miss is
// signaled by ok=false, never an error.
type ReputationCache interface {
	Get(ctx context.Context, url string) (rep Reputation, ok bool, err error)
	Set(ctx context.Context, url string, rep Reputation) error
}
