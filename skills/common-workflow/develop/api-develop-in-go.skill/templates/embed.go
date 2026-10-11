// Source: skill api-develop-in-go
// Package openapi embeds this service's own published OpenAPI contract so it
// can be served (as JSON, and via Swagger UI) without reading from disk at runtime.
package openapi

import _ "embed"

//go:embed {service}.swagger.json
var Spec []byte
