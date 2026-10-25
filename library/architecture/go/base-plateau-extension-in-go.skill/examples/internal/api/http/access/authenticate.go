// Package access is the HTTP half of the sample access variation point: the
// middleware that turns the request's bearer token into the caller's
// identity. Whether the caller may run a command is the command layer's
// decision (internal/command/access), so the gRPC API, added later, gets the
// same check with an interceptor of its own.
package access

import (
	"fmt"
	"net/http"
	"strings"

	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command/access"
	"github.com/example/todo/internal/command/pipeline"
)

// Authenticate is the entry decorator that identifies the caller: a request
// with "Authorization: Bearer {token}" of a known token goes on with the
// token's identity in its context; a request without the header goes on with
// none, and the command decides whether it needs one; a header that names no
// known caller is answered unauthenticated at once. tokens maps each token to
// its caller (access.Config.Tokens).
func Authenticate(tokens map[string]access.Identity) httppipeline.Middleware {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			header := r.Header.Get("Authorization")
			if header == "" {
				next.ServeHTTP(w, r)
				return
			}
			token, isBearer := strings.CutPrefix(header, "Bearer ")
			identity, known := tokens[token]
			if !isBearer || !known {
				httppipeline.WriteError(w, fmt.Errorf("%w: the bearer token names no known caller", pipeline.ErrUnauthenticated))
				return
			}
			next.ServeHTTP(w, r.WithContext(access.WithIdentity(r.Context(), identity)))
		})
	}
}
