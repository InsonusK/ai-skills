package postgres

import (
	"context"
	"database/sql"
	"fmt"

	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

// Database is the connection pool of one application's database. Opening it
// runs no statement, so the service starts while the database is down and
// its readiness check holds traffic back until it answers.
type Database struct {
	gorm *gorm.DB
}

// Open prepares the pool for the database of cfg.
func Open(cfg Config) (*Database, error) {
	db, err := gorm.Open(postgres.Open(cfg.DSN()), &gorm.Config{
		// Statements are not logged here: the repository's outcomes reach the
		// service, and the command's log says what failed.
		Logger: logger.Discard,
		// The engine's failures are translated by this package (outcomes.go),
		// from the codes of the engine, not by GORM.
		TranslateError: false,
		// A read names every column of the row, not *: a row's version is
		// the system column xmin, which * leaves out (version.go).
		QueryFields: true,
		// No statement before the first call of a repository.
		DisableAutomaticPing: true,
		// A write of one row needs no transaction of its own; the unit of work
		// variation point opens one around several.
		SkipDefaultTransaction: true,
	})
	if err != nil {
		// The driver's error may quote the DSN.
		return nil, fmt.Errorf("postgres: open the database of DATABASE_DSN: %w", translate(err, "open"))
	}
	return &Database{gorm: db}, nil
}

// Ping is the readiness check: the database answers a query. It has the
// shape of a check of the health variation point, func(context.Context)
// error, and imports nothing of it.
func (d *Database) Ping(ctx context.Context) error {
	return translate(d.gorm.WithContext(ctx).Exec("SELECT 1").Error, "ping")
}

// Use adds a GORM plugin to every statement of the database: how another
// variation point (the unit of work) reaches the statements of every
// repository with no edit of this package. Called in the composition root,
// before the first statement.
func (d *Database) Use(plugin gorm.Plugin) error {
	return d.gorm.Use(plugin)
}

// SQL is the pool itself, for its limits (SetMaxOpenConns) and for a tool
// that needs database/sql; a statement goes through a repository.
func (d *Database) SQL() (*sql.DB, error) {
	return d.gorm.DB()
}

// Close closes every connection of the pool.
func (d *Database) Close() error {
	pool, err := d.gorm.DB()
	if err != nil {
		return err
	}
	return pool.Close()
}
