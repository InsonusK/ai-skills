// Package services holds the module's business logic, independent of the
// transport that triggers it or the infrastructure it may later rely on.
package services

import (
	"context"
	"errors"
	"net/url"
	"strings"

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
}

func NewLinkCheckService(reputation interfaces.ReputationChecker) *LinkCheckService {
	return &LinkCheckService{reputation: reputation}
}

// Check validates rawURL, normalizes it (lowercase scheme and host, path
// unchanged; only http and https schemes are accepted), then asks the
// external reputation service whether it is flagged.
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

	rep, err := s.reputation.CheckReputation(ctx, normalized)
	if err != nil {
		return Result{}, err
	}

	return Result{URL: trimmed, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
}
