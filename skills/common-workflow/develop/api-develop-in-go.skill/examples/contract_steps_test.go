// Illustration for skill api-develop-in-go: the step of the scenario that holds the served
// routes and the embedded contract against one table. Module path and package names are
// those of the sample service.
package test

import (
	"context"
	"encoding/json"
	"fmt"
	"slices"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck/docs/integration/api/openapi"
	apihttp "github.com/example/linkcheck/internal/api/http"
)

func registerContractSteps(sc *godog.ScenarioContext) {
	sc.Step(`^the HTTP API and its OpenAPI contract both have exactly the operations:$`,
		func(ctx context.Context, table *godog.Table) error {
			want := []string{}
			for _, row := range table.Rows[1:] {
				want = append(want, row.Cells[0].Value+" "+row.Cells[1].Value)
			}
			slices.Sort(want)

			served := apihttp.New().ContractRoutes()
			slices.Sort(served)

			described, err := specOperations(openapi.Spec)
			if err != nil {
				return err
			}

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
