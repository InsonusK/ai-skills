// Package services holds the module's business logic, independent of the
// transport that triggers it or the infrastructure it may later rely on.
package services

import (
	"context"
	"errors"
	"net/url"
	"strings"
	"time"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
)

// ErrInvalidURL is returned when Check is given a URL that is not
// well-formed or not http(s).
var ErrInvalidURL = errors.New("services: invalid url")

// Result is the outcome of checking one URL.
type Result struct {
	URL        string
	Normalized string
	Flagged    bool
	Reason     string
}

// LinkCheckService validates, normalizes, and reputation-checks URLs.
type LinkCheckService struct {
	reputation interfaces.ReputationChecker
	cache      interfaces.ReputationCache
	history    interfaces.LinkHistory
}

func NewLinkCheckService(reputation interfaces.ReputationChecker, cache interfaces.ReputationCache, history interfaces.LinkHistory) *LinkCheckService {
	return &LinkCheckService{reputation: reputation, cache: cache, history: history}
}

// Check validates rawURL, normalizes it (lowercase scheme and host, path
// unchanged; only http and https schemes are accepted), then asks the
// external reputation service whether it is flagged - checking the cache
// first, and populating it after a real lookup.
func (s *LinkCheckService) Check(ctx context.Context, rawURL string) (Result, error) {
	trimmed := strings.TrimSpace(rawURL)
	u, err := url.Parse(trimmed)
	if err != nil || u.Host == "" {
		return Result{}, ErrInvalidURL
	}
	scheme := strings.ToLower(u.Scheme)
	if scheme != "http" && scheme != "https" {
		return Result{}, ErrInvalidURL
	}
	normalized := scheme + "://" + strings.ToLower(u.Host) + u.Path

	rep, err := s.reputationWithCache(ctx, normalized)
	if err != nil {
		return Result{}, err
	}

	checkedAt := time.Now().UTC()
	if err := s.history.Record(ctx, interfaces.LinkHistoryEntry{
		Normalized: normalized,
		Flagged:    rep.Flagged,
		Reason:     rep.Reason,
		CheckedAt:  checkedAt,
	}); err != nil {
		return Result{}, err
	}

	return Result{URL: trimmed, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
}

// RecentChecks returns the most recently recorded checks, most recent first.
func (s *LinkCheckService) RecentChecks(ctx context.Context, limit int) ([]interfaces.LinkHistoryEntry, error) {
	return s.history.Recent(ctx, limit)
}

// reputationWithCache checks the cache first; a cache-store failure (not a
// miss) degrades to computing the value normally, never fails the request.
func (s *LinkCheckService) reputationWithCache(ctx context.Context, normalized string) (interfaces.Reputation, error) {
	if cached, ok, err := s.cache.Get(ctx, normalized); err == nil && ok {
		return cached, nil
	}

	rep, err := s.reputation.CheckReputation(ctx, normalized)
	if err != nil {
		return interfaces.Reputation{}, err
	}

	_ = s.cache.Set(ctx, normalized, rep)
	return rep, nil
}
