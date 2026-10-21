// Source: skill base-plateau-in-go

// Package logging is the one way the application writes logs. Every part that
// logs - the command decorator, an entry point, later a client - writes the
// same records through *slog.Logger with the keys below; which of their
// attributes reach the output is decided only by the slog.Handler the
// composition root builds:
//
//   - NewHandler, the base plateau's: one JSON line per record, the payload
//     attribute dropped - who, what, how long, how it ended; never the data;
//   - the logging variation point's: the payload too, masked, with the
//     request id of the call.
//
// So a part never knows which handler runs, and attaching the logging
// variation point changes what is written, not the code that writes it.
package logging

import (
	"context"
	"crypto/rand"
	"encoding/hex"
	"io"
	"log/slog"
)

// The attribute keys every part logs under.
const (
	// PayloadKey holds the data of a call: a command's parameters, a
	// request's query, headers, and body, a call's arguments. The base handler
	// drops it, so no data - and no secret in it - reaches the output unless
	// the logging variation point masks it.
	PayloadKey = "payload"
	// FailedKey is true on a record of a call that ended with an error, so a
	// handler can keep the payload of failed calls only.
	FailedKey = "failed"
)

// Payload is the payload attribute of a record: v is the call's data as it is
// - a command, a map of the request's parts, a generated message.
func Payload(v any) slog.Attr { return slog.Any(PayloadKey, v) }

// Failed is the failed attribute of a record.
func Failed(failed bool) slog.Attr { return slog.Bool(FailedKey, failed) }

// NewHandler is the base plateau's handler: one JSON line per record on w at
// level and above, every payload attribute dropped.
func NewHandler(w io.Writer, level slog.Leveler) slog.Handler {
	return slog.NewJSONHandler(w, &slog.HandlerOptions{
		Level: level,
		ReplaceAttr: func(groups []string, a slog.Attr) slog.Attr {
			if len(groups) == 0 && a.Key == PayloadKey {
				return slog.Attr{} // an empty attribute is left out of the record
			}
			return a
		},
	})
}

type requestIDKey struct{}

// WithRequestID returns ctx carrying the id of the request being served, so
// every record of that request can be told apart from those of the requests
// served at the same time. An empty id is replaced by a new random one.
func WithRequestID(ctx context.Context, id string) context.Context {
	if id == "" {
		id = NewRequestID()
	}
	return context.WithValue(ctx, requestIDKey{}, id)
}

// RequestID is the id WithRequestID put into ctx, or "".
func RequestID(ctx context.Context) string {
	id, _ := ctx.Value(requestIDKey{}).(string)
	return id
}

// NewRequestID is a random id of 16 hex characters.
func NewRequestID() string {
	b := make([]byte, 8)
	_, _ = rand.Read(b)
	return hex.EncodeToString(b)
}
