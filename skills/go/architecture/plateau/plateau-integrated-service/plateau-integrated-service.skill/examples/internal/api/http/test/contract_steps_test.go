package test

import (
	"context"
	"encoding/json"
	"fmt"
	"slices"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck-service/docs/integration/api/linkcheck"
)

func registerContractSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the HTTP API and its OpenAPI contract both have exactly the operations:$`,
		func(ctx context.Context, table *godog.Table) error {
			want := []string{}
			for _, row := range table.Rows[1:] {
				want = append(want, row.Cells[0].Value+" "+row.Cells[1].Value)
			}
			slices.Sort(want)

			served := w.server.ContractRoutes()
			slices.Sort(served)

			described, err := specOperations(linkcheck.Spec)
			if err != nil {
				return err
			}

			logf("then: expected=%q served=%q described=%q", want, served, described)
			if !slices.Equal(served, want) || !slices.Equal(described, want) {
				return fmt.Errorf("operations differ\n  expected:  %q\n  served:    %q\n  described: %q", want, served, described)
			}
			return nil
		})
}

// specOperations lists the operations of an OpenAPI document as sorted "METHOD /path" patterns.
func specOperations(spec []byte) ([]string, error) {
	var doc struct {
		Paths map[string]map[string]json.RawMessage `json:"paths"`
	}
	if err := json.Unmarshal(spec, &doc); err != nil {
		return nil, fmt.Errorf("the embedded contract is not JSON: %w", err)
	}
	methods := []string{"get", "put", "post", "delete", "options", "head", "patch"}
	operations := []string{}
	for path, item := range doc.Paths {
		for key := range item {
			if slices.Contains(methods, key) {
				operations = append(operations, strings.ToUpper(key)+" "+path)
			}
		}
	}
	slices.Sort(operations)
	return operations, nil
}
