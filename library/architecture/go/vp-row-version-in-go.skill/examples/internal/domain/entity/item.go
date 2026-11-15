package entity

import (
	"strings"
	"unicode/utf8"
)

// MaxTitleLength is the longest item title, in characters.
const MaxTitleLength = 100

// Item is one to-do item. Its fields change only through its methods, which
// guard the item's own rules.
type Item struct {
	title string
	done  bool
}

// NewItem creates an open item; the title is trimmed and must be 1..MaxTitleLength characters.
func NewItem(title string) (Item, error) {
	title = strings.TrimSpace(title)
	if title == "" {
		return Item{}, ErrEmptyTitle
	}
	if utf8.RuneCountInString(title) > MaxTitleLength {
		return Item{}, ErrTitleTooLong
	}
	return Item{title: title}, nil
}

func (i Item) Title() string { return i.title }
func (i Item) Done() bool    { return i.done }

// Complete marks the item done; an item is completed once.
func (i *Item) Complete() error {
	if i.done {
		return ErrAlreadyDone
	}
	i.done = true
	return nil
}
