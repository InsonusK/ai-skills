// Source: skills vp-http-api-in-go, vp-grpc-api-in-go - one file, the same in both

// Package health holds the readiness checks every long-running entry point
// answers its probes with. A service variation point (a database, a client)
// adds its check once in the composition root, and every entry point - HTTP,
// gRPC - runs the same list.
package health

import (
	"context"
	"log/slog"
)

// Check is one dependency the service waits for before it takes traffic.
type Check struct {
	// Name is how the failed check is reported, e.g. "database".
	Name string
	// Check returns nil while the dependency can be used.
	Check func(ctx context.Context) error
}

// Failed runs every check and returns the names of those that failed, logging
// each failure; an empty result means the service is ready.
func Failed(ctx context.Context, checks []Check, logger *slog.Logger) []string {
	failed := []string{}
	for _, check := range checks {
		if err := check.Check(ctx); err != nil {
			logger.WarnContext(ctx, "readiness check failed", "check", check.Name, "error", err)
			failed = append(failed, check.Name)
		}
	}
	return failed
}
