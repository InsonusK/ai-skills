// Source: skill vp-logging-in-go

package masked

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"sync"
	"time"

	"{module-path}/internal/logging"
)

// RequestIDKey is the attribute the request id of the call is written under.
const RequestIDKey = "request_id"

// NotParsedMessage is the WARN record written the first time a record of one
// message holds a body the mask could not read in one place of its payload.
const NotParsedMessage = "log payload not parsed"

// Handler writes one JSON line per record like the base handler, but keeps
// the payload: masked (see Mask), cut to Config.MaxBytes, and only on the
// records Config.Payload allows. It adds the request id of the record's
// context, and warns once about every place a body could not be read.
type Handler struct {
	next slog.Handler
	cfg  Config
	// warned holds "message\x00path" of every unparsed body already warned
	// about; shared by every handler WithAttrs and WithGroup derive.
	warned *sync.Map
	// bound are the unparsed bodies of a payload given to logger.With, warned
	// about with the records that logger writes.
	bound []Unparsed
}

// NewHandler is the handler the composition root builds in place of
// logging.NewHandler when the logging variation point is attached.
func NewHandler(w io.Writer, level slog.Leveler, cfg Config) *Handler {
	return &Handler{next: slog.NewJSONHandler(w, &slog.HandlerOptions{Level: level}), cfg: cfg, warned: &sync.Map{}}
}

func (h *Handler) Enabled(ctx context.Context, level slog.Level) bool {
	return h.next.Enabled(ctx, level)
}

// Handle writes the record with its payload rendered, or without it, then a
// warning for each body of it not yet warned about.
func (h *Handler) Handle(ctx context.Context, r slog.Record) error {
	failed := false
	r.Attrs(func(a slog.Attr) bool {
		if a.Key == logging.FailedKey && a.Value.Kind() == slog.KindBool {
			failed = a.Value.Bool()
		}
		return true
	})
	out := slog.NewRecord(r.Time, r.Level, r.Message, r.PC)
	addRequestID(ctx, &out)
	unparsed := h.bound
	r.Attrs(func(a slog.Attr) bool {
		if a.Key != logging.PayloadKey {
			out.AddAttrs(a)
		} else if h.keeps(failed) {
			attr, found := h.payload(a.Value.Any())
			out.AddAttrs(attr)
			unparsed = append(unparsed[:len(unparsed):len(unparsed)], found...)
		}
		return true
	})
	if err := h.next.Handle(ctx, out); err != nil {
		return err
	}
	return h.warn(ctx, r.Message, unparsed)
}

func addRequestID(ctx context.Context, r *slog.Record) {
	if id := logging.RequestID(ctx); id != "" {
		r.AddAttrs(slog.String(RequestIDKey, id))
	}
}

// warn writes a WARN record for each unparsed body the first time a record of
// this message has one at that path: a body nobody can read says so once,
// not on every request, and the record keeps the level its code chose.
func (h *Handler) warn(ctx context.Context, message string, unparsed []Unparsed) error {
	if len(unparsed) == 0 || !h.next.Enabled(ctx, slog.LevelWarn) {
		return nil
	}
	for _, u := range unparsed {
		if _, seen := h.warned.LoadOrStore(message+"\x00"+u.Path, true); seen {
			continue
		}
		w := slog.NewRecord(time.Now(), slog.LevelWarn, NotParsedMessage, 0)
		addRequestID(ctx, &w)
		w.AddAttrs(slog.String("record", message), slog.String("field", u.Path), slog.Int("bytes", u.Bytes))
		if err := h.next.Handle(ctx, w); err != nil {
			return err
		}
	}
	return nil
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
// The mask runs before the cut, so a cut never leaves a secret unmasked.
func (h *Handler) payload(v any) (slog.Attr, []Unparsed) {
	masked, unparsed := Mask(v)
	encoded, err := encode(masked)
	if err != nil {
		return slog.String(logging.PayloadKey, "payload not written: "+err.Error()), unparsed
	}
	if len(encoded) > h.cfg.MaxBytes {
		return slog.String(logging.PayloadKey, fmt.Sprintf("%s... (cut, %d bytes)", encoded[:h.cfg.MaxBytes], len(encoded))), unparsed
	}
	return slog.Any(logging.PayloadKey, json.RawMessage(encoded)), unparsed
}

// encode writes "<" and ">" as they are, so the size a payload is cut at is
// the size every stack counts.
func encode(v any) ([]byte, error) {
	var b bytes.Buffer
	enc := json.NewEncoder(&b)
	enc.SetEscapeHTML(false)
	if err := enc.Encode(v); err != nil {
		return nil, err
	}
	return bytes.TrimSuffix(b.Bytes(), []byte("\n")), nil
}

// WithAttrs masks a payload given to logger.With as well, so no way of
// passing it escapes the mask; its unparsed bodies are warned about with the
// records of the logger it returns.
func (h *Handler) WithAttrs(attrs []slog.Attr) slog.Handler {
	kept := make([]slog.Attr, 0, len(attrs))
	bound := h.bound
	for _, a := range attrs {
		if a.Key != logging.PayloadKey {
			kept = append(kept, a)
		} else if h.keeps(false) {
			attr, found := h.payload(a.Value.Any())
			kept = append(kept, attr)
			bound = append(bound[:len(bound):len(bound)], found...)
		}
	}
	return &Handler{next: h.next.WithAttrs(kept), cfg: h.cfg, warned: h.warned, bound: bound}
}

func (h *Handler) WithGroup(name string) slog.Handler {
	return &Handler{next: h.next.WithGroup(name), cfg: h.cfg, warned: h.warned, bound: h.bound}
}
