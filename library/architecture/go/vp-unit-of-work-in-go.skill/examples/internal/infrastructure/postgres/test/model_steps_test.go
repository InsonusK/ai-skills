package test

import (
	"context"
	"fmt"
	"slices"
	"strings"

	"github.com/cucumber/godog"
	"github.com/jackc/pgx/v5"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"

	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
)

// The columns of the application's tables, and their unique keys, as one
// text per table: what the scenario compares.
const columnsQuery = `
SELECT table_name || '.' || column_name || ' ' || data_type || CASE WHEN is_nullable = 'NO' THEN ' not null' ELSE '' END
FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = ANY($1)
UNION ALL
SELECT t.relname || ' unique (' || string_agg(a.attname, ', ' ORDER BY a.attname) || ')'
FROM pg_index i
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(i.indkey)
WHERE i.indisunique AND NOT i.indisprimary AND t.relname = ANY($1)
GROUP BY t.relname, i.indexrelid`

func registerModelSteps(sc *godog.ScenarioContext, w *world) {
	sc.Step(`^every row of the application should match its table$`, func(ctx context.Context) error {
		names := tableNames()
		migrated, err := schemaOf(ctx, w.database.DSN, names)
		if err != nil {
			return err
		}
		// The schema GORM would make of the rows, on a database of its own.
		modelled, err := postgrestest.Create(ctx)
		if err != nil {
			return err
		}
		defer modelled.Drop(ctx)
		db, err := gorm.Open(postgres.Open(modelled.DSN), &gorm.Config{Logger: logger.Discard})
		if err != nil {
			return err
		}
		if err := db.AutoMigrate(rowPointers()...); err != nil {
			return err
		}
		if pool, err := db.DB(); err == nil {
			defer pool.Close()
		}
		fromRows, err := schemaOf(ctx, modelled.DSN, names)
		if err != nil {
			return err
		}
		if !slices.Equal(migrated, fromRows) {
			return fmt.Errorf("the migrations make\n  %s\nthe rows ask for\n  %s",
				strings.Join(migrated, "\n  "), strings.Join(fromRows, "\n  "))
		}
		logf("then: %d columns and keys match", len(migrated))
		return nil
	})
}

func rowPointers() []any {
	pointers := []any{}
	for _, row := range tables.Rows {
		pointers = append(pointers, &row)
	}
	return pointers
}

func tableNames() []string {
	names := []string{}
	for _, row := range tables.Rows {
		names = append(names, row.(interface{ TableName() string }).TableName())
	}
	return names
}

func schemaOf(ctx context.Context, dsn string, names []string) ([]string, error) {
	conn, err := pgx.Connect(ctx, dsn)
	if err != nil {
		return nil, err
	}
	defer conn.Close(ctx)
	rows, err := conn.Query(ctx, columnsQuery, names)
	if err != nil {
		return nil, err
	}
	schema, err := pgx.CollectRows(rows, pgx.RowTo[string])
	slices.Sort(schema)
	return schema, err
}
