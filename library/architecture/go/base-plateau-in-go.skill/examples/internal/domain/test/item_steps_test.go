package test

import (
	"context"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/domain"
)

func registerItemSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I create an item titled "([^"]*)"$`, w.iCreateAnItemTitled)
	sc.Step(`^I create an item with a title of (\d+) characters$`, w.iCreateAnItemWithATitleOf)
	sc.Step(`^the item outcome should be "([^"]*)"$`, w.theItemOutcomeShouldBe)
	sc.Step(`^the item title should be "([^"]*)"$`, w.theItemTitleShouldBe)
	sc.Step(`^an open item titled "([^"]*)"$`, w.anOpenItemTitled)
	sc.Step(`^the item is completed$`, w.theItemIsCompleted)
	sc.Step(`^I complete the item$`, w.iCompleteTheItem)
	sc.Step(`^the item should be done$`, w.theItemShouldBeDone)
	sc.Step(`^completing should fail with "([^"]*)"$`, w.completingShouldFailWith)
}

func (w *World) iCreateAnItemTitled(ctx context.Context, title string) error {
	w.item, w.itemErr = domain.NewItem(title)
	logf("when: NewItem(%q) -> title=%q err=%v", title, w.item.Title(), w.itemErr)
	return nil
}

func (w *World) iCreateAnItemWithATitleOf(ctx context.Context, n int) error {
	return w.iCreateAnItemTitled(ctx, strings.Repeat("я", n))
}

func (w *World) theItemOutcomeShouldBe(ctx context.Context, outcome string) error {
	logf("then: err=%v want=%s", w.itemErr, outcome)
	if outcome == "created" {
		if w.itemErr != nil {
			return fmt.Errorf("item not created: %v", w.itemErr)
		}
		return nil
	}
	return expectReason(w.itemErr, outcome)
}

func (w *World) theItemTitleShouldBe(ctx context.Context, want string) error {
	logf("then: title=%q want=%q", w.item.Title(), want)
	if w.item.Title() != want {
		return fmt.Errorf("title: got %q, want %q", w.item.Title(), want)
	}
	return nil
}

func (w *World) anOpenItemTitled(ctx context.Context, title string) error {
	item, err := domain.NewItem(title)
	if err != nil {
		return fmt.Errorf("given item: %w", err)
	}
	w.item = item
	return nil
}

func (w *World) theItemIsCompleted(ctx context.Context) error {
	return w.item.Complete()
}

func (w *World) iCompleteTheItem(ctx context.Context) error {
	w.completeErr = w.item.Complete()
	logf("when: Complete() -> done=%v err=%v", w.item.Done(), w.completeErr)
	return nil
}

func (w *World) theItemShouldBeDone(ctx context.Context) error {
	if w.completeErr != nil || !w.item.Done() {
		return fmt.Errorf("item not done: done=%v err=%v", w.item.Done(), w.completeErr)
	}
	return nil
}

func (w *World) completingShouldFailWith(ctx context.Context, reason string) error {
	return expectReason(w.completeErr, reason)
}
