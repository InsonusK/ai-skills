package postgres

import "gorm.io/gorm"

// Specification picks rows of TRow: conditions, joins along the relations of
// the rows, and preloads, applied to query in Pick. It holds no order, no
// page, and runs no statement: SelectBy refuses one with an order or a page,
// and the selector says what is done with the rows and in what order.
//
// A specification is a type of the application, named in the business's
// terms, that embeds SpecificationOf[TRow]:
//
//	type UnclosedTasks struct {
//		postgres.SpecificationOf[TaskRow]
//		Owner string
//	}
//
//	func (s UnclosedTasks) Pick(query *gorm.DB) *gorm.DB {
//		return query.Joins("JOIN owners ON owners.id = tasks.owner_id").
//			Where("owners.name = ? AND NOT tasks.done", s.Owner)
//	}
type Specification[TRow any] interface {
	Pick(query *gorm.DB) *gorm.DB
	specificationOf(TRow)
}

// SpecificationOf ties a specification to the rows it picks, so SelectBy of
// another table's repository does not take it.
type SpecificationOf[TRow any] struct{}

func (SpecificationOf[TRow]) specificationOf(TRow) {}

// Projection is a specification that answers TResult instead of TRow: its
// Pick also selects the columns of TResult, by their names
// (Select("owners.name AS owner, tasks.title")), often from a joined table.
// The same rules hold: no order, no page, no statement.
type Projection[TRow, TResult any] interface {
	Pick(query *gorm.DB) *gorm.DB
	projectionOf(TRow, TResult)
}

// ProjectionOf ties a projection to the rows it reads and the result it
// answers.
type ProjectionOf[TRow, TResult any] struct{}

func (ProjectionOf[TRow, TResult]) projectionOf(TRow, TResult) {}

// Key names a column to order by: of TRow for a selector of rows, of TResult
// for a selector of a projection's results. A table declares its keys beside
// its row, so a service orders by a name, not by a column's text:
//
//	var TaskTitle = postgres.KeyOf[TaskRow]("title")
type Key[T any] struct {
	column string
}

// KeyOf is the key of column.
func KeyOf[T any](column string) Key[T] {
	return Key[T]{column: column}
}
