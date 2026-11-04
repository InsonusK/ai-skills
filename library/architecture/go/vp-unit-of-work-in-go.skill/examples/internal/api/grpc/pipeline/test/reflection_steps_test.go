package test

import (
	"context"
	"fmt"
	"slices"
	"time"

	"github.com/cucumber/godog"
	reflectionpb "google.golang.org/grpc/reflection/grpc_reflection_v1"
	"google.golang.org/grpc/status"
)

func registerReflectionSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^reflection is enabled$`, w.reflectionIsEnabled)
	sc.Step(`^I list the services by reflection$`, w.iListTheServices)
	sc.Step(`^the listed services should include "([^"]*)"$`, w.theListedServicesShouldInclude)
	sc.Step(`^the reflection call should fail with (\w+)$`, w.callShouldFailWith)
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
