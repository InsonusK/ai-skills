// Source: skill project-version-in-go

// Package version holds the version of this build. The recorded value is
// raised by the release process; a snapshot build overrides it with
// -ldflags "-X github.com/example/todo/internal/version.Version=...".
package version

// Version is the version of the running binary.
var Version = "0.1.0"
