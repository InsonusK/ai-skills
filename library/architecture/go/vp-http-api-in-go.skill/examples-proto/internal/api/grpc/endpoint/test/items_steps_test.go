package test

import (
	"context"
	"fmt"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"github.com/example/todo/gen/api/todo"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/shared/errorkind"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)" over gRPC$`, w.planReturns)
	sc.Step(`^AddItem returns the item "([^"]*)" over gRPC$`, w.addReturns)
	sc.Step(`^AddItem fails with the kind "([^"]*)" over gRPC$`, w.addFailsWithKind)
	sc.Step(`^ListItems returns the items "([^"]*)" over gRPC$`, w.listHolds)
	sc.Step(`^I call ItemsService.PlanItems with the titles "([^"]*)"$`, w.iCallPlanItems)
	sc.Step(`^I call ItemsService.PlanItems with no titles$`, w.iCallPlanItemsWithNoTitles)
	sc.Step(`^I call ItemsService.AddItem with the list "([^"]*)" and the title "([^"]*)"$`, w.iCallAddItem)
	sc.Step(`^I call ItemsService.ListItems with the list "([^"]*)"$`, w.iCallListItems)
	sc.Step(`^I call ItemsService.ClearList with the list "([^"]*)"$`, w.iCallClearList)
	sc.Step(`^the gRPC response should hold the items "([^"]*)", none done$`, w.theResponseShouldHold)
	sc.Step(`^the gRPC response should hold the item "([^"]*)"$`, w.theAddedItemShouldBe)
	sc.Step(`^PlanItems should have received the titles "([^"]*)" over gRPC$`, w.planShouldHaveReceived)
	sc.Step(`^PlanItems should have received no titles over gRPC$`, w.planShouldHaveReceivedNoTitles)
	sc.Step(`^AddItem should have received the list "([^"]*)" and the title "([^"]*)" over gRPC$`, w.addShouldHaveReceived)
	sc.Step(`^ListItems should have received the list "([^"]*)" over gRPC$`, w.listShouldHaveReceived)
	sc.Step(`^the gRPC call of ItemsService should fail with ([A-Z_]+)$`, w.theCallShouldFailWithTheCode)
	sc.Step(`^the gRPC call of ItemsService should fail with ([A-Z_]+) and the message "([^"]*)"$`, w.theCallShouldFailWith)
}

func (w *World) planReturns(ctx context.Context, titles string) error {
	w.planResult = command.PlanItemsResult{}
	if titles == "" {
		return nil
	}
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
	items := w.planResponse.GetItems()
	if w.listResponse != nil {
		items = w.listResponse.GetItems()
	}
	for _, item := range items {
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

// addReturns: the fake AddItem answers the title it received.
func (w *World) addReturns(ctx context.Context, title string) error {
	w.addErr = nil
	return nil
}

func (w *World) addFailsWithKind(ctx context.Context, name string) error {
	for _, kind := range errorkind.Kinds {
		if kind.Error() == name {
			w.addErr = errorkind.New(kind, "the title is taken")
			return nil
		}
	}
	return fmt.Errorf("no kind %q", name)
}

func (w *World) iCallPlanItemsWithNoTitles(ctx context.Context) error {
	client, ctx, cancel, err := w.items()
	if err != nil {
		return err
	}
	defer cancel()
	w.planResponse, err = client.PlanItems(ctx, &todo.PlanItemsRequest{})
	return err
}

func (w *World) planShouldHaveReceivedNoTitles(ctx context.Context) error {
	if !w.planCalled || len(w.planGot.Titles) != 0 {
		return fmt.Errorf("handler received %q (called=%v), want no titles", w.planGot.Titles, w.planCalled)
	}
	return nil
}

func (w *World) listShouldHaveReceived(ctx context.Context, listID string) error {
	if w.listGot.ListID != listID {
		return fmt.Errorf("ListItems received %+v, want list %q", w.listGot, listID)
	}
	return nil
}

// codeNamed is the code the scenarios name by its canonical name (INVALID_ARGUMENT).
func codeNamed(name string) (codes.Code, error) {
	var code codes.Code
	err := code.UnmarshalJSON([]byte(`"` + name + `"`))
	return code, err
}

func (w *World) theCallShouldFailWithTheCode(ctx context.Context, codeName string) error {
	want, err := codeNamed(codeName)
	if err != nil {
		return err
	}
	if got := status.Code(w.callErr); w.callErr == nil || got != want {
		return fmt.Errorf("call error %v, want %s", w.callErr, codeName)
	}
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
	if w.addGot != (command.AddItemCommand{ListID: listID, Title: title}) {
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

func (w *World) theCallShouldFailWith(ctx context.Context, codeName, message string) error {
	want, err := codeNamed(codeName)
	if err != nil {
		return err
	}
	got := status.Convert(w.callErr)
	if w.callErr == nil || got.Code() != want || got.Message() != message {
		return fmt.Errorf("call error %v, want %s %q", w.callErr, codeName, message)
	}
	return nil
}
