package entity

// KeptItem is an item kept in a list: its id, the version of it the caller
// read, which the caller gives back with a change, and the item itself. The
// version is opaque (skill vp-row-version): it is carried and given back,
// never read or computed with.
type KeptItem struct {
	ID      int64
	Version string
	Item
}
