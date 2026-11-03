package postgres

import (
	"context"
	"fmt"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

// Statements runs the statements of one table, each through the translation
// of the engine's failures into the repository's outcomes. A table's own
// repository embeds Repository, and with it Statements, and passes through
// Run every statement the contract cannot express.
type Statements struct {
	db    *gorm.DB
	table string
}

// Run runs statement on a session of ctx and answers the rows it touched.
// what names the statement in the failure: "rename tasks".
//
//	n, err := r.Run(ctx, "rename tasks", func(session *gorm.DB) *gorm.DB {
//		return session.Exec("UPDATE tasks SET title = ? WHERE title = ?", to, from)
//	})
func (s Statements) Run(ctx context.Context, what string, statement func(session *gorm.DB) *gorm.DB) (int64, error) {
	result := statement(s.session(ctx))
	return result.RowsAffected, translate(result.Error, what)
}

// session is a new statement chain on the pool, bound to ctx.
func (s Statements) session(ctx context.Context) *gorm.DB {
	return s.db.WithContext(ctx)
}

// Repository is the one contract of every table: Insert, Update, Delete of
// one row, each saved before it returns, and SelectBy, which gives a selector
// over the rows a specification picks. TRow is a struct GORM maps (its
// columns, its TableName, its primary key).
type Repository[TRow any] struct {
	Statements
	// key is the primary key's column, by which the selector's Delete picks
	// the rows of a specification.
	key string
}

// NewRepository is the repository of the table of TRow. A type GORM cannot
// map, or one with no primary key, is a defect of the composition root, so it
// stops the start.
func NewRepository[TRow any](db *Database) *Repository[TRow] {
	table, key := mappingOf[TRow](db.gorm)
	if key == "" {
		panic(fmt.Sprintf("postgres: the row %s has no primary key", table))
	}
	return &Repository[TRow]{Statements: Statements{db: db.gorm, table: table}, key: key}
}

// Insert saves row; a key the database generates is set on row.
func (r *Repository[TRow]) Insert(ctx context.Context, row *TRow) error {
	_, err := r.Run(ctx, "insert into "+r.table, func(session *gorm.DB) *gorm.DB {
		// A related row is saved by its own repository, never with this one.
		return session.Omit(clause.Associations).Create(row)
	})
	return err
}

// Update saves every column of row by its primary key; a row no longer there
// is ErrRowChanged.
func (r *Repository[TRow]) Update(ctx context.Context, row *TRow) error {
	n, err := r.Run(ctx, "update "+r.table, func(session *gorm.DB) *gorm.DB {
		return saveRow(session, row)
	})
	if err == nil && n == 0 {
		return fmt.Errorf("update %s: %w: no row has its key", r.table, ErrRowChanged)
	}
	return err
}

// Delete deletes row by its primary key; a row no longer there is
// ErrRowChanged.
func (r *Repository[TRow]) Delete(ctx context.Context, row *TRow) error {
	n, err := r.Run(ctx, "delete from "+r.table, func(session *gorm.DB) *gorm.DB {
		return session.Delete(row)
	})
	if err == nil && n == 0 {
		return fmt.Errorf("delete from %s: %w: no row has its key", r.table, ErrRowChanged)
	}
	return err
}

// SelectBy gives the selector over the rows specification picks. No statement
// runs until a method of the selector is called; a specification holding an
// order or a page makes every call of the selector fail with ErrRefused.
func (r *Repository[TRow]) SelectBy(specification Specification[TRow]) Selector[TRow] {
	what := "select " + r.table
	return &rowSelector[TRow]{
		selection: selection[TRow]{
			statements: r.Statements,
			what:       what,
			picked:     func(session *gorm.DB) *gorm.DB { return specification.Pick(session.Model(new(TRow))) },
			fetch:      func(query *gorm.DB, rows *[]TRow) *gorm.DB { return query.Find(rows) },
			qualified:  true,
			refused:    refusal(r.db, new(TRow), specification.Pick, what),
		},
		key: r.key,
	}
}

// Projections answers the results of projections over the table of TRow: a
// repository that only reads, for a service that needs columns of several
// tables at once.
type Projections[TRow, TResult any] struct {
	Statements
}

// NewProjections is the reader of projections from the table of TRow into
// TResult.
func NewProjections[TRow, TResult any](db *Database) *Projections[TRow, TResult] {
	table, _ := mappingOf[TRow](db.gorm)
	return &Projections[TRow, TResult]{Statements: Statements{db: db.gorm, table: table}}
}

// SelectBy gives the selector over the results projection answers; like the
// repository's, it runs no statement yet and refuses an order or a page.
func (p *Projections[TRow, TResult]) SelectBy(projection Projection[TRow, TResult]) ResultSelector[TResult] {
	what := "select from " + p.table
	return &resultSelector[TResult]{selection: selection[TResult]{
		statements: p.Statements,
		what:       what,
		picked:     func(session *gorm.DB) *gorm.DB { return projection.Pick(session.Model(new(TRow))) },
		fetch:      func(query *gorm.DB, results *[]TResult) *gorm.DB { return query.Scan(results) },
		refused:    refusal(p.db, new(TRow), projection.Pick, what),
	}}
}

// saveRow writes every column of row, the zero values too, and none of its
// related rows.
func saveRow(session *gorm.DB, row any) *gorm.DB {
	return session.Model(row).Select("*").Omit(clause.Associations).Updates(row)
}

// mappingOf answers the table of TRow and its primary key's column.
func mappingOf[TRow any](db *gorm.DB) (table, key string) {
	statement := &gorm.Statement{DB: db}
	if err := statement.Parse(new(TRow)); err != nil {
		panic(fmt.Sprintf("postgres: %T is no row GORM maps: %v", *new(TRow), err))
	}
	if field := statement.Schema.PrioritizedPrimaryField; field != nil {
		key = field.DBName
	}
	return statement.Schema.Table, key
}

// refusal applies pick to a statement that never runs and answers ErrRefused
// when it holds an order or a page.
func refusal(db *gorm.DB, model any, pick func(*gorm.DB) *gorm.DB, what string) error {
	query := pick(db.Session(&gorm.Session{DryRun: true, NewDB: true}).Model(model))
	_, ordered := query.Statement.Clauses["ORDER BY"]
	_, paged := query.Statement.Clauses["LIMIT"]
	if ordered || paged {
		return fmt.Errorf("%s: %w: a specification picks rows; ask the selector for an order or a page", what, ErrRefused)
	}
	return nil
}
