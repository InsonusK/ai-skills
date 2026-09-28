package interfaces

import "time"

// Task is deferred work the domain asks for together with a data change: a
// data port that takes tasks writes them in the same transaction as the
// change, so both commit or neither does. The domain never sees how or where
// the task is stored.
type Task struct {
	Type    string    // stable task type name, e.g. "recheck-flagged-link"
	Payload any       // JSON-encoded by the adapter; its shape is owned by the task type
	Group   string    // tasks with the same Group run one at a time, in order; "" = unordered
	RunAt   time.Time // not before this time; zero = as soon as possible
}
