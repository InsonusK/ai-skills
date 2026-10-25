package test

import (
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/middleware/access"
	"github.com/example/todo/internal/command/pipeline"
)

func registerAccessCheckSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the caller "([^"]*)" with the roles "([^"]*)"$`, w.theCaller)
	sc.Step(`^no caller$`, func() error { return nil })
	sc.Step(`^a command restricted to "([^"]*)" goes through the access check$`, func(role string) error {
		handle(w, access.Middleware(), restrictedCommand{role: role})
		return nil
	})
	sc.Step(`^a command without a mark goes through the access check$`, func() error {
		handle(w, access.Middleware(), plainCommand{})
		return nil
	})
	sc.Step(`^the handler should have run$`, w.theHandlerShouldHaveRun)
	sc.Step(`^the handler should not have run$`, w.theHandlerShouldNotHaveRun)
	sc.Step(`^the call should be rejected as (unauthenticated|permission denied) with "([^"]*)"$`, w.theCallShouldBeRejected)
}

// theCaller puts the caller into the call's context, as an API middleware does.
func (w *World) theCaller(user, roles string) error {
	identity := access.Identity{User: user}
	if roles != "" {
		identity.Roles = strings.Split(roles, ",")
	}
	w.ctx = access.WithIdentity(w.ctx, identity)
	return nil
}

func (w *World) theHandlerShouldHaveRun() error {
	if !w.handled || w.err != nil {
		return fmt.Errorf("handled=%v err=%v, want the handler run without an error", w.handled, w.err)
	}
	return nil
}

func (w *World) theHandlerShouldNotHaveRun() error {
	if w.handled {
		return errors.New("the handler ran")
	}
	return nil
}

var kinds = map[string]error{
	"unauthenticated":   pipeline.ErrUnauthenticated,
	"permission denied": pipeline.ErrPermissionDenied,
}

func (w *World) theCallShouldBeRejected(kind, message string) error {
	if !errors.Is(w.err, kinds[kind]) || w.err.Error() != message {
		return fmt.Errorf("error: got %v, want %s %q", w.err, kind, message)
	}
	return nil
}
