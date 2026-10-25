// Source: skill vp-http-api-in-go

// Package answerfailures is the fixed HTTP middleware that answers every
// failure of a request as google.rpc.Status: an error by its kind
// (WriteError, which the routes and the application's middlewares call) and
// a panic as INTERNAL (Middleware, which the server puts around every route).
package answerfailures

import (
	"encoding/json"
	"log/slog"
	"net/http"

	"github.com/example/todo/internal/api/core/rpcstatus"
)

// Middleware answers a request whose handler panicked with 500 INTERNAL
// "internal error" and logs the panic, so the client gets google.rpc.Status
// and the request log a failed answer instead of a cut connection. The server
// puts it inside the request log and around every route; http.ErrAbortHandler,
// which a handler panics with to abort on purpose, passes through.
func Middleware(logger *slog.Logger) func(next http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			defer func() {
				p := recover()
				if p == nil {
					return
				}
				if p == http.ErrAbortHandler {
					panic(p)
				}
				logger.ErrorContext(r.Context(), "http request panicked",
					slog.String("method", r.Method), slog.String("path", r.URL.Path), slog.Any("panic", p))
				WriteStatus(w, rpcstatus.Internal, rpcstatus.InternalMessage)
			}()
			next.ServeHTTP(w, r)
		})
	}
}

// WriteError answers an error by its kind through the one table of
// rpcstatus: a kind's code and message, or INTERNAL "internal error" for an
// error with no kind. Every route answers its errors through it, and so does
// an application's middleware that rejects a request.
func WriteError(w http.ResponseWriter, err error) {
	code, message := rpcstatus.Of(err)
	WriteStatus(w, code, message)
}

// WriteStatus answers code with message as google.rpc.Status, with the HTTP
// status of the code.
func WriteStatus(w http.ResponseWriter, code rpcstatus.Code, message string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code.HTTPStatus())
	_ = json.NewEncoder(w).Encode(statusBody{Code: int(code), Message: message, Details: []any{}})
}

// statusBody is google.rpc.Status in proto-JSON, the body of every error.
type statusBody struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
	Details []any  `json:"details"`
}
