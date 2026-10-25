// Package batchlimit is the command middleware of the sample access variation
// point that decides by a value of the command: the size of a batch.
package batchlimit

import (
	"context"
	"fmt"

	"github.com/example/todo/internal/command/pipeline"
)

// Batch is the mark of a command that works on many items at once and tells
// how many. A command carries it by having the method, in its own file:
//
//	func (c PlanItemsCommand) BatchSize() int { return len(c.Titles) }
type Batch interface {
	BatchSize() int
}

// Middleware is the batch limit, the command middleware that decides by a
// value of the command: a command marked Batch whose size is above limit is
// rejected as resource exhausted before its handler runs; a batch within the
// limit and every unmarked command pass unchanged. limit is
// access.Config.MaxBatch.
func Middleware(limit int) pipeline.Middleware {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		if batch, ok := cmd.(Batch); ok && batch.BatchSize() > limit {
			return nil, fmt.Errorf("%w: %s takes at most %d items at once, got %d",
				pipeline.ErrResourceExhausted, name, limit, batch.BatchSize())
		}
		return next(ctx, cmd)
	}
}
