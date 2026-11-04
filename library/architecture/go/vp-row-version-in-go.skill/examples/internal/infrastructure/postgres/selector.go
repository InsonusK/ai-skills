package postgres

import (
	"context"
	"fmt"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

// Reads are the questions every selector answers about the picked rows. Each
// call asks the database again: a selector is no snapshot, so the number of
// rows a write touched comes from that write.
type Reads[T any] interface {
	// All answers every picked row; an empty list when none is picked.
	All(ctx context.Context) ([]T, error)
	// Single answers the one picked row, or nil; more than one is a failure
	// with no outcome.
	Single(ctx context.Context) (*T, error)
	// Count answers how many rows are picked.
	Count(ctx context.Context) (int64, error)
	// Any answers whether a row is picked.
	Any(ctx context.Context) (bool, error)
}

// OrderedReads are the questions that need an order, so only an ordered
// selector has them.
type OrderedReads[T any] interface {
	// First answers the first row of the order, or nil.
	First(ctx context.Context) (*T, error)
	// Take answers up to n rows, every row when fewer exist; n starts at 1.
	Take(ctx context.Context, n int) ([]T, error)
	// Page answers the rows of page (numbered from 1) of size rows; an empty
	// list past the end.
	Page(ctx context.Context, page, size int) ([]T, error)
}

// Selector works on the rows a specification picks.
type Selector[TRow any] interface {
	Reads[TRow]
	// Update applies change to every picked row and saves them in one
	// transaction, all or nothing; it answers how many rows it changed.
	Update(ctx context.Context, change func(row *TRow)) (int64, error)
	// Delete deletes every picked row in one statement; it answers how many.
	Delete(ctx context.Context) (int64, error)
	// OrderBy and OrderByDescending give a new ordered selector; this one is
	// unchanged.
	OrderBy(key Key[TRow]) OrderedSelector[TRow]
	OrderByDescending(key Key[TRow]) OrderedSelector[TRow]
}

// OrderedSelector is a selector with an order: everything a selector does
// (Count, Update, and Delete ignore the order), and the reads that need one.
type OrderedSelector[TRow any] interface {
	Selector[TRow]
	OrderedReads[TRow]
	// ThenBy gives a new ordered selector with one more key.
	ThenBy(key Key[TRow]) OrderedSelector[TRow]
}

// ResultSelector reads the results a projection answers.
type ResultSelector[TResult any] interface {
	Reads[TResult]
	OrderBy(key Key[TResult]) OrderedResultSelector[TResult]
	OrderByDescending(key Key[TResult]) OrderedResultSelector[TResult]
}

// OrderedResultSelector is a result selector with an order.
type OrderedResultSelector[TResult any] interface {
	ResultSelector[TResult]
	OrderedReads[TResult]
	ThenBy(key Key[TResult]) OrderedResultSelector[TResult]
}

// selection is what every selector shares: how to pick the rows, how to read
// them into T, and the order, which a new selector copies before it adds to.
type selection[T any] struct {
	statements Statements
	what       string
	// picked applies the model and the specification to a session.
	picked func(session *gorm.DB) *gorm.DB
	// fetch reads the query into a list: Find for rows (their columns, and
	// the preloads), Scan for a projection's own columns.
	fetch func(query *gorm.DB, list *[]T) *gorm.DB
	// qualified names a key's column with the table of the rows, which a
	// join would make ambiguous; a projection's key is a column of its
	// result, named as it is.
	qualified bool
	order     []clause.OrderByColumn
	// refused is the failure of every call when the specification holds an
	// order or a page.
	refused error
}

func (s selection[T]) by(column string, descending bool) selection[T] {
	key := clause.Column{Name: column}
	if s.qualified {
		key.Table = clause.CurrentTable
	}
	// A new slice: the selector this one came from keeps its own order.
	s.order = append(append([]clause.OrderByColumn(nil), s.order...), clause.OrderByColumn{Column: key, Desc: descending})
	return s
}

// list reads the picked rows in the order, limit and offset applied when not
// negative.
func (s selection[T]) list(ctx context.Context, limit, offset int) ([]T, error) {
	if s.refused != nil {
		return nil, s.refused
	}
	list := []T{}
	_, err := s.statements.Run(ctx, s.what, func(session *gorm.DB) *gorm.DB {
		query := s.picked(session)
		if len(s.order) > 0 {
			query = query.Clauses(clause.OrderBy{Columns: s.order})
		}
		if limit >= 0 {
			query = query.Limit(limit)
		}
		if offset > 0 {
			query = query.Offset(offset)
		}
		return s.fetch(query, &list)
	})
	if err != nil {
		return nil, err
	}
	return list, nil
}

func (s selection[T]) All(ctx context.Context) ([]T, error) {
	return s.list(ctx, -1, 0)
}

func (s selection[T]) Single(ctx context.Context) (*T, error) {
	// Two are enough to tell one from more than one.
	list, err := s.list(ctx, 2, 0)
	if err != nil || len(list) == 0 {
		return nil, err
	}
	if len(list) > 1 {
		return nil, fmt.Errorf("%s: Single: more than one row is picked", s.what)
	}
	return &list[0], nil
}

func (s selection[T]) Count(ctx context.Context) (int64, error) {
	if s.refused != nil {
		return 0, s.refused
	}
	var n int64
	// No order: an aggregate with ORDER BY a column is an error of the engine.
	_, err := s.statements.Run(ctx, s.what, func(session *gorm.DB) *gorm.DB {
		return s.picked(session).Count(&n)
	})
	return n, err
}

func (s selection[T]) Any(ctx context.Context) (bool, error) {
	if s.refused != nil {
		return false, s.refused
	}
	var exists bool
	_, err := s.statements.Run(ctx, s.what, func(session *gorm.DB) *gorm.DB {
		return session.Raw("SELECT EXISTS (?)", s.picked(s.statements.session(ctx)).Select("1")).Scan(&exists)
	})
	return exists, err
}

func (s selection[T]) First(ctx context.Context) (*T, error) {
	if err := s.needOrder("First"); err != nil {
		return nil, err
	}
	list, err := s.list(ctx, 1, 0)
	if err != nil || len(list) == 0 {
		return nil, err
	}
	return &list[0], nil
}

func (s selection[T]) Take(ctx context.Context, n int) ([]T, error) {
	if err := s.needOrder("Take"); err != nil {
		return nil, err
	}
	if n < 1 {
		return nil, fmt.Errorf("%s: %w: Take %d: n starts at 1", s.what, ErrRefused, n)
	}
	return s.list(ctx, n, 0)
}

func (s selection[T]) Page(ctx context.Context, page, size int) ([]T, error) {
	if err := s.needOrder("Page"); err != nil {
		return nil, err
	}
	if page < 1 || size < 1 {
		return nil, fmt.Errorf("%s: %w: Page %d of size %d: both start at 1", s.what, ErrRefused, page, size)
	}
	return s.list(ctx, size, (page-1)*size)
}

// needOrder guards the reads of an ordered selector, which the interfaces
// offer only after OrderBy, against a type assertion to the concrete type.
func (s selection[T]) needOrder(read string) error {
	if len(s.order) == 0 {
		return fmt.Errorf("%s: %w: %s needs an order", s.what, ErrRefused, read)
	}
	return nil
}

// rowSelector is the selector of a repository: the reads, and the writes of
// the picked rows. One type serves the unordered and the ordered selector;
// the interface it is answered as says which methods a caller sees.
type rowSelector[TRow any] struct {
	selection[TRow]
	key string
}

func (s *rowSelector[TRow]) Update(ctx context.Context, change func(row *TRow)) (int64, error) {
	if s.refused != nil {
		return 0, s.refused
	}
	var changed int64
	_, err := s.statements.Run(ctx, "update "+s.statements.table, func(session *gorm.DB) *gorm.DB {
		// The error of the transaction is the session's: Run translates it.
		session.Error = session.Transaction(func(tx *gorm.DB) error {
			rows := []TRow{}
			if err := s.picked(tx).Find(&rows).Error; err != nil {
				return err
			}
			for i := range rows {
				change(&rows[i])
				write, err := s.statements.versioned(ctx, tx, &rows[i], "update")
				if err != nil {
					return err
				}
				saved := saveRow(write, &rows[i])
				if saved.Error != nil {
					return saved.Error
				}
				// A row whose version moved since the read above fails the
				// whole call: every row or none.
				if s.statements.version != nil && saved.RowsAffected == 0 {
					return fmt.Errorf("%w: a picked row was changed meanwhile", ErrRowChanged)
				}
				changed += saved.RowsAffected
			}
			return nil
		})
		return session
	})
	if err != nil {
		return 0, err
	}
	return changed, nil
}

func (s *rowSelector[TRow]) Delete(ctx context.Context) (int64, error) {
	if s.refused != nil {
		return 0, s.refused
	}
	key := clause.Column{Table: s.statements.table, Name: s.key}
	// One statement: the rows whose key the specification picks, so a
	// specification with a join deletes the same rows it reads.
	return s.statements.Run(ctx, "delete from "+s.statements.table, func(session *gorm.DB) *gorm.DB {
		picked := s.picked(s.statements.session(ctx)).Select(session.Statement.Quote(key))
		return session.Where(clause.Expr{SQL: "? IN (?)", Vars: []any{key, picked}}).Delete(new(TRow))
	})
}

func (s *rowSelector[TRow]) OrderBy(key Key[TRow]) OrderedSelector[TRow] {
	return &rowSelector[TRow]{selection: s.restart().by(key.column, false), key: s.key}
}

func (s *rowSelector[TRow]) OrderByDescending(key Key[TRow]) OrderedSelector[TRow] {
	return &rowSelector[TRow]{selection: s.restart().by(key.column, true), key: s.key}
}

// ThenBy adds a key; OrderBy and OrderByDescending of an ordered selector
// begin a new order.
func (s *rowSelector[TRow]) ThenBy(key Key[TRow]) OrderedSelector[TRow] {
	return &rowSelector[TRow]{selection: s.by(key.column, false), key: s.key}
}

// resultSelector is the selector of a projection: reads only.
type resultSelector[TResult any] struct {
	selection[TResult]
}

func (s *resultSelector[TResult]) OrderBy(key Key[TResult]) OrderedResultSelector[TResult] {
	return &resultSelector[TResult]{selection: s.restart().by(key.column, false)}
}

func (s *resultSelector[TResult]) OrderByDescending(key Key[TResult]) OrderedResultSelector[TResult] {
	return &resultSelector[TResult]{selection: s.restart().by(key.column, true)}
}

func (s *resultSelector[TResult]) ThenBy(key Key[TResult]) OrderedResultSelector[TResult] {
	return &resultSelector[TResult]{selection: s.by(key.column, false)}
}

// restart is the selection with no order, for an OrderBy that begins one.
func (s selection[T]) restart() selection[T] {
	s.order = nil
	return s
}
