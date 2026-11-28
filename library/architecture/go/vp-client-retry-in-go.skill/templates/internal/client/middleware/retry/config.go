// Source: skill vp-client-retry-in-go

// Package retry is the client middleware that sends a call again when the API
// did not answer: only a call of an operation its adapter marked Retryable,
// only on outcome.ErrUnavailable, at most {API}_RETRY_ATTEMPTS times in all,
// waiting {API}_RETRY_BACKOFF and twice as long before each next attempt.
// Copied from templates, never edited; the composition root adds it to the
// client middleware of the API.
package retry

import (
	"fmt"
	"os"
	"regexp"
	"strconv"
	"strings"
	"time"
)

// DefaultAttempts is how many attempts a call gets in all when {API}_RETRY_ATTEMPTS is not set.
const DefaultAttempts = 3

// DefaultBackoff is the wait before the second attempt when {API}_RETRY_BACKOFF is not set.
const DefaultBackoff = 100 * time.Millisecond

// Config holds the retry settings of one API, beside the {API} group of skill api-client.
type Config struct {
	// Attempts is {API}_RETRY_ATTEMPTS: the attempts a call gets in all, the
	// first included; 1 sends no repeat. Default 3.
	Attempts int
	// Backoff is {API}_RETRY_BACKOFF: the wait before the second attempt,
	// doubled before each next one, each wait between half and one and a
	// half of it. Default 100ms.
	Backoff time.Duration
}

// duration is a number and ms, s, or m - the form every stack reads.
var duration = regexp.MustCompile(`^[0-9]+(\.[0-9]+)?(ms|s|m)$`)

// LoadConfig reads {prefix}_RETRY_ATTEMPTS and {prefix}_RETRY_BACKOFF; an
// unusable value is an error naming its variable.
func LoadConfig(prefix string) (Config, error) {
	name := strings.ToLower(strings.ReplaceAll(prefix, "_", "-"))
	cfg := Config{Attempts: DefaultAttempts, Backoff: DefaultBackoff}
	if raw := os.Getenv(prefix + "_RETRY_ATTEMPTS"); raw != "" {
		attempts, err := strconv.Atoi(raw)
		if err != nil || attempts < 1 {
			return Config{}, fmt.Errorf("%s config: %s_RETRY_ATTEMPTS %q is not a count of 1 or more", name, prefix, raw)
		}
		cfg.Attempts = attempts
	}
	if raw := os.Getenv(prefix + "_RETRY_BACKOFF"); raw != "" {
		d, err := time.ParseDuration(raw)
		if !duration.MatchString(raw) || err != nil || d <= 0 {
			return Config{}, fmt.Errorf("%s config: %s_RETRY_BACKOFF %q is not a duration like 100ms or 1s", name, prefix, raw)
		}
		cfg.Backoff = d
	}
	return cfg, nil
}
