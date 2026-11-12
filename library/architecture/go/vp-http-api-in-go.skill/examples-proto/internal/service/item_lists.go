// Package service holds the services: each wraps one infrastructure and
// speaks the application's language, hiding the technique behind it.
package service

import (
	"context"
	"errors"
	"fmt"

	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/infrastructure/memory"
	"github.com/example/todo/internal/shared/errorkind"
)

// lists is what ItemLists needs of its infrastructure; memory.Lists has it.
type lists interface {
	Insert(ctx context.Context, listID string, row memory.Row) error
	Select(ctx context.Context, listID string) ([]memory.Row, error)
}

// ItemLists keeps items in lists. It turns the store's technical outcomes
// into the application's error kinds: a duplicate key is a conflict, no rows
// is not found.
type ItemLists struct {
	store lists
}

func NewItemLists(store lists) *ItemLists {
	return &ItemLists{store: store}
}

// Add keeps item in the list.
func (s *ItemLists) Add(ctx context.Context, listID string, item entity.Item) error {
	err := s.store.Insert(ctx, listID, memory.Row{Title: item.Title(), Done: item.Done()})
	if errors.Is(err, memory.ErrDuplicateKey) {
		return errorkind.Wrap(errorkind.ErrConflict, fmt.Sprintf("list %q already has an item %q", listID, item.Title()), err)
	}
	return err
}

// Items returns the items of the list in the order they were added.
func (s *ItemLists) Items(ctx context.Context, listID string) ([]entity.Item, error) {
	rows, err := s.store.Select(ctx, listID)
	if errors.Is(err, memory.ErrNoRows) {
		return nil, errorkind.Wrap(errorkind.ErrNotFound, fmt.Sprintf("no list %q", listID), err)
	}
	if err != nil {
		return nil, err
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
