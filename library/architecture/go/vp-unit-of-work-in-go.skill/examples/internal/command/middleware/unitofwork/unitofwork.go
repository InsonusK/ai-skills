// Source: skill vp-unit-of-work-in-go

// Package unitofwork is the command middleware that keeps a command's writes
// together: the outermost command begins the unit of work and ends it, a
// commit when its handler returned and a rollback when it failed; a nested
// command only joins. It shares its name with
// internal/infrastructure/postgres/unitofwork, the unit it ends; the
// composition root imports it as commandunitofwork.
package unitofwork

import (
	"context"
	"fmt"

	"github.com/example/todo/internal/command/pipeline"
)

// Units is the unit of work as the middleware sees it, with no package of the
// database: unitofwork.UnitOfWork of the PostgreSQL variation point has it.
type Units interface {
	// Begin returns ctx carrying a new unit and true for the outermost
	// command; ctx and false when ctx already carries one.
	Begin(ctx context.Context) (context.Context, bool)
	// Commit keeps every write of the unit ctx carries.
	Commit(ctx context.Context) error
	// Rollback drops every write of the unit ctx carries.
	Rollback(ctx context.Context)
}

// errNotCommitted is the answer of a failed commit, the kind unavailable with
// no cause: a kind's text reaches the caller, and the cause may name the
// database. The commit may have reached the database, so a command a caller
// retries after it is idempotent.
var errNotCommitted = fmt.Errorf("%w: the changes could not be committed: the database does not answer", pipeline.ErrUnavailable)

// Middleware ends the unit of work of every outermost command. Listed last,
// so a command the middleware before it reject opens nothing and a failed
// commit reaches the log as the command's failure.
func Middleware(units Units) pipeline.Middleware {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (result any, err error) {
		ctx, outermost := units.Begin(ctx)
		if !outermost {
			return next(ctx, cmd)
		}
		// The work is done when the handler returns: a caller that left now
		// must not undo it, and a rollback must run for a cancelled caller too.
		ending := context.WithoutCancel(ctx)
		defer func() {
			if recovered := recover(); recovered != nil {
				units.Rollback(ending)
				panic(recovered)
			}
		}()
		result, err = next(ctx, cmd)
		if err != nil {
			units.Rollback(ending)
			return result, err
		}
		if err := units.Commit(ending); err != nil {
			units.Rollback(ending)
			return nil, errNotCommitted
		}
		return result, nil
	}
}
