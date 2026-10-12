// Source: skill api-develop-in-go
// Package linkcheck embeds this service's own published OpenAPI contract so it
// can be served (as JSON, and via Swagger UI) without reading from disk at runtime.
package linkcheck

import _ "embed"

//go:embed linkcheck.swagger.json
var Spec []byte
