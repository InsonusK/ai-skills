package test

import (
	"context"
	"errors"
	"fmt"
	"slices"
	"time"

	"github.com/cucumber/godog"
	healthpb "google.golang.org/grpc/health/grpc_health_v1"
	reflectionpb "google.golang.org/grpc/reflection/grpc_reflection_v1"
	"google.golang.org/grpc/status"

	"github.com/example/todo/internal/api/health"
)

func registerHealthReflectionSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a readiness check "([^"]*)" that (passes|fails)$`, w.aReadinessCheck)
	sc.Step(`^I ask the health service about "([^"]*)"$`, w.iAskTheHealthService)
	sc.Step(`^the health status should be (SERVING|NOT_SERVING)$`, w.theHealthStatusShouldBe)
	sc.Step(`^the health call should fail with (\w+)$`, w.callShouldFailWith)
	sc.Step(`^reflection is enabled$`, w.reflectionIsEnabled)
	sc.Step(`^I list the services by reflection$`, w.iListTheServices)
	sc.Step(`^the listed services should include "([^"]*)"$`, w.theListedServicesShouldInclude)
	sc.Step(`^the reflection call should fail with (\w+)$`, w.callShouldFailWith)
}

func (w *World) aReadinessCheck(ctx context.Context, name, outcome string) error {
	var result error
	if outcome == "fails" {
		result = errors.New(name + " is down")
	}
	w.checks = append(w.checks, health.Check{Name: name, Check: func(context.Context) error { return result }})
	return nil
}

func (w *World) iAskTheHealthService(ctx context.Context, service string) error {
	conn, err := w.client()
	if err != nil {
		return err
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	resp, err := healthpb.NewHealthClient(conn).Check(ctx, &healthpb.HealthCheckRequest{Service: service})
	w.reply, w.callErr = resp.GetStatus().String(), err
	logf("when: Health.Check(%q) -> %s err=%v", service, w.reply, w.callErr)
	return nil
}

func (w *World) theHealthStatusShouldBe(ctx context.Context, want string) error {
	if w.callErr != nil || w.reply != want {
		return fmt.Errorf("health: got %s err=%v, want %s", w.reply, w.callErr, want)
	}
	return nil
}

func (w *World) callShouldFailWith(ctx context.Context, codeName string) error {
	want, err := codeNamed(codeName)
	if err != nil {
		return err
	}
	if got := status.Code(w.callErr); got != want {
		return fmt.Errorf("call error: got %v, want %s", w.callErr, want)
	}
	return nil
}

func (w *World) reflectionIsEnabled(ctx context.Context) error {
	w.cfg.ReflectionEnabled = true
	return nil
}

func (w *World) iListTheServices(ctx context.Context) error {
	conn, err := w.client()
	if err != nil {
		return err
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	stream, err := reflectionpb.NewServerReflectionClient(conn).ServerReflectionInfo(ctx)
	if err == nil {
		err = stream.Send(&reflectionpb.ServerReflectionRequest{
			MessageRequest: &reflectionpb.ServerReflectionRequest_ListServices{},
		})
	}
	var resp *reflectionpb.ServerReflectionResponse
	if err == nil {
		resp, err = stream.Recv()
	}
	w.callErr = err
	w.listed = nil
	for _, service := range resp.GetListServicesResponse().GetService() {
		w.listed = append(w.listed, service.GetName())
	}
	logf("when: reflection ListServices -> %v err=%v", w.listed, err)
	return nil
}

func (w *World) theListedServicesShouldInclude(ctx context.Context, name string) error {
	if w.callErr != nil || !slices.Contains(w.listed, name) {
		return fmt.Errorf("listed %v err=%v, want %q among them", w.listed, w.callErr, name)
	}
	return nil
}
