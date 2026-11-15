package postgres

import (
	"context"
	"io/fs"

	"github.com/pressly/goose/v3"
)

// Migrate applies every migration of migrations not yet applied, in the order
// of their numbers, and records each in the database's goose_db_version
// table; a database already up to date is no failure. migrations holds the
// SQL files at its root (package migrations embeds them).
func Migrate(ctx context.Context, db *Database, migrations fs.FS) error {
	pool, err := db.gorm.DB()
	if err != nil {
		return translate(err, "migrate")
	}
	provider, err := goose.NewProvider(goose.DialectPostgres, pool, migrations)
	if err != nil {
		return translate(err, "migrate")
	}
	_, err = provider.Up(ctx)
	return translate(err, "migrate")
}
