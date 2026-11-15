package command

import (
	"context"
	"fmt"
	"sync"

	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/shared/errorkind"
)

// KeptLists keeps the lists of this example in the process while it runs, lost
// when it stops: the HTTP variation point adds a way to call the application,
// not a store, and AddItem and ListItems need somewhere to keep what they
// show. It has what the handlers ask of the item lists (itemLists); the DB
// variation point builds a service over the database in its place, and no
// handler changes. A title is unique in its list.
type KeptLists struct {
	mu    sync.Mutex
	items map[string][]entity.Item
}

func NewKeptLists() *KeptLists {
	return &KeptLists{items: map[string][]entity.Item{}}
}

// Add keeps item at the end of the list, started on its first item; a title
// already in the list is a conflict.
func (l *KeptLists) Add(_ context.Context, listID string, item entity.Item) error {
	l.mu.Lock()
	defer l.mu.Unlock()
	for _, kept := range l.items[listID] {
		if kept.Title() == item.Title() {
			return errorkind.New(errorkind.ErrConflict, fmt.Sprintf("list %q already has an item %q", listID, item.Title()))
		}
	}
	l.items[listID] = append(l.items[listID], item)
	return nil
}

// Items returns the items of the list in the order they were added; a list
// nobody added to is not found.
func (l *KeptLists) Items(_ context.Context, listID string) ([]entity.Item, error) {
	l.mu.Lock()
	defer l.mu.Unlock()
	items, ok := l.items[listID]
	if !ok {
		return nil, errorkind.New(errorkind.ErrNotFound, fmt.Sprintf("no list %q", listID))
	}
	return append([]entity.Item(nil), items...), nil
}
