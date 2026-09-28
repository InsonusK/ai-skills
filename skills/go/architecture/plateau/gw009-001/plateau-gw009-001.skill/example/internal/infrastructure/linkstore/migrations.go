package linkstore

import (
	"context"
	"embed"
	"fmt"
	"io/fs"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/stdlib"
	"github.com/pressly/goose/v3"
	"github.com/pressly/goose/v3/lock"
)

//go:embed migrations/*.sql
var migrationsFS embed.FS

// Migrate applies every migration in migrations/ that dsn's database has not
// yet recorded — link_checks and TaskBox schema v1 alike: the service's one
// schema history. Called by cmd/migrate (Job mode) or by cmd/linkcheck's
// guarded startup (MIGRATE_ON_START=true), never both.
func Migrate(ctx context.Context, dsn string) error {
	connConfig, err := pgx.ParseConfig(dsn)
	if err != nil {
		return fmt.Errorf("parse dsn: %w", err)
	}
	db := stdlib.OpenDB(*connConfig)
	defer db.Close()

	locker, err := lock.NewPostgresSessionLocker()
	if err != nil {
		return fmt.Errorf("build session locker: %w", err)
	}

	// goose reads migrations from the root of the FS it is given.
	migrations, err := fs.Sub(migrationsFS, "migrations")
	if err != nil {
		return fmt.Errorf("open embedded migrations: %w", err)
	}

	provider, err := goose.NewProvider(goose.DialectPostgres, db, migrations,
		goose.WithSessionLocker(locker))
	if err != nil {
		return fmt.Errorf("init migrator: %w", err)
	}

	if _, err := provider.Up(ctx); err != nil {
		return fmt.Errorf("run migrations: %w", err)
	}
	return nil
}
