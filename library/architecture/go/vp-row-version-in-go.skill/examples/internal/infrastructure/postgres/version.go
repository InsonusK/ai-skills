package postgres

import (
	"context"
	"database/sql/driver"
	"fmt"
	"reflect"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
	"gorm.io/gorm/schema"
)

// Version is the version of a row whose table changes (vp-row-version):
// PostgreSQL's xmin, which the engine moves on every write, by whatever code.
// A row declares it as
//
//	Version postgres.Version `gorm:"column:xmin;->;-:migration"`
//
// read with the row (->), never written by GORM, never made by a migration
// (xmin is a system column). The repository compares it in Update and
// Delete and puts the new one on the row it saved; a row that declares none
// is written by its key alone. It is opaque: compared for equality and
// carried as text, nothing computes with it.
type Version string

// versionColumn is the system column a version is read from.
const versionColumn = "xmin"

// Scan reads xmin, which the driver answers as a number or as text.
func (v *Version) Scan(src any) error {
	switch value := src.(type) {
	case nil:
		*v = ""
	case string:
		*v = Version(value)
	case []byte:
		*v = Version(value)
	default:
		*v = Version(fmt.Sprint(value))
	}
	return nil
}

// Value gives the version as text; a statement casts it to xid.
func (v Version) Value() (driver.Value, error) { return string(v), nil }

// versionField is the field of TRow that declares the version, or nil.
func versionField(rowSchema *schema.Schema) *schema.Field {
	field := rowSchema.LookUpField(versionColumn)
	if field == nil || field.FieldType != reflect.TypeOf(Version("")) {
		return nil
	}
	return field
}

// versioned adds to a write of row the comparison of its version and reads
// the new one back onto row; a row of a table with no version is unchanged.
// A versioned row with no version is a defect of the caller, who skipped
// the read: ErrRefused.
func (s Statements) versioned(ctx context.Context, session *gorm.DB, row any, write string) (*gorm.DB, error) {
	if s.version == nil {
		return session, nil
	}
	value, _ := s.version.ValueOf(ctx, reflect.ValueOf(row))
	version, _ := value.(Version)
	if version == "" {
		return nil, fmt.Errorf("%s %s: %w: the row has no version: read it before you write it", write, s.table, ErrRefused)
	}
	return session.
		Where(clause.Expr{SQL: "? = ?::xid", Vars: []any{clause.Column{Table: s.table, Name: versionColumn}, version}}).
		Clauses(clause.Returning{Columns: []clause.Column{{Name: versionColumn}}}), nil
}

// returningVersion reads the version of an inserted row back onto it.
func (s Statements) returningVersion(session *gorm.DB, key string) *gorm.DB {
	if s.version == nil {
		return session
	}
	return session.Clauses(clause.Returning{Columns: []clause.Column{{Name: key}, {Name: versionColumn}}})
}
