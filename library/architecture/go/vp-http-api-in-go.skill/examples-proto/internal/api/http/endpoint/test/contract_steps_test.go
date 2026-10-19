package test

import (
	"context"
	"encoding/json"
	"fmt"
	"slices"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/docs/integration/api/todo"
	"github.com/example/todo/internal/api/http/endpoint"
)

func registerContractSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the HTTP API and its OpenAPI contract both have exactly the operations:$`, w.bothHaveExactly)
}

// bothHaveExactly compares, as sets, the operations of the table with the
// routes the server serves and with the operations of the embedded spec.
func (w *World) bothHaveExactly(ctx context.Context, table *godog.Table) error {
	want := []string{}
	for _, row := range table.Rows[1:] {
		want = append(want, row.Cells[0].Value+" "+row.Cells[1].Value)
	}
	served := []string{}
	for _, route := range endpoint.Routes(w.handlers()) {
		served = append(served, route.Pattern)
	}
	spec, err := specOperations(todo.Spec)
	if err != nil {
		return err
	}
	slices.Sort(want)
	slices.Sort(served)
	slices.Sort(spec)
	logf("then: table=%v served=%v spec=%v", want, served, spec)
	if !slices.Equal(want, served) || !slices.Equal(want, spec) {
		return fmt.Errorf("operations differ:\n table:  %v\n served: %v\n spec:   %v", want, served, spec)
	}
	return nil
}

func specOperations(raw []byte) ([]string, error) {
	var doc struct {
		Paths map[string]map[string]json.RawMessage `json:"paths"`
	}
	if err := json.Unmarshal(raw, &doc); err != nil {
		return nil, fmt.Errorf("embedded spec: %w", err)
	}
	ops := []string{}
	for path, methods := range doc.Paths {
		for method := range methods {
			ops = append(ops, strings.ToUpper(method)+" "+path)
		}
	}
	return ops, nil
}
