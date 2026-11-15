// Package postgres is the PostgreSQL variation point's fixed code: the
// settings, the connection, one generic repository with its selectors, the
// translation of the engine's failures into the repository's outcomes, and the
// migrations. It is copied from the skill's templates and never edited; the
// application's rows, specifications, and migrations live in packages of
// their own (tables, migrations).
package postgres

import (
	"errors"
	"fmt"
	"log/slog"
	"os"
	"strconv"

	"github.com/jackc/pgx/v5/pgconn"
)

// Config is the database's settings. The DSN holds the password, so it is
// kept unexported, and every way of printing a Config - fmt's verbs, slog -
// leaves it out.
type Config struct {
	dsn string
	// MigrateOnStart is MIGRATE_ON_START: true applies the migrations at
	// start, for a deployment pinned to one instance; false (the default)
	// leaves them to the one-shot job, cmd/migrate.
	MigrateOnStart bool
}

// LoadConfig reads DATABASE_DSN (required, in a form pgx reads: a URL or
// key=value pairs) and MIGRATE_ON_START. A failure names the variable and
// never repeats its value.
func LoadConfig() (Config, error) {
	dsn := os.Getenv("DATABASE_DSN")
	if dsn == "" {
		return Config{}, errors.New("config: DATABASE_DSN is required")
	}
	migrateOnStart := false
	if raw := os.Getenv("MIGRATE_ON_START"); raw != "" {
		on, err := strconv.ParseBool(raw)
		if err != nil {
			return Config{}, fmt.Errorf("config: MIGRATE_ON_START must be true or false, not %q", raw)
		}
		migrateOnStart = on
	}
	cfg, err := NewConfig(dsn, migrateOnStart)
	if err != nil {
		return Config{}, fmt.Errorf("config: DATABASE_DSN %w", err)
	}
	return cfg, nil
}

// NewConfig is the settings of dsn, for a tool or a test that has its DSN
// from elsewhere (TEST_DATABASE_DSN); the failure never repeats dsn.
func NewConfig(dsn string, migrateOnStart bool) (Config, error) {
	// The parser's own error quotes the DSN with its password masked; the
	// host and the user stay out of the log too, so it is dropped here.
	if _, err := pgconn.ParseConfig(dsn); err != nil {
		return Config{}, errors.New("is not a connection string PostgreSQL can use")
	}
	return Config{dsn: dsn, MigrateOnStart: migrateOnStart}, nil
}

// DSN is the connection string, for Open only.
func (c Config) DSN() string { return c.dsn }

// String is what %v, %+v, and %s print: every setting but the DSN.
func (c Config) String() string {
	return fmt.Sprintf("postgres.Config{DSN: (hidden), MigrateOnStart: %t}", c.MigrateOnStart)
}

// GoString is what %#v prints.
func (c Config) GoString() string { return c.String() }

// LogValue is what slog writes for a Config.
func (c Config) LogValue() slog.Value {
	return slog.GroupValue(slog.String("dsn", "(hidden)"), slog.Bool("migrate_on_start", c.MigrateOnStart))
}
