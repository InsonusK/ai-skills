// Package taskbox realizes the common TaskBox storage contract (VP-C003):
// deferred, retried, per-group ordered task execution. This package holds
// the store-independent part — task values, the handler registry, the
// outcome classification, and the worker loop; each store lives in its own
// subpackage (pgstore, redisstore) and implements Store.
package taskbox

import (
	"context"
	"encoding/json"
	"errors"
	"time"

	"github.com/google/uuid"
)

// Status is a task's lifecycle state (contract §4).
type Status string

const (
	StatusPending   Status = "pending"
	StatusRunning   Status = "running"
	StatusDone      Status = "done"
	StatusDead      Status = "dead"
	StatusCancelled Status = "cancelled"
)

// DefaultQueue is the queue a task goes to when none is given.
const DefaultQueue = "default"

// DefaultMaxAttempts is the contract's default for max_attempts.
const DefaultMaxAttempts = 10

// ErrNotFound is returned when no task has the given id.
var ErrNotFound = errors.New("taskbox: task not found")

// ErrNotDead is returned when requeue or cancel targets a task that is not dead.
var ErrNotDead = errors.New("taskbox: task is not dead")

// NewTask is what a caller enqueues (contract §2 enqueue parameters).
// Zero values take the contract defaults.
type NewTask struct {
	ID             uuid.UUID // zero: a new UUIDv7 is assigned
	StatusKey      uuid.UUID // Inbox tasks only; zero: none
	Queue          string
	Group          string // "" = no ordering with any other task
	Type           string
	Payload        json.RawMessage
	IdempotencyKey string
	MaxAttempts    int
	RunAt          time.Time     // zero: now
	Retention      time.Duration // zero: the service default
}

// Normalize fills the contract defaults and assigns the id.
func (t NewTask) Normalize() (NewTask, error) {
	if t.Type == "" {
		return t, errors.New("taskbox: task type is required")
	}
	if t.ID == uuid.Nil {
		id, err := uuid.NewV7()
		if err != nil {
			return t, err
		}
		t.ID = id
	}
	if t.Queue == "" {
		t.Queue = DefaultQueue
	}
	if t.MaxAttempts <= 0 {
		t.MaxAttempts = DefaultMaxAttempts
	}
	if len(t.Payload) == 0 {
		t.Payload = json.RawMessage(`{}`)
	}
	return t, nil
}

// Task is a stored task as read back from a store (contract §1).
type Task struct {
	Seq            int64
	ID             uuid.UUID
	StatusKey      uuid.UUID
	Queue          string
	Group          string
	Type           string
	Payload        json.RawMessage
	IdempotencyKey string
	Status         Status
	Attempt        int
	MaxAttempts    int
	RunAt          time.Time
	LockedUntil    time.Time
	LastStatus     int
	LastError      string
	Retention      time.Duration
	CreatedAt      time.Time
	UpdatedAt      time.Time
	FinishedAt     time.Time
}

// Claimed is a task a worker holds under a lease; Attempt is the fencing token.
type Claimed struct {
	Seq     int64
	ID      uuid.UUID
	Type    string
	Payload json.RawMessage
	Attempt int
}

// Result is the outcome a worker records for one claimed run.
type Result struct {
	Status     int           // HTTP status code of the outcome
	Error      string        // last_error text; "" keeps none
	Retryable  bool          // classified by Retryable
	RetryDelay time.Duration // max(backoff, Retry-After) when Retryable
}

// Store is one store's realization of the contract, as the worker and the
// dead-task operations use it. Enqueue is store-specific (it takes that
// store's transaction type) and lives on the concrete store only.
type Store interface {
	// Claim takes up to batch due tasks that head their group, running
	// them under a lease (contract §6 claim).
	Claim(ctx context.Context, queue string, lease time.Duration, batch int) ([]Claimed, error)
	// Finish records a claimed run's outcome; it is a no-op when the task is
	// no longer running under the claimed attempt (contract §4 fencing).
	Finish(ctx context.Context, c Claimed, r Result) error
	// Requeue sends a dead task back to pending with attempt 0.
	Requeue(ctx context.Context, id uuid.UUID) error
	// Cancel ends a dead task as cancelled; its group resumes.
	Cancel(ctx context.Context, id uuid.UUID) error
	// Cleanup removes finished tasks past their effective retention.
	Cleanup(ctx context.Context, defaultRetention time.Duration) error
	// Get reads one task by id.
	Get(ctx context.Context, id uuid.UUID) (Task, error)
}
