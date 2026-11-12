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

	"github.com/example/todo/internal/logging"
)

// RequestIDKey is the attribute the request id of the call is written under.
const RequestIDKey = "request_id"

// NotParsedMessage is the WARN record written the first time a record of one
// message holds a body the mask could not read in one place of its payload.
const NotParsedMessage = "log payload not parsed"

// UnmaskedMessage is the WARN record written when the handler is built with
// Config.Unmasked, and then once per request - before its first record with a
// payload - and before every such record outside a request.
const UnmaskedMessage = "log payload unmasked"

// recentRequests is how many request ids the handler remembers having warned
// about unmasking; an older request may be warned about again.
const recentRequests = 4096

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
	// boundUnmasked: a payload given to logger.With was written unmasked.
	boundUnmasked bool
	// unmaskedRequests are the request ids already warned about unmasking.
	unmaskedRequests *recentIDs
}

// NewHandler is the handler the composition root builds in place of
// logging.NewHandler when the logging variation point is attached.
func NewHandler(w io.Writer, level slog.Leveler, cfg Config) *Handler {
	h := &Handler{
		next: slog.NewJSONHandler(w, &slog.HandlerOptions{Level: level}), cfg: cfg,
		warned: &sync.Map{}, unmaskedRequests: &recentIDs{seen: map[string]bool{}},
	}
	if cfg.Unmasked {
		_ = h.next.Handle(context.Background(), slog.NewRecord(time.Now(), slog.LevelWarn, UnmaskedMessage, 0))
	}
	return h
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
	unparsed, unmasked := h.bound, h.boundUnmasked
	r.Attrs(func(a slog.Attr) bool {
		if a.Key != logging.PayloadKey {
			out.AddAttrs(a)
		} else if h.keeps(failed) {
			attr, found := h.payload(a.Value.Any())
			out.AddAttrs(attr)
			unparsed = append(unparsed[:len(unparsed):len(unparsed)], found...)
			unmasked = unmasked || h.cfg.Unmasked
		}
		return true
	})
	if unmasked {
		if err := h.warnUnmasked(ctx); err != nil {
			return err
		}
	}
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

// warnUnmasked writes a WARN record before the first record of a request
// that carries an unmasked payload, and before every such record outside a
// request (with no request id), so nobody reads an unmasked log unwarned.
func (h *Handler) warnUnmasked(ctx context.Context) error {
	if id := logging.RequestID(ctx); id != "" && !h.unmaskedRequests.add(id) {
		return nil
	}
	w := slog.NewRecord(time.Now(), slog.LevelWarn, UnmaskedMessage, 0)
	addRequestID(ctx, &w)
	return h.next.Handle(ctx, w)
}

// recentIDs remembers the last recentRequests ids added, oldest out first.
type recentIDs struct {
	mu    sync.Mutex
	seen  map[string]bool
	order []string
}

// add reports whether id is new, remembering it.
func (r *recentIDs) add(id string) bool {
	r.mu.Lock()
	defer r.mu.Unlock()
	if r.seen[id] {
		return false
	}
	r.seen[id] = true
	r.order = append(r.order, id)
	if len(r.order) > recentRequests {
		delete(r.seen, r.order[0])
		r.order = r.order[1:]
	}
	return true
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
	var masked any
	var unparsed []Unparsed
	if h.cfg.Unmasked {
		masked = Unmask(v)
	} else {
		masked, unparsed = Mask(v)
	}
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
	bound, boundUnmasked := h.bound, h.boundUnmasked
	for _, a := range attrs {
		if a.Key != logging.PayloadKey {
			kept = append(kept, a)
		} else if h.keeps(false) {
			attr, found := h.payload(a.Value.Any())
			kept = append(kept, attr)
			bound = append(bound[:len(bound):len(bound)], found...)
			boundUnmasked = boundUnmasked || h.cfg.Unmasked
		}
	}
	derived := *h
	derived.next, derived.bound, derived.boundUnmasked = h.next.WithAttrs(kept), bound, boundUnmasked
	return &derived
}

func (h *Handler) WithGroup(name string) slog.Handler {
	derived := *h
	derived.next = h.next.WithGroup(name)
	return &derived
}
