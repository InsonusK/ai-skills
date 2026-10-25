package access

import (
	"context"
	"fmt"

	"github.com/example/todo/internal/command/pipeline"
)

// Restricted is the mark of a command only a caller with a role may run. A
// command carries it by having the method, in its own file:
//
//	func (PlanItemsCommand) RequiredRole() string { return "planner" }
type Restricted interface {
	// RequiredRole is the role the caller needs.
	RequiredRole() string
}

// Middleware is the access check, the command middleware that filters by the
// command's mark: a command marked Restricted runs only for a caller with its
// role - no identity is unauthenticated, an identity without the role is
// permission denied; every other command passes unchanged. List it after the
// logging middleware, so a rejected call is logged as failed.
func Middleware() pipeline.Middleware {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		restricted, ok := cmd.(Restricted)
		if !ok {
			return next(ctx, cmd)
		}
		identity, ok := IdentityOf(ctx)
		if !ok {
			return nil, fmt.Errorf("%w: %s needs a caller", pipeline.ErrUnauthenticated, name)
		}
		if !identity.HasRole(restricted.RequiredRole()) {
			return nil, fmt.Errorf("%w: %s needs the role %s", pipeline.ErrPermissionDenied, name, restricted.RequiredRole())
		}
		return next(ctx, cmd)
	}
}
