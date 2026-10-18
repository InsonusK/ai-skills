// Source: skill vp-health-in-go

// Package management is the service's management entry point: a small HTTP
// server on a port of its own that answers the orchestrator's probes -
// /health/live, /health/ready, /health - whatever the service's other entry
// points are (HTTP, gRPC, a listener, none). Kept off the API port, it is
// never published through the ingress. It calls no command.
package management

import (
	"fmt"
	"os"
	"strconv"
)

// Config holds the management entry point's own settings. They are read here,
// not in internal/config, so they come and go together with this package.
type Config struct {
	// ListenPort is MANAGEMENT_LISTEN_PORT, the port the probes call; default 8081.
	ListenPort string
}

// LoadConfig reads the settings from the environment.
func LoadConfig() (Config, error) {
	cfg := Config{ListenPort: "8081"}
	if port := os.Getenv("MANAGEMENT_LISTEN_PORT"); port != "" {
		if _, err := strconv.ParseUint(port, 10, 16); err != nil {
			return Config{}, fmt.Errorf("management config: MANAGEMENT_LISTEN_PORT %q is not a port", port)
		}
		cfg.ListenPort = port
	}
	return cfg, nil
}
