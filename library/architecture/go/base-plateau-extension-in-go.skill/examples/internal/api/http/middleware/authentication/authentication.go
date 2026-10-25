// Package authentication is the HTTP middleware of the sample access
// variation point: it turns the request's bearer token into the caller's
// identity. Whether the caller may run a command is the command layer's
// decision (internal/command/middleware/access), so the gRPC API, added
// later, gets the same check with a middleware of its own.
package authentication

import (
	"fmt"
	"net/http"
	"strings"

	"github.com/example/todo/internal/api/http/middleware/answerfailures"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command/middleware/access"
	"github.com/example/todo/internal/command/pipeline"
)

// Middleware identifies the caller: a request
// with "Authorization: Bearer {token}" of a known token goes on with the
// token's identity in its context; a request without the header goes on with
// none, and the command decides whether it needs one; a header that names no
// known caller is answered unauthenticated at once. tokens maps each token to
// its caller (access.Config.Tokens).
func Middleware(tokens map[string]access.Identity) httppipeline.Middleware {
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
				answerfailures.WriteError(w, fmt.Errorf("%w: the bearer token names no known caller", pipeline.ErrUnauthenticated))
				return
			}
			next.ServeHTTP(w, r.WithContext(access.WithIdentity(r.Context(), identity)))
		})
	}
}
