// Package access is the command half of the sample access variation point:
// who the caller is (Identity, put into the call's context by an entry
// decorator such as the HTTP middleware in internal/api/http/access), and two
// command decorators that decide by it - WithAccessCheck for the commands
// marked Restricted, WithBatchLimit for the commands marked Batch. Its
// settings (config.go) are read here, never in internal/config.
//
// It is a sample of how a variation point extends the base plateau (skill
// base-plateau-extension-in-go), not an authentication to ship: a real one
// verifies a signed token instead of looking it up in a table.
package access

import (
	"context"
	"slices"
)

// Identity is the caller of a command as plain values, the way the API layer
// passes it to the command layer (skill app-layers, "Identity resolved in the
// command layer").
type Identity struct {
	User  string
	Roles []string
}

// HasRole reports whether the caller has role.
func (i Identity) HasRole(role string) bool {
	return slices.Contains(i.Roles, role)
}

type identityKey struct{}

// WithIdentity returns a context that carries the caller's identity; an entry
// decorator calls it once it has identified the caller.
func WithIdentity(ctx context.Context, identity Identity) context.Context {
	return context.WithValue(ctx, identityKey{}, identity)
}

// IdentityOf returns the caller's identity, and false when the call came
// without credentials.
func IdentityOf(ctx context.Context) (Identity, bool) {
	identity, ok := ctx.Value(identityKey{}).(Identity)
	return identity, ok
}
