package main

// The migration job (features/migrate.feature): run, the job's own
// function, on a database of its own per scenario on TEST_DATABASE_DSN's
// server.

import (
	"bytes"
	"context"
	"fmt"
	"os"
	"strings"
	"testing"

	"github.com/cucumber/godog"
	"github.com/jackc/pgx/v5"

	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
)

type job struct {
	database *postgrestest.Database
	logs     bytes.Buffer
	exitCode int
}

func registerJobSteps(sc *godog.ScenarioContext, j *job) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*j = job{}
		return ctx, os.Unsetenv("DATABASE_DSN")
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		os.Unsetenv("DATABASE_DSN")
		if j.database != nil {
			return ctx, j.database.Drop(context.Background())
		}
		return ctx, nil
	})
	sc.Step(`^an empty database$`, func() error {
		db, err := postgrestest.Create(context.Background())
		if err != nil {
			return err
		}
		j.database = db
		return os.Setenv("DATABASE_DSN", db.DSN)
	})
	sc.Step(`^a database that does not answer$`, func() error {
		return os.Setenv("DATABASE_DSN", "postgres://nobody@127.0.0.1:1/nothing?connect_timeout=2")
	})
	sc.Step(`^the migration job runs$`, func() error {
		j.exitCode = run(context.Background(), &j.logs)
		fmt.Printf("when: migrate -> %d %s", j.exitCode, j.logs.String())
		return nil
	})
	sc.Step(`^the job should exit with (\d+)$`, func(want int) error {
		if j.exitCode != want {
			return fmt.Errorf("exit code: got %d, want %d (%s)", j.exitCode, want, j.logs.String())
		}
		return nil
	})
	sc.Step(`^the job should log "([^"]*)"$`, func(text string) error {
		if !strings.Contains(j.logs.String(), text) {
			return fmt.Errorf("the log does not mention %q: %s", text, j.logs.String())
		}
		return nil
	})
	sc.Step(`^the table "([^"]*)" should exist$`, func(table string) error {
		conn, err := pgx.Connect(context.Background(), j.database.DSN)
		if err != nil {
			return err
		}
		defer conn.Close(context.Background())
		var found bool
		err = conn.QueryRow(context.Background(), "SELECT to_regclass($1) IS NOT NULL", table).Scan(&found)
		if err != nil || !found {
			return fmt.Errorf("no table %s (err %v)", table, err)
		}
		return nil
	})
}

func TestMigrate(t *testing.T) {
	j := &job{}
	suite := godog.TestSuite{
		ScenarioInitializer: func(sc *godog.ScenarioContext) { registerJobSteps(sc, j) },
		Options: &godog.Options{
			Format: "pretty", Paths: []string{"features"}, Tags: "~@status/todo && ~@status/broken",
			Strict: true, TestingT: t,
		},
	}
	if suite.Run() != 0 {
		t.Fatal("non-zero status returned, failed to run feature tests")
	}
}
