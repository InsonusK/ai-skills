// Package service holds the services: each wraps one infrastructure and
// speaks the application's language, hiding the technique behind it.
package service

import (
	"context"
	"errors"
	"fmt"

	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
)

// itemRows is what ItemLists needs of its infrastructure;
// postgres.Repository[tables.ItemRow] has it.
type itemRows interface {
	Insert(ctx context.Context, row *tables.ItemRow) error
	SelectBy(specification postgres.Specification[tables.ItemRow]) postgres.Selector[tables.ItemRow]
}

// ItemLists keeps items in lists. It turns the repository's outcomes into the
// application's error kinds: a duplicate key is a conflict, no rows is not
// found, a database that does not answer is unavailable. The selector stops
// here: entities go up.
type ItemLists struct {
	rows itemRows
}

func NewItemLists(rows itemRows) *ItemLists {
	return &ItemLists{rows: rows}
}

// errListsUnavailable leaves the repository's failure out: the text of an
// error with a kind is answered to the caller, and the driver's names the
// database's host and user.
var errListsUnavailable = fmt.Errorf("%w: the lists are unavailable", entity.ErrUnavailable)

// Add keeps item in the list.
func (s *ItemLists) Add(ctx context.Context, listID string, item entity.Item) error {
	err := s.rows.Insert(ctx, &tables.ItemRow{ListID: listID, Title: item.Title(), Done: item.Done()})
	switch {
	case errors.Is(err, postgres.ErrDuplicateKey):
		return fmt.Errorf("%w: list %q already has an item %q", entity.ErrConflict, listID, item.Title())
	case errors.Is(err, postgres.ErrUnavailable):
		return errListsUnavailable
	}
	return err
}

// Items returns the items of the list in the order they were added.
func (s *ItemLists) Items(ctx context.Context, listID string) ([]entity.Item, error) {
	rows, err := s.rows.SelectBy(tables.ItemsOfList{ListID: listID}).OrderBy(tables.ItemPosition).All(ctx)
	if errors.Is(err, postgres.ErrUnavailable) {
		return nil, errListsUnavailable
	}
	if err != nil {
		return nil, err
	}
	if len(rows) == 0 {
		return nil, fmt.Errorf("%w: no list %q", entity.ErrNotFound, listID)
	}
	items := make([]entity.Item, len(rows))
	for i, row := range rows {
		item, err := entity.NewItem(row.Title)
		if err != nil {
			return nil, fmt.Errorf("stored item %q of list %q: %w", row.Title, listID, err)
		}
		if row.Done {
			if err := item.Complete(); err != nil {
				return nil, err
			}
		}
		items[i] = item
	}
	return items, nil
}
