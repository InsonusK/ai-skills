// Source: skill vp-http-api-in-go

// Package answerfailures is the fixed HTTP middleware that answers every
// failure of a request as google.rpc.Status: an error by its kind
// (WriteError, which the routes and the application's middlewares call), a
// panic as INTERNAL (Middleware), and a request no route serves - an unknown
// path, a method a path does not serve - in place of the ServeMux's plain
// text (Unrouted). The server builds both from its logger and puts them
// around everything it serves (ADR fixed-middleware-inside-the-entry).
package answerfailures

import (
	"encoding/json"
	"log/slog"
	"net/http"

	"{module-path}/internal/api/core/rpcstatus"
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

// Unrouted serves mux, answering a request no route of it serves as
// google.rpc.Status instead of the ServeMux's plain text: an unknown path 404
// NOT_FOUND "no route for {method} {path}", a method the path does not serve
// 405 UNIMPLEMENTED "method not allowed: {method} {path}" with its Allow
// header - the answers of every stack. A request a route serves passes
// through unchanged.
func Unrouted(mux *http.ServeMux) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if _, pattern := mux.Handler(r); pattern != "" {
			mux.ServeHTTP(w, r)
			return
		}
		// No route: the ServeMux answers 404 or 405; keep its status and its
		// Allow header, give it the body every error has.
		answered := &muxAnswer{header: http.Header{}}
		mux.ServeHTTP(answered, r)
		request := r.Method + " " + r.URL.Path
		if allow := answered.header.Get("Allow"); allow != "" {
			w.Header().Set("Allow", allow)
		}
		if answered.status == http.StatusMethodNotAllowed {
			writeStatus(w, http.StatusMethodNotAllowed, rpcstatus.Unimplemented, "method not allowed: "+request)
			return
		}
		writeStatus(w, http.StatusNotFound, rpcstatus.NotFound, "no route for "+request)
	})
}

// muxAnswer keeps the status and the headers the ServeMux answers a request
// of no route with, and drops its plain-text body.
type muxAnswer struct {
	header http.Header
	status int
}

func (a *muxAnswer) Header() http.Header         { return a.header }
func (a *muxAnswer) Write(b []byte) (int, error) { return len(b), nil }
func (a *muxAnswer) WriteHeader(status int)      { a.status = status }

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
	writeStatus(w, code.HTTPStatus(), code, message)
}

// writeStatus answers code with message as google.rpc.Status under the HTTP
// status httpStatus - the code's own, or the 405 the ServeMux chose.
func writeStatus(w http.ResponseWriter, httpStatus int, code rpcstatus.Code, message string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(httpStatus)
	_ = json.NewEncoder(w).Encode(statusBody{Code: int(code), Message: message, Details: []any{}})
}

// statusBody is google.rpc.Status in proto-JSON, the body of every error.
type statusBody struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
	Details []any  `json:"details"`
}
