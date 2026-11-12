// Package service holds the domain services: business logic reused by
// several commands. A domain service calls entities and, through interfaces
// it declares itself, services of internal/service - never infrastructure.
// Other packages import it as domainservice, so it is never confused with
// internal/service.
package service

import (
	"strings"

	"github.com/example/todo/internal/domain/entity"
)

// Planner is a domain service: it holds the rules that span several items,
// which no single item can check on its own.
type Planner struct{}

func NewPlanner() Planner { return Planner{} }

// Plan creates one open item per title; titles must be valid items and must
// not repeat, compared case-insensitively after trimming.
func (Planner) Plan(titles []string) ([]entity.Item, error) {
	if len(titles) == 0 {
		return nil, entity.ErrNoItems
	}
	seen := make(map[string]bool, len(titles))
	items := make([]entity.Item, 0, len(titles))
	for _, title := range titles {
		item, err := entity.NewItem(title)
		if err != nil {
			return nil, err
		}
		key := strings.ToLower(item.Title())
		if seen[key] {
			return nil, entity.ErrDuplicateTitle
		}
		seen[key] = true
		items = append(items, item)
	}
	return items, nil
}
