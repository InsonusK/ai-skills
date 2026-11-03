// Source: skill vp-grpc-api-in-go

// Package pipeline is the fixed part of the gRPC entry point, copied from
// templates and never edited: settings, the server, the generic Call that
// turns a request message into a command and its result into the response,
// and one file per interceptor around every call (logging.go, recover.go).
// Like every entry point it knows only the command layer - never domain,
// service, or infrastructure. Other packages import it as grpcpipeline;
// inside it, the command layer's pipeline is commandpipeline.
package pipeline

import (
	"fmt"
	"os"
	"strconv"
)

// Config holds the gRPC entry point's own settings. They are read here, not in
// internal/config, so they come and go together with this package.
type Config struct {
	// ListenPort is GRPC_LISTEN_PORT, the port the server listens on; default 9090.
	ListenPort string
	// ReflectionEnabled is GRPC_REFLECTION_ENABLED: serve gRPC server
	// reflection, so grpcurl or Postman can list and call the methods without
	// the proto file - the gRPC counterpart of Swagger UI. Off by default.
	ReflectionEnabled bool
}

// LoadConfig reads the settings from the environment.
func LoadConfig() (Config, error) {
	cfg := Config{ListenPort: "9090"}
	if port := os.Getenv("GRPC_LISTEN_PORT"); port != "" {
		if _, err := strconv.ParseUint(port, 10, 16); err != nil {
			return Config{}, fmt.Errorf("grpc config: GRPC_LISTEN_PORT %q is not a port", port)
		}
		cfg.ListenPort = port
	}
	if raw := os.Getenv("GRPC_REFLECTION_ENABLED"); raw != "" {
		enabled, err := strconv.ParseBool(raw)
		if err != nil {
			return Config{}, fmt.Errorf("grpc config: GRPC_REFLECTION_ENABLED %q is not a bool", raw)
		}
		cfg.ReflectionEnabled = enabled
	}
	return cfg, nil
}
