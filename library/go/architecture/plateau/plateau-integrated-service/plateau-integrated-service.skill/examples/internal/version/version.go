// Package version holds the version of the program.
package version

// Version is the project's version, recorded here and nowhere else: `make version` reads
// this line. A snapshot build overrides it with
//
//	-ldflags "-X github.com/example/linkcheck-service/internal/version.Version={version}"
var Version = "0.1.0"
