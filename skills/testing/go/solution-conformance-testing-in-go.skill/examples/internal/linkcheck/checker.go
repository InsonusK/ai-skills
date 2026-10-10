// Package linkcheck validates and normalizes a URL before it is used further.
package linkcheck

import (
	"net/url"
	"strings"
)

// Result is the outcome of checking one URL. ErrorCode is empty when IsValid.
type Result struct {
	IsValid    bool   `json:"is_valid"`
	Normalized string `json:"normalized"`
	ErrorCode  string `json:"error_code"`
}

// Check accepts http and https URLs with a host and returns the URL with its
// scheme and host in lower case; the path is kept as it is.
func Check(rawURL string) Result {
	trimmed := strings.TrimSpace(rawURL)
	u, err := url.Parse(trimmed)
	if err != nil {
		return Result{ErrorCode: "UNSUPPORTED_SCHEME"}
	}
	scheme := strings.ToLower(u.Scheme)
	if scheme != "http" && scheme != "https" {
		return Result{ErrorCode: "UNSUPPORTED_SCHEME"}
	}
	if u.Host == "" {
		return Result{ErrorCode: "MISSING_HOST"}
	}
	if u.User != nil {
		return Result{ErrorCode: "CREDENTIALS_NOT_ALLOWED"}
	}
	return Result{IsValid: true, Normalized: scheme + "://" + strings.ToLower(u.Host) + u.Path}
}
