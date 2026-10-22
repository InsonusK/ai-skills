package endpoint

import (
	"strings"

	"github.com/spf13/cobra"
	"github.com/spf13/pflag"

	clipipeline "github.com/example/todo/internal/api/cli/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the items resource (`todo items ...`): its command handlers,
// its actions, and per action its flags, its output type, and its text form.
// A new command on items adds a field, an action line, and its bind, encode,
// and text functions here.

// ItemsHandler is the items resource: the command handlers its actions call.
// The composition root fills it; Resource lists its actions.
type ItemsHandler struct {
	PlanItems pipeline.Handler[command.PlanItemsCommand, command.PlanItemsResult]
}

// Resource is `todo items` with its actions.
func (h ItemsHandler) Resource() clipipeline.Resource {
	return clipipeline.Resource{Name: "items", Short: "Plan to-do items", Actions: []*cobra.Command{
		clipipeline.Command("plan", "Plan one open item per --title", h.PlanItems, bindPlanItems, encodePlanItems, textPlanItems),
	}}
}

// bindPlanItems declares the flags of `todo items plan`.
func bindPlanItems(flags *pflag.FlagSet) func() (command.PlanItemsCommand, error) {
	titles := flags.StringArray("title", nil, "the title of an item; repeat for several")
	return func() (command.PlanItemsCommand, error) {
		return command.PlanItemsCommand{Titles: *titles}, nil
	}
}

// planItemsOutput is the JSON line of `todo items plan`.
type planItemsOutput struct {
	Items []itemOutput `json:"items"`
}

type itemOutput struct {
	Title string `json:"title"`
	Done  bool   `json:"done"`
}

func encodePlanItems(result command.PlanItemsResult) any {
	items := make([]itemOutput, len(result.Items))
	for i, item := range result.Items {
		items[i] = itemOutput{Title: item.Title, Done: item.Done}
	}
	return planItemsOutput{Items: items}
}

// textPlanItems is `todo items plan --output text`: one checkbox line per item.
func textPlanItems(result command.PlanItemsResult) string {
	lines := make([]string, len(result.Items))
	for i, item := range result.Items {
		box := "[ ]"
		if item.Done {
			box = "[x]"
		}
		lines[i] = box + " " + item.Title
	}
	return strings.Join(lines, "\n")
}
