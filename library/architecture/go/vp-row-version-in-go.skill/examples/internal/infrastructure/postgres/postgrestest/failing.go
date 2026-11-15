package postgrestest

import (
	"context"

	"github.com/example/todo/internal/infrastructure/postgres"
)

// Failing is a repository whose every call answers Err: a service's scenario
// gives it the outcome to turn into a kind (postgres.ErrDuplicateKey,
// postgres.ErrUnavailable) or a failure with none. What the rows are is a
// scenario on the real engine.
type Failing[TRow any] struct {
	Err error
}

func (f Failing[TRow]) Insert(context.Context, *TRow) error { return f.Err }
func (f Failing[TRow]) Update(context.Context, *TRow) error { return f.Err }
func (f Failing[TRow]) Delete(context.Context, *TRow) error { return f.Err }

func (f Failing[TRow]) SelectBy(postgres.Specification[TRow]) postgres.Selector[TRow] {
	return failingSelector[TRow](f)
}

// failingSelector answers Err to every call; an order gives the same.
type failingSelector[TRow any] Failing[TRow]

func (f failingSelector[TRow]) All(context.Context) ([]TRow, error)            { return nil, f.Err }
func (f failingSelector[TRow]) Single(context.Context) (*TRow, error)          { return nil, f.Err }
func (f failingSelector[TRow]) Count(context.Context) (int64, error)           { return 0, f.Err }
func (f failingSelector[TRow]) Any(context.Context) (bool, error)              { return false, f.Err }
func (f failingSelector[TRow]) First(context.Context) (*TRow, error)           { return nil, f.Err }
func (f failingSelector[TRow]) Take(context.Context, int) ([]TRow, error)      { return nil, f.Err }
func (f failingSelector[TRow]) Page(context.Context, int, int) ([]TRow, error) { return nil, f.Err }
func (f failingSelector[TRow]) Delete(context.Context) (int64, error)          { return 0, f.Err }

func (f failingSelector[TRow]) Update(context.Context, func(*TRow)) (int64, error) { return 0, f.Err }

func (f failingSelector[TRow]) OrderBy(postgres.Key[TRow]) postgres.OrderedSelector[TRow] {
	return f
}

func (f failingSelector[TRow]) OrderByDescending(postgres.Key[TRow]) postgres.OrderedSelector[TRow] {
	return f
}

func (f failingSelector[TRow]) ThenBy(postgres.Key[TRow]) postgres.OrderedSelector[TRow] {
	return f
}
