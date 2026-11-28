// Package contract owns the logical API contract shared by transport implementations.
package contract

// ItemDTO preserves the API fields; Domain Model mapping belongs to service.
type ItemDTO struct {
	Title string
	Done  bool
}
