// Source: skill vp-http-client-in-go

// Package pipeline is the extension point of every client of another
// service's API, over HTTP or gRPC alike: the client middleware the
// composition root lists runs around the attempts of each call, so a
// variation point (the retry) adds a step to every call of every API without
// editing the client. Copied from templates, never edited.
package pipeline

import "context"

// Call is one call of an operation of an API, as every middleware sees it.
type Call struct {
	// API is how the log names the API: "todo-api".
	API string
	// Operation names the operation: "POST /v1/items/plan" over HTTP, the
	// full method name "/todo.v1.ItemsService/PlanItems" over gRPC.
	Operation string
	// Retryable is true when the adapter marked the operation safe to send
	// again: a repeat does what one call does (it is idempotent).
	Retryable bool
}

// Attempt sends the call once: one request, bounded by {API}_TIMEOUT,
// logged in its two records, ending in nil or a technical outcome.
type Attempt func(ctx context.Context) error

// Middleware runs around the attempts of a call: it calls next once to pass
// the call on, more than once to repeat it, or not at all to refuse it.
type Middleware func(ctx context.Context, call Call, next Attempt) error

// Run sends call through middlewares, the first the outermost, to attempt.
func Run(ctx context.Context, call Call, middlewares []Middleware, attempt Attempt) error {
	next := attempt
	for i := len(middlewares) - 1; i >= 0; i-- {
		middleware, inner := middlewares[i], next
		next = func(ctx context.Context) error { return middleware(ctx, call, inner) }
	}
	return next(ctx)
}
