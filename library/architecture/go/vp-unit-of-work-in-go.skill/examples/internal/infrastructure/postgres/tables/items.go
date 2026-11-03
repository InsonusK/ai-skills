// Package tables holds the application's rows, their keys, and the
// specifications that pick them: what a service hands to a repository. A row
// maps one table, column by column; the service builds entities from rows, so
// a row carries no rule of the business.
package tables

import (
	"gorm.io/gorm"

	"github.com/example/todo/internal/infrastructure/postgres"
)

// ItemRow is one item of a list; a title is unique in its list.
type ItemRow struct {
	ID     int64  `gorm:"primaryKey"`
	ListID string `gorm:"not null;uniqueIndex:list_items_list_id_title_key"`
	Title  string `gorm:"not null;uniqueIndex:list_items_list_id_title_key"`
	Done   bool   `gorm:"not null"`
}

func (ItemRow) TableName() string { return "list_items" }

// ItemPosition orders the items of a list as they were added.
var ItemPosition = postgres.KeyOf[ItemRow]("id")

// ItemsOfList picks the items of one list.
type ItemsOfList struct {
	postgres.SpecificationOf[ItemRow]
	ListID string
}

func (s ItemsOfList) Pick(query *gorm.DB) *gorm.DB {
	return query.Where("list_id = ?", s.ListID)
}

// Rows is every row of the application, for the scenario that checks them
// against the migrations.
var Rows = []any{ItemRow{}}
