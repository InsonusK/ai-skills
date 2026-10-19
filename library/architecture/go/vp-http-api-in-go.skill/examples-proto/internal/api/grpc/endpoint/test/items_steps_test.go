package test

import (
	"context"
	"fmt"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc/status"

	"github.com/example/todo/gen/api/todo"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)"$`, w.planReturns)
	sc.Step(`^I call ItemsService.PlanItems with the titles "([^"]*)"$`, w.iCallPlanItems)
	sc.Step(`^the response should hold the items "([^"]*)", none done$`, w.theResponseShouldHold)
	sc.Step(`^PlanItems should have received the titles "([^"]*)"$`, w.planShouldHaveReceived)
	sc.Step(`^AddItem fails with the kind conflict$`, w.addConflicts)
	sc.Step(`^ListItems holds "([^"]*)"$`, w.listHolds)
	sc.Step(`^I call ItemsService.AddItem with the list "([^"]*)" and the title "([^"]*)"$`, w.iCallAddItem)
	sc.Step(`^I call ItemsService.ListItems with the list "([^"]*)"$`, w.iCallListItems)
	sc.Step(`^I call ItemsService.ClearList with the list "([^"]*)"$`, w.iCallClearList)
	sc.Step(`^AddItem should have received the list "([^"]*)" and the title "([^"]*)"$`, w.addShouldHaveReceived)
	sc.Step(`^the added item should be "([^"]*)"$`, w.theAddedItemShouldBe)
	sc.Step(`^the listed items should be "([^"]*)" for the list "([^"]*)"$`, w.theListedItemsShouldBe)
	sc.Step(`^the call should fail with (\w+) and the message "([^"]*)"$`, w.theCallShouldFailWith)
}

func (w *World) planReturns(ctx context.Context, titles string) error {
	w.planResult = command.PlanItemsResult{}
	for _, title := range strings.Split(titles, "|") {
		w.planResult.Items = append(w.planResult.Items, command.ItemView{Title: title})
	}
	return nil
}

func (w *World) iCallPlanItems(ctx context.Context, titles string) error {
	conn, err := w.client()
	if err != nil {
		return err
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	w.planResponse, err = todo.NewItemsServiceClient(conn).PlanItems(ctx, &todo.PlanItemsRequest{Titles: strings.Split(titles, "|")})
	logf("when: PlanItems(%q) -> %v err=%v", titles, w.planResponse, err)
	return err
}

func (w *World) theResponseShouldHold(ctx context.Context, titles string) error {
	got := []string{}
	for _, item := range w.planResponse.GetItems() {
		if item.GetDone() {
			return fmt.Errorf("item %q is done", item.GetTitle())
		}
		got = append(got, item.GetTitle())
	}
	if strings.Join(got, "|") != titles {
		return fmt.Errorf("items: got %q, want %q", strings.Join(got, "|"), titles)
	}
	return nil
}

func (w *World) planShouldHaveReceived(ctx context.Context, titles string) error {
	if got := strings.Join(w.planGot.Titles, "|"); !w.planCalled || got != titles {
		return fmt.Errorf("handler received %q (called=%v), want %q", got, w.planCalled, titles)
	}
	return nil
}

func (w *World) addConflicts(ctx context.Context) error {
	w.addErr = fmt.Errorf("%w: the title is taken", pipeline.ErrConflict)
	return nil
}

func (w *World) listHolds(ctx context.Context, titles string) error {
	w.listHeld = strings.Split(titles, "|")
	return nil
}

func (w *World) items() (todo.ItemsServiceClient, context.Context, context.CancelFunc, error) {
	conn, err := w.client()
	if err != nil {
		return nil, nil, nil, err
	}
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	return todo.NewItemsServiceClient(conn), ctx, cancel, nil
}

func (w *World) iCallAddItem(_ context.Context, listID, title string) error {
	client, ctx, cancel, err := w.items()
	if err != nil {
		return err
	}
	defer cancel()
	w.addResponse, w.callErr = client.AddItem(ctx, &todo.AddItemRequest{ListId: listID, Title: title})
	logf("when: AddItem(%q, %q) -> %v err=%v", listID, title, w.addResponse, w.callErr)
	return nil
}

func (w *World) iCallListItems(_ context.Context, listID string) error {
	client, ctx, cancel, err := w.items()
	if err != nil {
		return err
	}
	defer cancel()
	w.listResponse, w.callErr = client.ListItems(ctx, &todo.ListItemsRequest{ListId: listID})
	logf("when: ListItems(%q) -> %v err=%v", listID, w.listResponse, w.callErr)
	return nil
}

func (w *World) iCallClearList(_ context.Context, listID string) error {
	client, ctx, cancel, err := w.items()
	if err != nil {
		return err
	}
	defer cancel()
	_, w.callErr = client.ClearList(ctx, &todo.ClearListRequest{ListId: listID})
	logf("when: ClearList(%q) -> err=%v", listID, w.callErr)
	return nil
}

func (w *World) addShouldHaveReceived(ctx context.Context, listID, title string) error {
	if w.addGot != (command.AddItem{ListID: listID, Title: title}) {
		return fmt.Errorf("AddItem received %+v, want list %q title %q", w.addGot, listID, title)
	}
	return nil
}

func (w *World) theAddedItemShouldBe(ctx context.Context, title string) error {
	if w.callErr != nil || w.addResponse.GetItem().GetTitle() != title {
		return fmt.Errorf("added %v err=%v, want %q", w.addResponse, w.callErr, title)
	}
	return nil
}

func (w *World) theListedItemsShouldBe(ctx context.Context, titles, listID string) error {
	got := []string{}
	for _, item := range w.listResponse.GetItems() {
		got = append(got, item.GetTitle())
	}
	if w.callErr != nil || strings.Join(got, "|") != titles || w.listGot.ListID != listID {
		return fmt.Errorf("listed %q for %q err=%v, want %q for %q", got, w.listGot.ListID, w.callErr, titles, listID)
	}
	return nil
}

func (w *World) theCallShouldFailWith(ctx context.Context, codeName, message string) error {
	got := status.Convert(w.callErr)
	if w.callErr == nil || got.Code().String() != codeName || got.Message() != message {
		return fmt.Errorf("call error %v, want %s %q", w.callErr, codeName, message)
	}
	return nil
}
