// Source: skill vp-http-api-in-go

// Package pipeline is the fixed part of the HTTP entry point, copied from
// templates and never edited: settings, the server, the generic Endpoint that
// turns a request into a command and its result into the response, the docs
// routes, and one file per wrapper around every request (logging.go). Like
// every entry point it knows only the command layer - never domain, service,
// or infrastructure. Other packages import it as httppipeline; inside it, the
// command layer's pipeline is commandpipeline.
package pipeline

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

// Config holds the HTTP entry point's own settings. They are read here, not in
// internal/config, so they come and go together with this package.
type Config struct {
	// ListenPort is HTTP_LISTEN_PORT, the port the server listens on; default 8080.
	ListenPort string
	// BasePath is HTTP_BASE_PATH, the path prefix the service is published
	// under: empty, or with a leading "/" and no trailing one. Routes stay
	// mounted at "/"; only URLs handed to a client carry it.
	BasePath string
	// DocsEnabled is HTTP_API_DOCS_ENABLED: serve the OpenAPI spec and Swagger UI.
	DocsEnabled bool
}

// LoadConfig reads the settings from the environment.
func LoadConfig() (Config, error) {
	cfg := Config{ListenPort: "8080"}
	if port := os.Getenv("HTTP_LISTEN_PORT"); port != "" {
		if _, err := strconv.ParseUint(port, 10, 16); err != nil {
			return Config{}, fmt.Errorf("http config: HTTP_LISTEN_PORT %q is not a port", port)
		}
		cfg.ListenPort = port
	}
	cfg.BasePath = normalizeBasePath(os.Getenv("HTTP_BASE_PATH"))
	if raw := os.Getenv("HTTP_API_DOCS_ENABLED"); raw != "" {
		enabled, err := strconv.ParseBool(raw)
		if err != nil {
			return Config{}, fmt.Errorf("http config: HTTP_API_DOCS_ENABLED %q is not a bool", raw)
		}
		cfg.DocsEnabled = enabled
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
