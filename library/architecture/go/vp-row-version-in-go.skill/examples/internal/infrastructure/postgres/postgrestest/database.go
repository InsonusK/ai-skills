// Package postgrestest gives the scenarios of an application with the
// PostgreSQL variation point what they need: a database of their own on the
// server the project provides, a database that does not answer, and a
// repository that answers a chosen failure. It is copied from the skill's
// templates and never edited; only test code imports it.
package postgrestest

import (
	"context"
	"crypto/rand"
	"encoding/hex"
	"errors"
	"fmt"
	"net/url"
	"os"
	"strings"

	"github.com/jackc/pgx/v5"

	"github.com/example/todo/internal/infrastructure/postgres"
)

// Database is a database created for one scenario, dropped by Drop.
type Database struct {
	// Config is the settings of this database, for postgres.Open.
	Config postgres.Config
	// Name is the database's name, for a step that reaches it with a
	// connection of its own.
	Name string
	// DSN is the connection string of this database, for another connection
	// than the repository's.
	DSN   string
	admin string
}

// Create creates an empty database on the server of TEST_DATABASE_DSN, which
// must have the right to create databases. With no TEST_DATABASE_DSN it
// fails: a scenario of the database never passes without one.
func Create(ctx context.Context) (*Database, error) {
	admin := os.Getenv("TEST_DATABASE_DSN")
	if admin == "" {
		return nil, errors.New("TEST_DATABASE_DSN is not set: the scenarios need the PostgreSQL the project provides (its dev container, or a service of CI)")
	}
	random := make([]byte, 6)
	if _, err := rand.Read(random); err != nil {
		return nil, err
	}
	name := "test_" + hex.EncodeToString(random)
	if err := exec(ctx, admin, "CREATE DATABASE "+name); err != nil {
		return nil, err
	}
	dsn, err := withDatabase(admin, name)
	if err != nil {
		return nil, err
	}
	cfg, err := postgres.NewConfig(dsn, false)
	if err != nil {
		return nil, fmt.Errorf("TEST_DATABASE_DSN %w", err)
	}
	return &Database{Config: cfg, Name: name, DSN: dsn, admin: admin}, nil
}

// Drop drops the database, closing the connections still open to it.
func (d *Database) Drop(ctx context.Context) error {
	return exec(ctx, d.admin, "DROP DATABASE IF EXISTS "+d.Name+" WITH (FORCE)")
}

// NotAnswering is the settings of a database that does not answer: nothing
// listens on its port.
func NotAnswering() postgres.Config {
	cfg, err := postgres.NewConfig("host=127.0.0.1 port=1 user=nobody dbname=nothing connect_timeout=2", false)
	if err != nil {
		panic(err)
	}
	return cfg
}

func exec(ctx context.Context, dsn, statement string) error {
	conn, err := pgx.Connect(ctx, dsn)
	if err != nil {
		return fmt.Errorf("connect to TEST_DATABASE_DSN: %w", err)
	}
	defer conn.Close(ctx)
	_, err = conn.Exec(ctx, statement)
	return err
}

// withDatabase is dsn naming the database name, in either form of a DSN.
func withDatabase(dsn, name string) (string, error) {
	if strings.HasPrefix(dsn, "postgres://") || strings.HasPrefix(dsn, "postgresql://") {
		u, err := url.Parse(dsn)
		if err != nil {
			return "", errors.New("TEST_DATABASE_DSN is not a URL")
		}
		u.Path = "/" + name
		return u.String(), nil
	}
	// The last value of a key counts.
	return dsn + " dbname=" + name, nil
}
