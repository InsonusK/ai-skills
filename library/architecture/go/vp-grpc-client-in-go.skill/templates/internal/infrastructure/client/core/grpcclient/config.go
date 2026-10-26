// Source: skill vp-grpc-client-in-go

// Package grpcclient is the fixed part of every client of another service's
// gRPC API, copied from templates and never edited: the {API} settings
// (config.go), and New, which builds the connection whose every unary call is
// bounded by the timeout, carries the request id, is logged, and fails with a
// technical outcome - or, mocked, is answered by the API's emulator inside the
// process (client.go). An adapter of one API (internal/infrastructure/client/{api})
// calls its methods through the generated client over this connection.
package grpcclient

import (
	"fmt"
	"net"
	"os"
	"regexp"
	"strconv"
	"strings"
	"time"
)

// Mock is the value of {API}_HOST that runs the client against the emulator
// of its API inside the process.
const Mock = "mock"

// DefaultTimeout bounds a call when {API}_TIMEOUT is not set.
const DefaultTimeout = 5 * time.Second

// Config holds the settings of one API: the {API} group of skill api-client.
type Config struct {
	// Name is how the log names the API: the prefix in lower case with "-",
	// "TODO_API" is "todo-api".
	Name string
	// Host is {API}_HOST; Mock runs the client against its emulator.
	Host string
	// Port is {API}_GRPC_PORT; default 443 with TLS, 80 without.
	Port int
	// TLS is {API}_TLS: connect over TLS; default false.
	TLS bool
	// Timeout is {API}_TIMEOUT: the longest one call may take; default 5s.
	Timeout time.Duration
}

// Mocked reports whether the client runs against its emulator.
func (c Config) Mocked() bool { return c.Host == Mock }

// Target is host:port, the address the connection dials.
func (c Config) Target() string {
	return net.JoinHostPort(c.Host, strconv.Itoa(c.Port))
}

// timeout is a number and ms, s, or m - the form every stack reads.
var timeout = regexp.MustCompile(`^[0-9]+(\.[0-9]+)?(ms|s|m)$`)

// LoadConfig reads the {API} group of prefix ("TODO_API") from the
// environment; an unusable or missing value is an error naming its variable.
func LoadConfig(prefix string) (Config, error) {
	cfg := Config{Name: strings.ToLower(strings.ReplaceAll(prefix, "_", "-")), Timeout: DefaultTimeout}
	cfg.Host = os.Getenv(prefix + "_HOST")
	if cfg.Host == "" {
		return Config{}, fmt.Errorf("%s config: %s_HOST is not set", cfg.Name, prefix)
	}
	if raw := os.Getenv(prefix + "_TLS"); raw != "" {
		tls, err := strconv.ParseBool(raw)
		if err != nil {
			return Config{}, fmt.Errorf("%s config: %s_TLS %q is not a bool", cfg.Name, prefix, raw)
		}
		cfg.TLS = tls
	}
	cfg.Port = 80
	if cfg.TLS {
		cfg.Port = 443
	}
	if raw := os.Getenv(prefix + "_GRPC_PORT"); raw != "" {
		port, err := strconv.ParseUint(raw, 10, 16)
		if err != nil || port == 0 {
			return Config{}, fmt.Errorf("%s config: %s_GRPC_PORT %q is not a port", cfg.Name, prefix, raw)
		}
		cfg.Port = int(port)
	}
	if raw := os.Getenv(prefix + "_TIMEOUT"); raw != "" {
		d, err := time.ParseDuration(raw)
		if !timeout.MatchString(raw) || err != nil || d <= 0 {
			return Config{}, fmt.Errorf("%s config: %s_TIMEOUT %q is not a duration like 5s or 500ms", cfg.Name, prefix, raw)
		}
		cfg.Timeout = d
	}
	return cfg, nil
}
