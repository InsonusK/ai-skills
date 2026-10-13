// Package tasks is the TaskBox inbound adapter: one handler per task type,
// each decoding its payload, calling the domain service, and mapping the
// result to an HTTP status code — no business logic, exactly like the HTTP
// and gRPC adapters.
package tasks

import (
	"context"
	"encoding/json"
	"errors"
	"net/http"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
	"github.com/example/linkcheck-service/internal/domain/services"
	"github.com/example/linkcheck-service/internal/taskbox"
)

// Handlers adapts TaskBox deliveries to the domain service.
type Handlers struct {
	svc *services.LinkCheckService
}

func New(svc *services.LinkCheckService) *Handlers {
	return &Handlers{svc: svc}
}

// Register registers every task type this service handles.
func (h *Handlers) Register(r *taskbox.Registry) {
	r.Register(services.TaskRecheckFlaggedLink, h.recheckFlaggedLink)
}

func (h *Handlers) recheckFlaggedLink(ctx context.Context, d taskbox.Delivery) (taskbox.Outcome, error) {
	var p services.RecheckFlaggedLink
	if err := json.Unmarshal(d.Payload, &p); err != nil || p.URL == "" {
		return taskbox.Outcome{Status: http.StatusBadRequest}, nil
	}
	_, err := h.svc.Recheck(ctx, p.URL)
	switch {
	case err == nil:
		return taskbox.OK, nil
	case errors.Is(err, services.ErrInvalidURL):
		return taskbox.Outcome{Status: http.StatusBadRequest}, nil
	case errors.Is(err, interfaces.ErrUnavailable):
		return taskbox.Outcome{Status: http.StatusServiceUnavailable}, nil
	default:
		return taskbox.Outcome{}, err
	}
}
