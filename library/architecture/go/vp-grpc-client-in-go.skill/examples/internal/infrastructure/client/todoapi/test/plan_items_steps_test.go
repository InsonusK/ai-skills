package test

import (
	"fmt"
	"strings"

	"google.golang.org/protobuf/encoding/protojson"
	"google.golang.org/protobuf/proto"

	"github.com/example/todo/gen/client/todo"
)

func splitTitles(titles string) []string { return strings.Split(titles, "|") }

// shouldHaveReceived compares the request the todo API received with the
// one given as JSON, as messages: protojson output is not stable text.
func (w *World) shouldHaveReceived(method, request string) error {
	want := &todo.PlanItemsRequest{}
	if err := protojson.Unmarshal([]byte(request), want); err != nil {
		return err
	}
	if w.method != method || !proto.Equal(w.received, want) {
		return fmt.Errorf("received %s %v, want %s %v", w.method, w.received, method, want)
	}
	return nil
}

func (w *World) itemsShouldBe(want string) error {
	var got []string
	for _, item := range w.items {
		state := "open"
		if item.Done {
			state = "done"
		}
		got = append(got, item.Title+":"+state)
	}
	if w.callErr != nil || strings.Join(got, "|") != want {
		return fmt.Errorf("items %v, error %v; want %s", got, w.callErr, want)
	}
	return nil
}
