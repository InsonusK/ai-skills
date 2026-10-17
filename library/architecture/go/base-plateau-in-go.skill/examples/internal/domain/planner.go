package domain

import "strings"

// Planner is a domain service: it holds the rules that span several items,
// which no single item can check on its own.
type Planner struct{}

func NewPlanner() Planner { return Planner{} }

// Plan creates one open item per title; titles must be valid items and must
// not repeat, compared case-insensitively after trimming.
func (Planner) Plan(titles []string) ([]Item, error) {
	if len(titles) == 0 {
		return nil, ErrNoItems
	}
	seen := make(map[string]bool, len(titles))
	items := make([]Item, 0, len(titles))
	for _, title := range titles {
		item, err := NewItem(title)
		if err != nil {
			return nil, err
		}
		key := strings.ToLower(item.Title())
		if seen[key] {
			return nil, ErrDuplicateTitle
		}
		seen[key] = true
		items = append(items, item)
	}
	return items, nil
}
