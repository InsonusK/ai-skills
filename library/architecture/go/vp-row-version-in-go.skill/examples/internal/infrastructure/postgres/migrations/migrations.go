// Package migrations holds the application's schema, one SQL file per change,
// numbered in the order they apply: 00001_create_list_items.sql. A file is
// never edited once merged; a new change is a new file. Every row of package
// tables must match the schema these files make, which a scenario checks.
package migrations

import "embed"

// Files are the migrations, embedded in the binary, so cmd/migrate and a
// start with MIGRATE_ON_START need no file beside them.
//
//go:embed *.sql
var Files embed.FS
