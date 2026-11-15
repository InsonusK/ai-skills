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
	"github.com/example/todo/internal/shared/errorkind"
)

// itemRows is what ItemLists needs of its infrastructure;
// postgres.Repository[tables.ItemRow] has it.
type itemRows interface {
	Insert(ctx context.Context, row *tables.ItemRow) error
	Update(ctx context.Context, row *tables.ItemRow) error
	SelectBy(specification postgres.Specification[tables.ItemRow]) postgres.Selector[tables.ItemRow]
}

// ItemLists keeps items in lists. It turns the repository's outcomes into the
// application's error kinds: a duplicate key is a conflict, no rows is not
// found, a row changed since the caller read it is a concurrent change, a
// database that does not answer is unavailable. The selector stops here:
// entities go up, each with its id and its version.
type ItemLists struct {
	rows itemRows
}

func NewItemLists(rows itemRows) *ItemLists {
	return &ItemLists{rows: rows}
}

// listsUnavailable keeps the repository's failure as the cause, for the log
// only: the message of an error with a kind is answered to the caller, and
// the driver's text names the database's host and user.
func listsUnavailable(cause error) error {
	return errorkind.Wrap(errorkind.ErrUnavailable, "the lists are unavailable", cause)
}

// Add keeps item in the list and answers it with its id and its first
// version, so the caller can change it next.
func (s *ItemLists) Add(ctx context.Context, listID string, item entity.Item) (entity.KeptItem, error) {
	row := tables.ItemRow{ListID: listID, Title: item.Title(), Done: item.Done()}
	err := s.rows.Insert(ctx, &row)
	switch {
	case errors.Is(err, postgres.ErrDuplicateKey):
		return entity.KeptItem{}, errorkind.Wrap(errorkind.ErrConflict, fmt.Sprintf("list %q already has an item %q", listID, item.Title()), err)
	case errors.Is(err, postgres.ErrUnavailable):
		return entity.KeptItem{}, listsUnavailable(err)
	case err != nil:
		return entity.KeptItem{}, err
	}
	return entity.KeptItem{ID: row.ID, Version: string(row.Version), Item: item}, nil
}

// Items returns the items of the list in the order they were added.
func (s *ItemLists) Items(ctx context.Context, listID string) ([]entity.KeptItem, error) {
	rows, err := s.rows.SelectBy(tables.ItemsOfList{ListID: listID}).OrderBy(tables.ItemPosition).All(ctx)
	if errors.Is(err, postgres.ErrUnavailable) {
		return nil, listsUnavailable(err)
	}
	if err != nil {
		return nil, err
	}
	if len(rows) == 0 {
		return nil, errorkind.New(errorkind.ErrNotFound, fmt.Sprintf("no list %q", listID))
	}
	items := make([]entity.KeptItem, len(rows))
	for i, row := range rows {
		if items[i], err = keptItem(row); err != nil {
			return nil, fmt.Errorf("stored item %q of list %q: %w", row.Title, listID, err)
		}
	}
	return items, nil
}

// Item returns one item of the list, with its version as it is now.
func (s *ItemLists) Item(ctx context.Context, listID string, id int64) (entity.KeptItem, error) {
	row, err := s.rows.SelectBy(tables.ItemOfList{ListID: listID, ID: id}).Single(ctx)
	if errors.Is(err, postgres.ErrUnavailable) {
		return entity.KeptItem{}, listsUnavailable(err)
	}
	if err != nil {
		return entity.KeptItem{}, err
	}
	if row == nil {
		return entity.KeptItem{}, errorkind.New(errorkind.ErrNotFound, fmt.Sprintf("no item %d in list %q", id, listID))
	}
	item, err := keptItem(*row)
	if err != nil {
		return entity.KeptItem{}, fmt.Errorf("stored item %d of list %q: %w", id, listID, err)
	}
	return item, nil
}

// Save writes item back to the list under the version item carries - the
// one the caller read - and answers it with its new version. An item changed
// since is a concurrent change: the caller reads it again and decides.
func (s *ItemLists) Save(ctx context.Context, listID string, item entity.KeptItem) (entity.KeptItem, error) {
	row := tables.ItemRow{ID: item.ID, ListID: listID, Title: item.Title(), Done: item.Done(), Version: postgres.Version(item.Version)}
	err := s.rows.Update(ctx, &row)
	switch {
	case errors.Is(err, postgres.ErrRowChanged):
		return entity.KeptItem{}, errorkind.Wrap(errorkind.ErrConcurrentChange,
			fmt.Sprintf("item %d of list %q was changed since it was read: read it again", item.ID, listID), err)
	case errors.Is(err, postgres.ErrDuplicateKey):
		return entity.KeptItem{}, errorkind.Wrap(errorkind.ErrConflict, fmt.Sprintf("list %q already has an item %q", listID, item.Title()), err)
	case errors.Is(err, postgres.ErrUnavailable):
		return entity.KeptItem{}, listsUnavailable(err)
	case err != nil:
		return entity.KeptItem{}, err
	}
	item.Version = string(row.Version)
	return item, nil
}

// keptItem is the entity of a row: its rules checked again, its id and
// version carried.
func keptItem(row tables.ItemRow) (entity.KeptItem, error) {
	item, err := entity.NewItem(row.Title)
	if err != nil {
		// %v, not %w: a stored row that breaks a rule is a failure of this
		// service, never the caller's invalid input.
		return entity.KeptItem{}, fmt.Errorf("%v", err)
	}
	if row.Done {
		if err := item.Complete(); err != nil {
			return entity.KeptItem{}, err
		}
	}
	return entity.KeptItem{ID: row.ID, Version: string(row.Version), Item: item}, nil
}
