// Package services holds the module's business logic, independent of the
// transport that triggers it or the infrastructure it may later rely on.
package services

import (
	"context"
	"errors"
	"net/url"
	"strings"
)

// ErrInvalidURL is returned when Check is given a URL that is not
// well-formed or not http(s).
var ErrInvalidURL = errors.New("services: invalid url")

// Result is the outcome of checking one URL.
type Result struct {
	URL        string
	Normalized string
}

// LinkCheckService validates and normalizes URLs.
type LinkCheckService struct{}

func NewLinkCheckService() *LinkCheckService {
	return &LinkCheckService{}
}

// Check validates rawURL and returns its normalized form (lowercase scheme
// and host, path unchanged). Only http and https schemes are accepted.
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
	return Result{URL: trimmed, Normalized: normalized}, nil
}
