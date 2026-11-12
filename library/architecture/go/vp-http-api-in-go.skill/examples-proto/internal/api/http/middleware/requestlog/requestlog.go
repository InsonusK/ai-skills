// Source: skill vp-http-api-in-go

// Package requestlog is the fixed HTTP middleware that logs every request:
// the server puts it outside everything else, so every request - a contract
// route, the docs, an unknown path, one an application's middleware
// rejected - is logged with its answer.
package requestlog

import (
	"bytes"
	"io"
	"log/slog"
	"net/http"
	"time"

	"github.com/example/todo/internal/logging"
)

// RequestIDHeader carries the id of a request: taken from the client when it
// sends one, else made here, and always sent back in the response.
const RequestIDHeader = "X-Request-Id"

// Middleware writes two records per request, so the log reads top-down as
// the request was served: "http request received" when it arrives, with the
// method, the path, and as the payload what the client sent - the query, the
// headers, and the body; then "http response sent" once it is answered, with
// the status, the duration, failed (a status of 400 or above), and as the
// payload the body as written, so a fault in turning a result into the
// response shows in the log. A failed request's second record repeats what
// the client sent, so it is in the log even when only failed records keep
// their payload. It puts the request id into the request's context, so every
// record of the request carries it once the logging variation point is
// attached. The request body is read here and given back to the routes
// unchanged.
func Middleware(logger *slog.Logger) func(next http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			start := time.Now()
			ctx := logging.WithRequestID(r.Context(), r.Header.Get(RequestIDHeader))
			w.Header().Set(RequestIDHeader, logging.RequestID(ctx))
			body, _ := io.ReadAll(r.Body) // a broken body reaches the route empty and fails its decode
			r.Body = io.NopCloser(bytes.NewReader(body))
			received := map[string]any{"query": r.URL.Query(), "headers": r.Header, "body": body}
			logger.LogAttrs(ctx, slog.LevelInfo, "http request received",
				slog.String("method", r.Method), slog.String("path", r.URL.Path), logging.Payload(received))

			answered := &responseRecorder{ResponseWriter: w, status: http.StatusOK}
			next.ServeHTTP(answered, r.WithContext(ctx))

			failed := answered.status >= http.StatusBadRequest
			sent := map[string]any{"response": map[string]any{"body": answered.body.Bytes()}}
			if failed {
				sent["request"] = received
			}
			logger.LogAttrs(ctx, slog.LevelInfo, "http response sent",
				slog.String("method", r.Method), slog.String("path", r.URL.Path),
				slog.Int("status", answered.status), slog.Duration("duration", time.Since(start)),
				logging.Failed(failed), logging.Payload(sent))
		})
	}
}

// responseRecorder remembers the status and the body a route answered with,
// while writing both to the client.
type responseRecorder struct {
	http.ResponseWriter
	status int
	body   bytes.Buffer
}

func (r *responseRecorder) WriteHeader(status int) {
	r.status = status
	r.ResponseWriter.WriteHeader(status)
}

func (r *responseRecorder) Write(b []byte) (int, error) {
	r.body.Write(b)
	return r.ResponseWriter.Write(b)
}
