package test

import (
	"context"
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/linkcheck/internal/linkcheck"
	"reflect"
	"slices"
)

func registerContractSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a check result has the fields:$`, func(ctx context.Context, table *godog.Table) error {
		typ := reflect.TypeOf(linkcheck.Result{})
		got := []string{}
		for i := 0; i < typ.NumField(); i++ {
			got = append(got, typ.Field(i).Tag.Get("json"))
		}
		want := []string{}
		for _, r := range table.Rows[1:] {
			want = append(want, r.Cells[0].Value)
		}
		slices.Sort(got)
		slices.Sort(want)
		logf("fields=%q want=%q", got, want)
		if !slices.Equal(got, want) {
			return fmt.Errorf("fields got %q want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the error code is empty$`, func(ctx context.Context) error {
		logf("error code=%q", w.result.ErrorCode)
		if w.result.ErrorCode != "" {
			return fmt.Errorf("error code is %q", w.result.ErrorCode)
		}
		return nil
	})

}
