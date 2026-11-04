package postgres

import (
	"context"
	"errors"
	"fmt"
	"net"

	"github.com/jackc/pgx/v5/pgconn"
)

// The outcomes of a repository a service tells apart, each turned into an
// error kind by the service: a duplicate key is a conflict, a missing
// reference is not found (or what the business says), a changed row is a
// concurrent change, an unavailable database is unavailable. Every other
// failure wraps none of them and keeps no kind.
var (
	// ErrDuplicateKey: a unique key is taken (23505).
	ErrDuplicateKey = errors.New("duplicate key")
	// ErrMissingReference: a foreign key names no row (23503).
	ErrMissingReference = errors.New("missing reference")
	// ErrRowChanged: the row to update or delete is gone, or was changed
	// meanwhile (with vp-row-version).
	ErrRowChanged = errors.New("row changed")
	// ErrUnavailable: no answer, or a failure a retry may cure - a lost
	// connection, a deadlock (40P01), a serialization failure (40001), a lock
	// not available (55P03), a server shutting down.
	ErrUnavailable = errors.New("database unavailable")
)

// ErrRefused is a call the contract does not allow: a specification holding
// an order or a page, Take or Page below 1. It is a defect of the caller, so
// it is no outcome and keeps no kind.
var ErrRefused = errors.New("refused by the repository's contract")

// translate wraps err in the outcome it is, keeping err as the cause, and
// names what was done: "insert into tasks: duplicate key: ERROR: ...".
func translate(err error, what string) error {
	if err == nil {
		return nil
	}
	if outcome := outcomeOf(err); outcome != nil {
		return fmt.Errorf("%s: %w: %w", what, outcome, err)
	}
	return fmt.Errorf("%s: %w", what, err)
}

func outcomeOf(err error) error {
	var pg *pgconn.PgError
	if errors.As(err, &pg) {
		switch {
		case pg.Code == "23505":
			return ErrDuplicateKey
		case pg.Code == "23503":
			return ErrMissingReference
		case transient(pg.Code):
			return ErrUnavailable
		}
		return nil
	}
	// The caller's own cancellation is not the database's failure.
	if errors.Is(err, context.Canceled) {
		return nil
	}
	var connect *pgconn.ConnectError
	var network net.Error
	if errors.As(err, &connect) || errors.As(err, &network) || pgconn.Timeout(err) || pgconn.SafeToRetry(err) {
		return ErrUnavailable
	}
	return nil
}

// transient tells the codes a retry of the same statement may cure.
func transient(code string) bool {
	switch code {
	case "40001", // serialization_failure
		"40P01", // deadlock_detected
		"55P03", // lock_not_available
		"53300", // too_many_connections
		"57P01", // admin_shutdown
		"57P02", // crash_shutdown
		"57P03": // cannot_connect_now
		return true
	}
	return len(code) == 5 && code[:2] == "08" // connection_exception
}
