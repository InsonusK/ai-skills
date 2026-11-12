// Source: skill vp-logging-in-go

// Package masked is the logging variation point: a slog.Handler that writes
// what the base handler drops - the payload of every call - masked, cut to a
// size, and with the request id of the call. It replaces logging.NewHandler
// in one line of main.go; no part that logs changes.
package masked

import (
	"fmt"
	"log/slog"
	"os"
	"strconv"
)

// The values of LOG_PAYLOAD.
const (
	PayloadAlways = "always" // every record keeps its payload
	PayloadErrors = "errors" // only a record with failed=true keeps it
	PayloadOff    = "off"    // no record keeps it, as with the base handler
)

// Config holds the logging variation point's own settings. They are read here,
// not in internal/config, so they come and go together with this package.
type Config struct {
	// Payload is LOG_PAYLOAD: always (default), errors, or off.
	Payload string
	// MaxBytes is LOG_PAYLOAD_MAX_BYTES, the size a payload is cut to once
	// written as JSON; default 4096.
	MaxBytes int
	// Unmasked is LOG_PAYLOAD_UNMASKED: write every payload as it is, for
	// debugging only; allowed only with LOG_LEVEL=debug, warned at start and
	// once per request. Default false.
	Unmasked bool
}

// LoadConfig reads the settings from the environment. level is the
// application's LOG_LEVEL: masking is switched off only at debug level, so
// one variable forgotten in a deployment cannot write every secret.
func LoadConfig(level slog.Level) (Config, error) {
	cfg := Config{Payload: PayloadAlways, MaxBytes: 4096}
	if raw := os.Getenv("LOG_PAYLOAD"); raw != "" {
		if raw != PayloadAlways && raw != PayloadErrors && raw != PayloadOff {
			return Config{}, fmt.Errorf("logging config: LOG_PAYLOAD %q is not always, errors, or off", raw)
		}
		cfg.Payload = raw
	}
	if raw := os.Getenv("LOG_PAYLOAD_MAX_BYTES"); raw != "" {
		n, err := strconv.Atoi(raw)
		if err != nil || n < 64 {
			return Config{}, fmt.Errorf("logging config: LOG_PAYLOAD_MAX_BYTES %q is not a number of 64 or more", raw)
		}
		cfg.MaxBytes = n
	}
	if raw := os.Getenv("LOG_PAYLOAD_UNMASKED"); raw != "" {
		unmasked, err := strconv.ParseBool(raw)
		if err != nil {
			return Config{}, fmt.Errorf("logging config: LOG_PAYLOAD_UNMASKED %q is not true or false", raw)
		}
		if unmasked && level > slog.LevelDebug {
			return Config{}, fmt.Errorf("logging config: LOG_PAYLOAD_UNMASKED is true, which needs LOG_LEVEL=debug, not %s", level)
		}
		cfg.Unmasked = unmasked
	}
	return cfg, nil
}
