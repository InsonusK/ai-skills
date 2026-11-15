package main

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"

	"github.com/cucumber/godog"
	"github.com/jackc/pgx/v5"
)

// The sample's end-to-end behavior, beside the shared persistence steps.
func registerCompletionSteps(sc *godog.ScenarioContext, app *app) {
	var id, version string
	type item struct {
		ID      string `json:"id"`
		Version string `json:"version"`
		Done    bool   `json:"done"`
	}
	readItem := func() (item, error) {
		var answer struct {
			Item item `json:"item"`
		}
		err := json.Unmarshal([]byte(app.body), &answer)
		return answer.Item, err
	}
	sc.Step(`^I keep the id and version of that item$`, func() error {
		answered, err := readItem()
		id, version = answered.ID, answered.Version
		if err != nil || id == "" || version == "" {
			return fmt.Errorf("add did not answer id/version: %s (%v)", app.body, err)
		}
		return nil
	})
	sc.Step(`^another connection renames that item$`, func(ctx context.Context) error {
		connection, err := pgx.Connect(ctx, app.database.DSN)
		if err != nil {
			return err
		}
		defer connection.Close(ctx)
		changed, err := connection.Exec(ctx, "UPDATE list_items SET title = title || ' (renamed)' WHERE id = $1", id)
		if err != nil {
			return err
		}
		if changed.RowsAffected() != 1 {
			return fmt.Errorf("rename affected %d rows", changed.RowsAffected())
		}
		return nil
	})
	sc.Step(`^I complete that item with the version I kept$`, func() error {
		body, _ := json.Marshal(map[string]string{"version": version})
		return app.send(http.MethodPost, "/v1/lists/home/items/"+id+"/complete", string(body))
	})
	sc.Step(`^I complete that item with the version "([^"]*)"$`, func(given string) error {
		body, _ := json.Marshal(map[string]string{"version": given})
		return app.send(http.MethodPost, "/v1/lists/home/items/"+id+"/complete", string(body))
	})
	sc.Step(`^the completed answer should carry a new version$`, func() error {
		answered, err := readItem()
		if err != nil || !answered.Done || answered.ID != id || answered.Version == "" || answered.Version == version {
			return fmt.Errorf("completion did not answer a new version: %s (%v)", app.body, err)
		}
		return nil
	})
	sc.Step(`^the call should be rejected as a concurrent change$`, func() error {
		var answer struct {
			Code int `json:"code"`
		}
		if err := json.Unmarshal([]byte(app.body), &answer); err != nil {
			return err
		}
		if app.status != http.StatusConflict || answer.Code != 10 {
			return fmt.Errorf("want 409 ABORTED, got %d %s", app.status, app.body)
		}
		return nil
	})
}
