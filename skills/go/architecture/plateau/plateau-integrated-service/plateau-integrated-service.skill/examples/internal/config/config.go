// Package config loads runtime settings from environment variables.
package config

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

// Config holds all runtime settings, sourced from environment variables.
type Config struct {
	LogLevel       string
	HTTPListenPort string
	// HTTPAPIDocsEnabled registers the spec and Swagger UI routes; off unless set.
	HTTPAPIDocsEnabled bool
	// HTTPBasePath is the path prefix the service is published under: empty, or
	// with a leading "/" and no trailing one.
	HTTPBasePath   string
	GRPCListenPort string
	ReputationAddr string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		LogLevel:           l.get("LOG_LEVEL", "info"),
		HTTPListenPort:     l.get("HTTP_LISTEN_PORT", "8080"),
		HTTPAPIDocsEnabled: l.getBool("HTTP_API_DOCS_ENABLED", false),
		HTTPBasePath:       normalizeBasePath(l.get("HTTP_BASE_PATH", "")),
		GRPCListenPort:     l.get("GRPC_LISTEN_PORT", "50051"),
		ReputationAddr:     l.require("REPUTATION_ADDR"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}

// normalizeBasePath gives a path prefix its one form: "" for none, otherwise a
// leading "/" and no trailing one ("orders/" and "/orders" both become "/orders").
func normalizeBasePath(path string) string {
	path = strings.Trim(strings.TrimSpace(path), "/")
	if path == "" {
		return ""
	}
	return "/" + path
}

// loader reads env vars, stopping at the first error so Load's field list
// stays a flat struct literal instead of an if-err-chain per field.
type loader struct {
	err error
}

// get returns the env var's value, or def if unset. Every var also accepts
// a "<NAME>_FILE" form pointing at a file whose trimmed contents become the
// value (container-secret convention); if both are set, "<NAME>_FILE" wins.
func (l *loader) get(key, def string) string {
	if l.err != nil {
		return ""
	}
	if path := os.Getenv(key + "_FILE"); path != "" {
		data, err := os.ReadFile(path)
		if err != nil {
			l.err = fmt.Errorf("read %s_FILE (%s): %w", key, path, err)
			return ""
		}
		return strings.TrimSpace(string(data))
	}
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}

// getInt is like get, but parses the value as an integer.
func (l *loader) getInt(key string, def int) int {
	v := l.get(key, "")
	if l.err != nil || v == "" {
		return def
	}
	n, err := strconv.Atoi(v)
	if err != nil {
		l.err = fmt.Errorf("%s: %w", key, err)
		return def
	}
	return n
}

// getBool is like get, but parses the value as a boolean.
func (l *loader) getBool(key string, def bool) bool {
	v := l.get(key, "")
	if l.err != nil || v == "" {
		return def
	}
	b, err := strconv.ParseBool(v)
	if err != nil {
		l.err = fmt.Errorf("%s: %w", key, err)
		return def
	}
	return b
}

// require is like get, but records an error if the resulting value is empty.
func (l *loader) require(key string) string {
	v := l.get(key, "")
	if l.err == nil && v == "" {
		l.err = fmt.Errorf("%s is required", key)
	}
	return v
}
