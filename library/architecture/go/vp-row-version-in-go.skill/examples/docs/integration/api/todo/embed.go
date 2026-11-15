// Source: skill api-develop-in-go
// Package todo embeds this service's own published OpenAPI contract so it
// can be served (as JSON, and via Swagger UI) without reading from disk at runtime.
package todo

import _ "embed"

//go:embed todo.swagger.json
var Spec []byte
