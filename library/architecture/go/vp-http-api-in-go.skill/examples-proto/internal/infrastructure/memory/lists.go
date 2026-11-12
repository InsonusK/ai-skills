// Package memory keeps the item lists in the process memory - the
// infrastructure of this example, so it runs with no database. The DB
// variation point replaces it with a store over a real database; the service
// above it keeps its methods, and no command changes.
package memory

import (
	"context"
	"errors"
	"sync"
)

// The technical outcomes a caller of the store tells apart, as a database
// driver reports them: a unique key taken, and no row found.
var (
	ErrDuplicateKey = errors.New("duplicate key")
	ErrNoRows       = errors.New("no rows")
)

// Row is one stored item.
type Row struct {
	Title string
	Done  bool
}

// Lists stores rows by list id, each title unique in its list.
type Lists struct {
	mu   sync.Mutex
	rows map[string][]Row
}

func NewLists() *Lists {
	return &Lists{rows: map[string][]Row{}}
}

// Insert appends row to the list, creating the list on its first row; a title
// already in the list is ErrDuplicateKey.
func (l *Lists) Insert(_ context.Context, listID string, row Row) error {
	l.mu.Lock()
	defer l.mu.Unlock()
	for _, existing := range l.rows[listID] {
		if existing.Title == row.Title {
			return ErrDuplicateKey
		}
	}
	l.rows[listID] = append(l.rows[listID], row)
	return nil
}

// Select returns the rows of the list in insertion order; a list with no
// row is ErrNoRows.
func (l *Lists) Select(_ context.Context, listID string) ([]Row, error) {
	l.mu.Lock()
	defer l.mu.Unlock()
	rows, ok := l.rows[listID]
	if !ok {
		return nil, ErrNoRows
	}
	return append([]Row(nil), rows...), nil
}
