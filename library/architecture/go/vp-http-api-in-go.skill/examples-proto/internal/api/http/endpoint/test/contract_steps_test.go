package test

import (
	"context"
	"encoding/json"
	"fmt"
	"slices"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/docs/integration/api/todo"
	httpapi "github.com/example/todo/internal/api/http"
)

func registerContractSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the HTTP API should serve exactly the operations its OpenAPI contract describes$`, func(ctx context.Context) error { return w.bothHaveExactly(ctx, nil) })
	sc.Step(`^the HTTP API and its OpenAPI contract both have exactly the operations:$`, w.bothHaveExactly)
}

// bothHaveExactly compares served routes with the embedded spec, and with an
// explicit application operation list when the example supplies a table.
func (w *World) bothHaveExactly(ctx context.Context, table *godog.Table) error {
	want := []string{}
	if table != nil {
		for _, row := range table.Rows[1:] {
			want = append(want, row.Cells[0].Value+" "+row.Cells[1].Value)
		}
	}
	served := []string{}
	for _, route := range httpapi.Routes(w.handlers()) {
		served = append(served, route.Pattern)
	}
	spec, err := specOperations(todo.Spec)
	if err != nil {
		return err
	}
	if table == nil {
		want = slices.Clone(served)
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
