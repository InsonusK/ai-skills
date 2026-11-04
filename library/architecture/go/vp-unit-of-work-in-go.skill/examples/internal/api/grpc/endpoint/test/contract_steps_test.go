package test

import (
	"context"
	"fmt"
	"slices"

	"github.com/cucumber/godog"

	"github.com/example/todo/gen/api/todo"
	grpcapi "github.com/example/todo/internal/api/grpc"
)

func registerContractSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the gRPC API and its proto contract both have exactly the services:$`, w.bothHaveExactly)
}

// bothHaveExactly compares, as sets, the services of the table with the
// services endpoint.Services serves and with the services of the proto file.
// The generated server interfaces already hold every method of a served
// service; what they cannot hold is a service left out of Services.
func (w *World) bothHaveExactly(ctx context.Context, table *godog.Table) error {
	want := []string{}
	for _, row := range table.Rows[1:] {
		want = append(want, row.Cells[0].Value)
	}
	served := []string{}
	for _, service := range grpcapi.Services(w.handlers()) {
		served = append(served, service.Desc.ServiceName)
	}
	contract := []string{}
	services := todo.File_todo_todo_proto.Services()
	for i := range services.Len() {
		contract = append(contract, string(services.Get(i).FullName()))
	}
	slices.Sort(want)
	slices.Sort(served)
	slices.Sort(contract)
	logf("then: table=%v served=%v contract=%v", want, served, contract)
	if !slices.Equal(want, served) || !slices.Equal(want, contract) {
		return fmt.Errorf("services differ:\n table:    %v\n served:   %v\n contract: %v", want, served, contract)
	}
	return nil
}
