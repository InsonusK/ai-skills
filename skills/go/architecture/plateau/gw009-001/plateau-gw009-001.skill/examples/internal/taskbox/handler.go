package taskbox

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"sync"
	"time"

	"github.com/google/uuid"
)

// Delivery is what a handler receives for one run (contract §2).
type Delivery struct {
	ID      uuid.UUID
	Type    string
	Payload json.RawMessage
	Attempt int
}

// Outcome is a handler's answer: an HTTP status code (2xx = success) and an
// optional Retry-After.
type Outcome struct {
	Status     int
	RetryAfter time.Duration
}

// OK is the success outcome.
var OK = Outcome{Status: http.StatusOK}

// Handler runs one task. A returned error counts as status 500 (contract §2);
// ctx ends when the task's lease ends, and the handler must honour it.
type Handler func(ctx context.Context, d Delivery) (Outcome, error)

// Registry maps task types to handlers. Safe for concurrent use.
type Registry struct {
	mu       sync.RWMutex
	handlers map[string]Handler
}

func NewRegistry() *Registry {
	return &Registry{handlers: map[string]Handler{}}
}

// Register sets the handler for taskType, replacing any earlier one.
func (r *Registry) Register(taskType string, h Handler) {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.handlers[taskType] = h
}

// Unregister removes the handler for taskType.
func (r *Registry) Unregister(taskType string) {
	r.mu.Lock()
	defer r.mu.Unlock()
	delete(r.handlers, taskType)
}

func (r *Registry) lookup(taskType string) (Handler, bool) {
	r.mu.RLock()
	defer r.mu.RUnlock()
	h, ok := r.handlers[taskType]
	return h, ok
}

// Retryable reports whether an outcome status may succeed on a later
// attempt, by VP-C004's retry classification; every TaskBox handler is
// idempotent, so 500 is retryable.
func Retryable(status int) bool {
	switch status {
	case http.StatusRequestTimeout, http.StatusTooManyRequests, http.StatusInternalServerError,
		http.StatusBadGateway, http.StatusServiceUnavailable, http.StatusGatewayTimeout:
		return true
	}
	return false
}

// Success reports whether status is 2xx.
func Success(status int) bool { return status >= 200 && status < 300 }

// Backoff is the contract's min(base × 2^attempt, cap).
func Backoff(base, cap time.Duration, attempt int) time.Duration {
	d := base
	for i := 0; i < attempt; i++ {
		d *= 2
		if d >= cap {
			return cap
		}
	}
	return min(d, cap)
}

// resultOf classifies one run's outcome into what the store records.
func resultOf(out Outcome, err error, attempt int, cfg Config) Result {
	if err != nil {
		out = Outcome{Status: http.StatusInternalServerError}
	}
	r := Result{Status: out.Status}
	if err != nil {
		r.Error = err.Error()
	} else if !Success(out.Status) {
		r.Error = fmt.Sprintf("handler answered %d", out.Status)
	}
	if Retryable(out.Status) {
		r.Retryable = true
		r.RetryDelay = max(Backoff(cfg.BackoffBase, cfg.BackoffCap, attempt), out.RetryAfter)
	}
	return r
}
