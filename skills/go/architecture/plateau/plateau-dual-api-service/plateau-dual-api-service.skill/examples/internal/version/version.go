// Package version holds the build-time version string.
package version

// Version is overridden at build time via:
//
//	-ldflags "-X github.com/example/linkcheck-service/internal/version.Version=$(VERSION)"
var Version = "dev"
