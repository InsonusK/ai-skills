package interfaces

import (
	"context"
	"time"
)

// LinkHistoryEntry is one durably-stored record of a checked URL.
type LinkHistoryEntry struct {
	Normalized string
	Flagged    bool
	Reason     string
	CheckedAt  time.Time
}

// LinkHistory is the outbound port for durable link-check storage.
type LinkHistory interface {
	Record(ctx context.Context, entry LinkHistoryEntry) error
	Recent(ctx context.Context, limit int) ([]LinkHistoryEntry, error)
}
