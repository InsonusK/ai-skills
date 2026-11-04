package tables

// NeverChanges marks a row of a table whose rows never change once
// inserted (skill vp-row-version): it declares no version, and a service
// never updates it. Every row of Rows either declares a postgres.Version or
// has this method; a scenario fails on a row with neither.
//
//	func (EventRow) NeverChanges() {}
type NeverChanges interface {
	NeverChanges()
}
