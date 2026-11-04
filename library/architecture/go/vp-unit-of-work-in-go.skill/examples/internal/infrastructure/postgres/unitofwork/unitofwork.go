// Source: skill vp-unit-of-work-in-go

// Package unitofwork is the unit of work over the PostgreSQL database: every
// write of one command, and of the commands it runs, in one transaction that
// opens on the first write. It is a GORM plugin, added to the database by
// Attach, so no repository changes: a write of a repository whose context
// carries a begun unit runs in the unit's transaction, behind a savepoint of
// its own; a read runs there once the transaction is open, and on the pool
// before. It is copied from the skill's templates and never edited.
//
// The unit lives in the command's context.Context: the outermost command
// begins it (Begin), every statement and every nested command with that
// context joins it, and only the command middleware ends it (Commit,
// Rollback). The statements of one unit run one at a time, on one connection.
package unitofwork

import (
	"context"
	"database/sql"
	"errors"
	"fmt"
	"sync"
	"time"

	"gorm.io/gorm"

	"github.com/example/todo/internal/infrastructure/postgres"
)

// endTimeout bounds a commit and a rollback, which no caller's cancellation
// reaches: the database's own timeout, as Npgsql's 30 seconds in dotnet.
const endTimeout = 30 * time.Second

// UnitOfWork begins and ends the units of the commands over one database.
type UnitOfWork struct {
	pool *sql.DB
}

// Attach adds the unit of work to every statement of db. Called once, in the
// composition root, right after postgres.Open and before the first statement.
func Attach(db *postgres.Database) (*UnitOfWork, error) {
	work := &UnitOfWork{}
	if err := db.Use(plugin{work: work}); err != nil {
		return nil, fmt.Errorf("unit of work: %w", err)
	}
	return work, nil
}

// Begin returns ctx carrying a new unit and true for the outermost command,
// which then ends the unit; ctx itself and false when ctx already carries one,
// for a nested command, which joins it. A unit opens no transaction until its
// first write.
func (w *UnitOfWork) Begin(ctx context.Context) (context.Context, bool) {
	if unitOf(ctx) != nil {
		return ctx, false
	}
	return context.WithValue(ctx, unitKey{}, &unit{pool: w.pool, savepoints: map[*gorm.Statement]string{}}), true
}

// Commit keeps every write of the unit ctx carries; a unit with no write does
// nothing. ctx's cancellation does not reach it, endTimeout bounds it. A
// failure wraps postgres.ErrUnavailable: the connection may have been lost
// after the commit reached the database, so the outcome is unknown.
func (w *UnitOfWork) Commit(ctx context.Context) error {
	work := unitOf(ctx)
	if work == nil {
		return nil
	}
	return work.end(func(tx *sql.Tx) error {
		if err := tx.Commit(); err != nil {
			return fmt.Errorf("commit: %w: %w", postgres.ErrUnavailable, err)
		}
		return nil
	})
}

// Rollback drops every write of the unit ctx carries. It reports nothing: a
// rollback that fails has lost its connection, and the server drops the
// transaction with it; a unit already ended or with no write does nothing.
func (w *UnitOfWork) Rollback(ctx context.Context) {
	if work := unitOf(ctx); work != nil {
		_ = work.end(func(tx *sql.Tx) error { return tx.Rollback() })
	}
}

type unitKey struct{}

func unitOf(ctx context.Context) *unit {
	if ctx == nil {
		return nil
	}
	work, _ := ctx.Value(unitKey{}).(*unit)
	return work
}

// unit is one command's transaction, opened by its first write.
type unit struct {
	pool *sql.DB

	mu    sync.Mutex
	conn  *sql.Conn
	tx    *sql.Tx
	stop  context.CancelFunc // ends the context of tx: at the end, or at endTimeout
	ended bool
	// savepoints holds the savepoint of each write running now, by its
	// statement; sequence numbers the savepoints.
	savepoints map[*gorm.Statement]string
	sequence   int
}

var errEnded = errors.New("unit of work: the command has ended its unit; a write after it belongs to no transaction")

// open returns the unit's transaction, beginning it on the first call. The
// connection is taken with the caller's context, so a caller that leaves stops
// the wait for one; the transaction lives in a context of its own, which only
// end stops, so the caller's cancellation never rolls it back halfway.
func (u *unit) open(ctx context.Context) (*sql.Tx, error) {
	u.mu.Lock()
	defer u.mu.Unlock()
	if u.ended {
		return nil, errEnded
	}
	if u.tx != nil {
		return u.tx, nil
	}
	conn, err := u.pool.Conn(ctx)
	if err != nil {
		return nil, err
	}
	txContext, stop := context.WithCancel(context.WithoutCancel(ctx))
	tx, err := conn.BeginTx(txContext, nil)
	if err != nil {
		stop()
		_ = conn.Close()
		return nil, err
	}
	u.conn, u.tx, u.stop = conn, tx, stop
	return tx, nil
}

// current is the open transaction, or nil before the first write.
func (u *unit) current() *sql.Tx {
	u.mu.Lock()
	defer u.mu.Unlock()
	return u.tx
}

// end runs finish on the open transaction once, then gives the connection
// back. endTimeout ends the transaction's context, which stops finish.
func (u *unit) end(finish func(tx *sql.Tx) error) error {
	u.mu.Lock()
	tx, conn, stop := u.tx, u.conn, u.stop
	u.tx, u.conn, u.stop, u.ended = nil, nil, nil, true
	u.mu.Unlock()
	if tx == nil {
		return nil
	}
	timer := time.AfterFunc(endTimeout, stop)
	err := finish(tx)
	timer.Stop()
	stop()
	// A connection lost, or still in a transaction, is closed by the pool,
	// not given to the next caller.
	_ = conn.Close()
	return err
}

// savepoint marks the transaction before one write, so a failure of the write
// (a duplicate key a service handles) undoes only that write and leaves the
// transaction usable; PostgreSQL otherwise refuses every statement after a
// failed one.
func (u *unit) savepoint(ctx context.Context, tx *sql.Tx) (string, error) {
	u.mu.Lock()
	u.sequence++
	name := fmt.Sprintf("unit_of_work_%d", u.sequence)
	u.mu.Unlock()
	if _, err := tx.ExecContext(ctx, "SAVEPOINT "+name); err != nil {
		return "", err
	}
	return name, nil
}

func (u *unit) hold(statement *gorm.Statement, savepoint string) {
	u.mu.Lock()
	defer u.mu.Unlock()
	u.savepoints[statement] = savepoint
}

func (u *unit) release(statement *gorm.Statement) (string, bool) {
	u.mu.Lock()
	defer u.mu.Unlock()
	savepoint, ok := u.savepoints[statement]
	delete(u.savepoints, statement)
	return savepoint, ok
}

// plugin puts the statements of a begun unit into its transaction.
type plugin struct {
	work *UnitOfWork
}

func (plugin) Name() string { return "unit-of-work" }

// Initialize routes the database's statements: a pool that hands the unit's
// transaction to GORM's Transaction, the callbacks that open the transaction
// before a write and join it before a read.
func (p plugin) Initialize(db *gorm.DB) error {
	pool, ok := db.ConnPool.(*sql.DB)
	if !ok {
		return fmt.Errorf("the database's pool is a %T, not *sql.DB", db.ConnPool)
	}
	p.work.pool = pool
	routed := &routedPool{pool: pool}
	db.ConnPool = routed
	db.Statement.ConnPool = routed

	// Every write: Create, Updates and Save, Delete, and Exec (Raw), which a
	// statement past the contract (Statements.Run) uses.
	for _, register := range []func(string, func(*gorm.DB)) error{
		db.Callback().Create().Before("*").Register,
		db.Callback().Update().Before("*").Register,
		db.Callback().Delete().Before("*").Register,
		db.Callback().Raw().Before("*").Register,
	} {
		if err := register("unit-of-work:open", openBeforeWrite); err != nil {
			return err
		}
	}
	for _, register := range []func(string, func(*gorm.DB)) error{
		db.Callback().Create().After("*").Register,
		db.Callback().Update().After("*").Register,
		db.Callback().Delete().After("*").Register,
		db.Callback().Raw().After("*").Register,
	} {
		if err := register("unit-of-work:release", releaseAfterWrite); err != nil {
			return err
		}
	}
	// Every read: Find, First, Count (Query), and Raw(...).Scan, Rows (Row).
	for _, register := range []func(string, func(*gorm.DB)) error{
		db.Callback().Query().Before("*").Register,
		db.Callback().Row().Before("*").Register,
	} {
		if err := register("unit-of-work:join", joinBeforeRead); err != nil {
			return err
		}
	}
	return nil
}

// openBeforeWrite runs a write of a begun unit in its transaction, behind a
// savepoint. A write inside GORM's Transaction already runs there.
func openBeforeWrite(db *gorm.DB) {
	work := unitOf(db.Statement.Context)
	if work == nil || db.Error != nil {
		return
	}
	if _, inside := db.Statement.ConnPool.(*savepointPool); inside {
		return
	}
	tx, err := work.open(db.Statement.Context)
	if err != nil {
		_ = db.AddError(err)
		return
	}
	savepoint, err := work.savepoint(db.Statement.Context, tx)
	if err != nil {
		_ = db.AddError(err)
		return
	}
	work.hold(db.Statement, savepoint)
	db.Statement.ConnPool = tx
}

// releaseAfterWrite keeps the write, or undoes it alone when it failed.
func releaseAfterWrite(db *gorm.DB) {
	work := unitOf(db.Statement.Context)
	if work == nil {
		return
	}
	savepoint, ok := work.release(db.Statement)
	if !ok {
		return
	}
	tx := work.current()
	if tx == nil {
		return
	}
	// The write is over: the caller's cancellation must not leave the
	// transaction behind a savepoint it never released.
	ctx := context.WithoutCancel(db.Statement.Context)
	if db.Error != nil {
		if _, err := tx.ExecContext(ctx, "ROLLBACK TO SAVEPOINT "+savepoint); err != nil {
			_ = db.AddError(err)
		}
		return
	}
	if _, err := tx.ExecContext(ctx, "RELEASE SAVEPOINT "+savepoint); err != nil {
		_ = db.AddError(err)
	}
}

// joinBeforeRead runs a read of a unit with an open transaction there, so a
// command reads its own writes; before the first write a read runs on the
// pool, and a command that only reads holds no transaction.
func joinBeforeRead(db *gorm.DB) {
	work := unitOf(db.Statement.Context)
	if work == nil {
		return
	}
	if _, inside := db.Statement.ConnPool.(*savepointPool); inside {
		return
	}
	if tx := work.current(); tx != nil {
		db.Statement.ConnPool = tx
	}
}

// routedPool is the database's pool as GORM sees it once the unit of work is
// attached: every statement goes to the pool, as before; GORM's Transaction
// (a selector's Update) inside a begun unit gets a savepoint of the unit's
// transaction instead of a transaction of its own.
type routedPool struct {
	pool *sql.DB
}

func (r *routedPool) PrepareContext(ctx context.Context, query string) (*sql.Stmt, error) {
	return r.pool.PrepareContext(ctx, query)
}

func (r *routedPool) ExecContext(ctx context.Context, query string, args ...any) (sql.Result, error) {
	return r.pool.ExecContext(ctx, query, args...)
}

func (r *routedPool) QueryContext(ctx context.Context, query string, args ...any) (*sql.Rows, error) {
	return r.pool.QueryContext(ctx, query, args...)
}

func (r *routedPool) QueryRowContext(ctx context.Context, query string, args ...any) *sql.Row {
	return r.pool.QueryRowContext(ctx, query, args...)
}

// GetDBConn gives postgres.Database.SQL and Close the pool itself.
func (r *routedPool) GetDBConn() (*sql.DB, error) {
	return r.pool, nil
}

// BeginTx is GORM's Transaction: a transaction of its own outside a unit, a
// savepoint of the unit's transaction inside one, ended by GORM's Commit and
// Rollback. The options of a transaction of its own do not apply in a unit.
func (r *routedPool) BeginTx(ctx context.Context, opts *sql.TxOptions) (gorm.ConnPool, error) {
	work := unitOf(ctx)
	if work == nil {
		return r.pool.BeginTx(ctx, opts)
	}
	tx, err := work.open(ctx)
	if err != nil {
		return nil, err
	}
	savepoint, err := work.savepoint(ctx, tx)
	if err != nil {
		return nil, err
	}
	return &savepointPool{tx: tx, name: savepoint, ctx: context.WithoutCancel(ctx)}, nil
}

// savepointPool is GORM's Transaction inside a unit: its statements run in
// the unit's transaction, its commit releases the savepoint, its rollback
// undoes what it wrote and nothing before it.
type savepointPool struct {
	tx   *sql.Tx
	name string
	ctx  context.Context
}

func (s *savepointPool) PrepareContext(ctx context.Context, query string) (*sql.Stmt, error) {
	return s.tx.PrepareContext(ctx, query)
}

func (s *savepointPool) ExecContext(ctx context.Context, query string, args ...any) (sql.Result, error) {
	return s.tx.ExecContext(ctx, query, args...)
}

func (s *savepointPool) QueryContext(ctx context.Context, query string, args ...any) (*sql.Rows, error) {
	return s.tx.QueryContext(ctx, query, args...)
}

func (s *savepointPool) QueryRowContext(ctx context.Context, query string, args ...any) *sql.Row {
	return s.tx.QueryRowContext(ctx, query, args...)
}

func (s *savepointPool) Commit() error {
	_, err := s.tx.ExecContext(s.ctx, "RELEASE SAVEPOINT "+s.name)
	return err
}

func (s *savepointPool) Rollback() error {
	_, err := s.tx.ExecContext(s.ctx, "ROLLBACK TO SAVEPOINT "+s.name)
	return err
}
