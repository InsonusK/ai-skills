// Source: skill api-develop-in-go
// Package {contract-package} embeds this service's own published OpenAPI contract so it
// can be served (as JSON, and via Swagger UI) without reading from disk at runtime.
package {contract-package}

import _ "embed"

//go:embed {file}.swagger.json
var Spec []byte
