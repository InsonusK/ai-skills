// Source: skill vp-logging-in-go

package masked

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"

	"{module-path}/internal/logging"
)

// RequestIDKey is the attribute the request id of the call is written under.
const RequestIDKey = "request_id"

// Handler writes one JSON line per record like the base handler, but keeps
// the payload: masked (see Mask), cut to Config.MaxBytes, and only on the
// records Config.Payload allows. It adds the request id of the record's
// context.
type Handler struct {
	next slog.Handler
	cfg  Config
}

// NewHandler is the handler the composition root builds in place of
// logging.NewHandler when the logging variation point is attached.
func NewHandler(w io.Writer, level slog.Leveler, cfg Config) *Handler {
	return &Handler{next: slog.NewJSONHandler(w, &slog.HandlerOptions{Level: level}), cfg: cfg}
}

func (h *Handler) Enabled(ctx context.Context, level slog.Level) bool {
	return h.next.Enabled(ctx, level)
}

// Handle writes the record with its payload rendered, or without it.
func (h *Handler) Handle(ctx context.Context, r slog.Record) error {
	failed := false
	r.Attrs(func(a slog.Attr) bool {
		if a.Key == logging.FailedKey && a.Value.Kind() == slog.KindBool {
			failed = a.Value.Bool()
		}
		return true
	})
	out := slog.NewRecord(r.Time, r.Level, r.Message, r.PC)
	if id := logging.RequestID(ctx); id != "" {
		out.AddAttrs(slog.String(RequestIDKey, id))
	}
	r.Attrs(func(a slog.Attr) bool {
		if a.Key != logging.PayloadKey {
			out.AddAttrs(a)
		} else if h.keeps(failed) {
			out.AddAttrs(h.payload(a.Value.Any()))
		}
		return true
	})
	return h.next.Handle(ctx, out)
}

// keeps reports whether a record of this outcome keeps its payload.
func (h *Handler) keeps(failed bool) bool {
	switch h.cfg.Payload {
	case PayloadOff:
		return false
	case PayloadErrors:
		return failed
	default:
		return true
	}
}

// payload is the payload attribute masked and, past MaxBytes, cut: then it
// is the beginning of its JSON as a string, saying how long the whole was.
func (h *Handler) payload(v any) slog.Attr {
	encoded, err := json.Marshal(Mask(v))
	if err != nil {
		return slog.String(logging.PayloadKey, "payload not written: "+err.Error())
	}
	if len(encoded) > h.cfg.MaxBytes {
		return slog.String(logging.PayloadKey, fmt.Sprintf("%s... (cut, %d bytes)", encoded[:h.cfg.MaxBytes], len(encoded)))
	}
	return slog.Any(logging.PayloadKey, json.RawMessage(encoded))
}

// WithAttrs masks a payload given to logger.With as well, so no way of
// passing it escapes the mask.
func (h *Handler) WithAttrs(attrs []slog.Attr) slog.Handler {
	kept := make([]slog.Attr, 0, len(attrs))
	for _, a := range attrs {
		if a.Key != logging.PayloadKey {
			kept = append(kept, a)
		} else if h.keeps(false) {
			kept = append(kept, h.payload(a.Value.Any()))
		}
	}
	return &Handler{next: h.next.WithAttrs(kept), cfg: h.cfg}
}

func (h *Handler) WithGroup(name string) slog.Handler {
	return &Handler{next: h.next.WithGroup(name), cfg: h.cfg}
}
