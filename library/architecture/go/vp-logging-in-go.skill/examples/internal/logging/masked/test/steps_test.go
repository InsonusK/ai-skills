package test

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"log/slog"
	"os"
	"reflect"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/bufbuild/protocompile"
	"github.com/cucumber/godog"
	"google.golang.org/protobuf/encoding/protojson"
	"google.golang.org/protobuf/encoding/protowire"
	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/reflect/protoreflect"
	"google.golang.org/protobuf/reflect/protoregistry"
	"google.golang.org/protobuf/types/dynamicpb"
	"google.golang.org/protobuf/types/known/anypb"

	"github.com/example/todo/internal/logging"
	"github.com/example/todo/internal/logging/masked"
)

var variables = []string{"LOG_PAYLOAD", "LOG_PAYLOAD_MAX_BYTES"}

// The types masking.feature gives as a payload.

type person struct {
	Name  string `json:"name"`
	PIN   string `json:"pin" log:"mask"`
	Note  string `json:"note" log:"omit"`
	Draft string `json:"-"`
}

type vault struct {
	Code string `json:"code"`
}

type team struct {
	Title   string   `json:"title"`
	Lead    person   `json:"lead"`
	Members []person `json:"members"`
	Vault   vault    `json:"vault" log:"mask"`
}

type login struct {
	User     string `json:"user"`
	Password string `json:"password"`
}

type account struct {
	User     string `json:"user"`
	Password string `json:"p"`
}

type upload struct {
	Name    string `json:"name"`
	Content []byte `json:"content"`
}

type link struct {
	Name string `json:"name"`
	Next *link  `json:"next"`
}

// protoMessages are the messages of the shared masking.proto, compiled from
// the copy beside the scenarios and registered, so an Any of them unpacks as a
// generated message's would.
var protoMessages = sync.OnceValues(func() (protoreflect.MessageDescriptors, error) {
	compiler := protocompile.Compiler{
		Resolver: protocompile.WithStandardImports(&protocompile.SourceResolver{ImportPaths: []string{"../features"}}),
	}
	files, err := compiler.Compile(context.Background(), "masking.proto")
	if err != nil {
		return nil, err
	}
	messages := files[0].Messages()
	for i := range messages.Len() {
		if err := protoregistry.GlobalTypes.RegisterMessage(dynamicpb.NewMessageType(messages.Get(i))); err != nil {
			return nil, err
		}
	}
	return messages, nil
})

func newMessage(name string) (*dynamicpb.Message, error) {
	messages, err := protoMessages()
	if err != nil {
		return nil, err
	}
	desc := messages.ByName(protoreflect.Name(name))
	if desc == nil {
		return nil, fmt.Errorf("masking.proto has no message %s", name)
	}
	return dynamicpb.NewMessage(desc), nil
}

// nested is levels maps or lists, each holding the next, the innermost the
// text "end".
func nested(shape string, levels int) any {
	var v any = "end"
	for range levels {
		if shape == "maps" {
			v = map[string]any{"next": v}
		} else {
			v = []any{v}
		}
	}
	return v
}

// nestedNodes is levels Node messages as proto JSON, each in the field of the
// one before, the innermost with the title "end".
func nestedNodes(field string, levels int) string {
	v := `{"title":"end"}`
	for range levels - 1 {
		switch field {
		case "next":
			v = `{"next":` + v + `}`
		case "children":
			v = `{"children":[` + v + `]}`
		default:
			v = `{"named":{"n":` + v + `}}`
		}
	}
	return v
}

// withoutComments drops what follows "//" outside a JSON string on each line
// of a doc string: the scenarios say there what is special about a field.
func withoutComments(text string) string {
	lines := strings.Split(text, "\n")
	for n, line := range lines {
		inString, escaped := false, false
		for i := 0; i < len(line); i++ {
			switch c := line[i]; {
			case escaped:
				escaped = false
			case inString && c == '\\':
				escaped = true
			case c == '"':
				inString = !inString
			case !inString && c == '/' && i+1 < len(line) && line[i+1] == '/':
				line, i = line[:i], len(line)
			}
		}
		lines[n] = strings.TrimRight(line, " ")
	}
	return strings.Join(lines, "\n")
}

// fixture builds the type a scenario names from the JSON of its fields; a
// field left out of the type's JSON form (Person.draft) or a body
// (Upload.content) is set by its name.
func fixture(name, given string) (any, error) {
	var value any
	var fields map[string]any
	if err := json.Unmarshal([]byte(given), &fields); err != nil {
		return nil, err
	}
	switch name {
	case "Upload":
		// content is a body: the scenario gives its text, json would read base64
		content, _ := fields["content"].(string)
		name, _ := fields["name"].(string)
		return upload{Name: name, Content: []byte(content)}, nil
	case "Person":
		value = &person{}
	case "Team":
		value = &team{}
	case "Account":
		value = &account{}
	default:
		value = &login{}
	}
	if err := json.Unmarshal([]byte(given), value); err != nil {
		return nil, err
	}
	if p, ok := value.(*person); ok {
		p.Draft, _ = fields["draft"].(string)
	}
	return value, nil
}

// encode writes JSON as the handler does, "<" and ">" as they are.
func encode(v any) string {
	var b bytes.Buffer
	enc := json.NewEncoder(&b)
	enc.SetEscapeHTML(false)
	_ = enc.Encode(v)
	return strings.TrimSuffix(b.String(), "\n")
}

// sameJSON compares two JSON texts by what they say, not by key order.
func sameJSON(got, want string) bool {
	var g, w any
	return json.Unmarshal([]byte(got), &g) == nil && json.Unmarshal([]byte(want), &w) == nil && reflect.DeepEqual(g, w)
}

func registerSteps(sc *godog.ScenarioContext) {
	var rendered string
	var unparsed []masked.Unparsed
	var out bytes.Buffer
	var logger *slog.Logger
	var ctx context.Context
	var cfg masked.Config
	var level slog.Level
	var loadErr error
	sc.Before(func(c context.Context, _ *godog.Scenario) (context.Context, error) {
		rendered, unparsed, logger, ctx, level = "", nil, nil, context.Background(), slog.LevelInfo
		out.Reset()
		cfg = masked.Config{Payload: masked.PayloadAlways, MaxBytes: 4096}
		for _, name := range variables {
			_ = os.Unsetenv(name)
		}
		return c, nil
	})

	// masking.feature

	render := func(v any) error {
		var value any
		value, unparsed = masked.Mask(v)
		rendered = encode(value)
		fmt.Printf("when: Mask -> %s, unparsed %v\n", rendered, unparsed)
		return nil
	}
	sc.Step(`^the payload is an? (Person|Team|Login|Account|Upload):$`, func(name string, given *godog.DocString) error {
		value, err := fixture(name, withoutComments(given.Content))
		if err != nil {
			return err
		}
		return render(value)
	})
	sc.Step(`^the payload is the body:$`, func(given *godog.DocString) error {
		return render([]byte(strings.TrimSpace(withoutComments(given.Content))))
	})
	sc.Step(`^the payload is a map:$`, func(given *godog.DocString) error {
		var entries map[string]any
		if err := json.Unmarshal([]byte(withoutComments(given.Content)), &entries); err != nil {
			return err
		}
		return render(entries)
	})
	sc.Step(`^the payload is a map of lists:$`, func(given *godog.DocString) error {
		var entries map[string][]string
		if err := json.Unmarshal([]byte(withoutComments(given.Content)), &entries); err != nil {
			return err
		}
		return render(entries)
	})
	sc.Step(`^the payload is a map with "([^"]*)" holding the bodies '([^']*)', '([^']*)', and '([^']*)'$`,
		func(key, b1, b2, b3 string) error { return render(map[string]any{key: [][]byte{[]byte(b1), []byte(b2), []byte(b3)}}) })
	sc.Step(`^the payload is a map with "([^"]*)" = "([^"]*)" and "([^"]*)" = "([^"]*)"$`,
		func(k1, v1, k2, v2 string) error { return render(map[string]string{k1: v1, k2: v2}) })
	sc.Step(`^the payload is the body '([^']*)'$`, func(body string) error { return render([]byte(body)) })
	sc.Step(`^the payload is a map with "([^"]*)" holding the body '([^']*)'$`,
		func(key, body string) error { return render(map[string]any{key: []byte(body)}) })
	sc.Step(`^the payload is a (\w+) message given as '([^']*)'$`, func(name, given string) error {
		msg, err := newMessage(name)
		if err != nil {
			return err
		}
		if err := (protojson.UnmarshalOptions{Resolver: protoregistry.GlobalTypes}).Unmarshal([]byte(given), msg); err != nil {
			return err
		}
		return render(msg)
	})
	sc.Step(`^the payload is a (\w+) message:$`, func(name string, given *godog.DocString) error {
		msg, err := newMessage(name)
		if err != nil {
			return err
		}
		if err := (protojson.UnmarshalOptions{Resolver: protoregistry.GlobalTypes}).Unmarshal([]byte(withoutComments(given.Content)), msg); err != nil {
			return err
		}
		return render(msg)
	})
	sc.Step(`^the payload is a Wallet message whose extra is an Any of the type "([^"]*)" holding (\d+) bytes$`, func(url string, size int) error {
		msg, err := newMessage("Wallet")
		if err != nil {
			return err
		}
		// built on the wire: no message of an unknown type can be given as JSON
		extra, err := proto.Marshal(&anypb.Any{TypeUrl: url, Value: bytes.Repeat([]byte("x"), size)})
		if err != nil {
			return err
		}
		wire := protowire.AppendBytes(protowire.AppendTag(nil, 7, protowire.BytesType), extra)
		if err := proto.Unmarshal(wire, msg); err != nil {
			return err
		}
		return render(msg)
	})
	sc.Step(`^the payload is (maps|lists) nested (\d+) levels deep$`, func(shape string, levels int) error {
		return render(nested(shape, levels))
	})
	sc.Step(`^the payload is a JSON body of (objects|lists) nested (\d+) levels deep$`, func(shape string, levels int) error {
		open, end := `{"next":`, "}"
		if shape == "lists" {
			open, end = "[", "]"
		}
		return render([]byte(strings.Repeat(open, levels) + `"end"` + strings.Repeat(end, levels)))
	})
	sc.Step(`^the payload is Node messages (?:linked by|each in) (next|children|named) nested (\d+) levels deep$`, func(field string, levels int) error {
		msg, err := newMessage("Node")
		if err != nil {
			return err
		}
		if err := protojson.Unmarshal([]byte(nestedNodes(field, levels)), msg); err != nil {
			return err
		}
		return render(msg)
	})
	sc.Step(`^the payload is a link "([^"]*)" whose next is itself$`, func(name string) error {
		self := &link{Name: name}
		self.Next = self
		return render(self)
	})
	sc.Step(`^the payload is the time "([^"]*)"$`, func(at string) error {
		t, err := time.Parse(time.RFC3339, at)
		if err != nil {
			return err
		}
		return render(t)
	})
	sc.Step(`^the masked payload should be:$`, func(want *godog.DocString) error {
		if !sameJSON(rendered, want.Content) {
			return fmt.Errorf("masked: got %s, want %s", rendered, want.Content)
		}
		return nil
	})
	sc.Step(`^the masked payload should be '([^']*)'$`, func(want string) error {
		if !sameJSON(rendered, want) {
			return fmt.Errorf("masked: got %s, want %s", rendered, want)
		}
		return nil
	})
	sc.Step(`^the masked payload should (mention|not mention) "([^"]*)"$`, func(how, text string) error {
		if strings.Contains(rendered, text) != (how == "mention") {
			return fmt.Errorf("masked %s: want it to %s %q", rendered, how, text)
		}
		return nil
	})
	sc.Step(`^the mask should report no unparsed body$`, func() error {
		if len(unparsed) != 0 {
			return fmt.Errorf("unparsed: got %v, want none", unparsed)
		}
		return nil
	})
	sc.Step(`^the mask should report these unparsed bodies:$`, func(table *godog.Table) error {
		var want []masked.Unparsed
		for _, row := range table.Rows[1:] {
			size, err := strconv.Atoi(row.Cells[1].Value)
			if err != nil {
				return err
			}
			want = append(want, masked.Unparsed{Path: row.Cells[0].Value, Bytes: size})
		}
		if !reflect.DeepEqual(unparsed, want) {
			return fmt.Errorf("unparsed: got %v, want %v", unparsed, want)
		}
		return nil
	})
	sc.Step(`^the mask should report the unparsed body "([^"]*)" of (\d+) bytes$`, func(path string, size int) error {
		if want := []masked.Unparsed{{Path: path, Bytes: size}}; !reflect.DeepEqual(unparsed, want) {
			return fmt.Errorf("unparsed: got %v, want %v", unparsed, want)
		}
		return nil
	})

	// payload-policy.feature and bound-payload.feature

	loggerOf := func() *slog.Logger {
		if logger == nil {
			logger = slog.New(masked.NewHandler(&out, level, cfg))
		}
		return logger
	}
	logRecordAt := func(at slog.Level, message string, failed bool, payload any) {
		loggerOf().LogAttrs(ctx, at, message, slog.String("command", "PlanItems"), logging.Failed(failed), logging.Payload(payload))
	}
	logRecord := func(message string, failed bool, payload any) { logRecordAt(slog.LevelInfo, message, failed, payload) }
	// lines are the records written, decoded, and written again as the
	// handler encodes, so a step finds "<" as it is.
	lines := func() (all []map[string]any, text []string) {
		for _, raw := range strings.Split(strings.TrimSpace(out.String()), "\n") {
			var rec map[string]any
			if raw == "" || json.Unmarshal([]byte(raw), &rec) != nil {
				continue
			}
			all, text = append(all, rec), append(text, encode(rec))
		}
		fmt.Printf("then: log\n%s\n", strings.Join(text, "\n"))
		return all, text
	}
	isWarning := func(rec map[string]any) bool { return rec["msg"] == masked.NotParsedMessage }
	firstRecord := func() (map[string]any, string, error) {
		all, text := lines()
		if len(all) == 0 {
			return nil, "", fmt.Errorf("nothing was logged")
		}
		return all[0], text[0], nil
	}
	sc.Step(`^the request id "([^"]*)" is in the context$`, func(id string) error { ctx = logging.WithRequestID(ctx, id); return nil })
	sc.Step(`^the log is written from the level "([^"]*)"$`, func(name string) error { return level.UnmarshalText([]byte(name)) })
	sc.Step(`^a record "([^"]*)" is logged at ERROR with the payload body '([^']*)'$`, func(message, body string) error {
		logRecordAt(slog.LevelError, message, true, map[string]any{"body": []byte(body)})
		return nil
	})
	sc.Step(`^LOG_PAYLOAD is "([^"]*)"$`, func(mode string) error { cfg.Payload = mode; return nil })
	sc.Step(`^LOG_PAYLOAD_MAX_BYTES is (\d+)$`, func(n int) error { cfg.MaxBytes = n; return nil })
	sc.Step(`^a record is logged with the payload password "([^"]*)" and title "([^"]*)"$`, func(password, title string) error {
		logRecord("command called", false, map[string]string{"password": password, "title": title})
		return nil
	})
	sc.Step(`^a record with failed "([^"]*)" is logged with the payload title "([^"]*)"$`, func(failed, title string) error {
		logRecord("command called", failed == "true", map[string]string{"title": title})
		return nil
	})
	sc.Step(`^a record is logged with the payload password "([^"]*)" and a title of (\d+) characters$`, func(password string, n int) error {
		logRecord("command called", false, map[string]string{"password": password, "title": strings.Repeat("a", n)})
		return nil
	})
	sc.Step(`^a record is logged with the payload title of (\d+) characters$`, func(n int) error {
		logRecord("command called", false, map[string]string{"title": strings.Repeat("a", n)})
		return nil
	})
	sc.Step(`^a record is logged with a payload that cannot be written as JSON$`, func() error {
		logRecord("command called", false, map[string]any{"events": make(chan int)})
		return nil
	})
	sc.Step(`^a record "([^"]*)" is logged with the payload body '([^']*)'$`, func(message, body string) error {
		logRecord(message, false, map[string]any{"body": []byte(body)})
		return nil
	})
	sc.Step(`^the payload password "([^"]*)" and the body '([^']*)' are bound with logger\.With, and (\d+) records "([^"]*)" are logged$`,
		func(password, body string, n int, message string) error {
			bound := loggerOf().With(logging.Payload(map[string]any{"password": password, "body": []byte(body)}))
			for range n {
				bound.LogAttrs(ctx, slog.LevelInfo, message)
			}
			return nil
		})
	sc.Step(`^the record should (have|not have) "([^"]*)"$`, func(how, key string) error {
		rec, text, err := firstRecord()
		if err != nil {
			return err
		}
		if _, has := rec[key]; has != (how == "have") {
			return fmt.Errorf("record %s: want it to %s %q", text, how, key)
		}
		return nil
	})
	sc.Step(`^the record should have "([^"]*)" = "([^"]*)"$`, func(key, want string) error {
		rec, text, err := firstRecord()
		if err != nil {
			return err
		}
		if rec[key] != want {
			return fmt.Errorf("record %s: %s=%v, want %q", text, key, rec[key], want)
		}
		return nil
	})
	sc.Step(`^the record should (mention|not mention) "([^"]*)"$`, func(how, text string) error {
		_, line, err := firstRecord()
		if err != nil {
			return err
		}
		if strings.Contains(line, text) != (how == "mention") {
			return fmt.Errorf("record %s: want it to %s %q", line, how, text)
		}
		return nil
	})
	sc.Step(`^the log should not mention "([^"]*)"$`, func(text string) error {
		if _, all := lines(); strings.Contains(strings.Join(all, "\n"), text) {
			return fmt.Errorf("the log mentions %q", text)
		}
		return nil
	})
	sc.Step(`^every record should have "([^"]*)" = "([^"]*)"$`, func(key, want string) error {
		all, text := lines()
		for i, rec := range all {
			if !isWarning(rec) && rec[key] != want {
				return fmt.Errorf("record %s: %s=%v, want %q", text[i], key, rec[key], want)
			}
		}
		return nil
	})
	sc.Step(`^every warning should have "([^"]*)" = "([^"]*)"$`, func(key, want string) error {
		all, text := lines()
		for i, rec := range all {
			if isWarning(rec) && rec[key] != want {
				return fmt.Errorf("warning %s: %s=%v, want %q", text[i], key, rec[key], want)
			}
		}
		return nil
	})
	warnings := func() []string {
		all, _ := lines()
		var got []string
		for _, rec := range all {
			if isWarning(rec) {
				got = append(got, fmt.Sprintf("%v %v|%v|%v", rec["level"], rec["record"], rec["field"], rec["bytes"]))
			}
		}
		return got
	}
	sc.Step(`^the log should hold these warnings:$`, func(table *godog.Table) error {
		var want []string
		for _, row := range table.Rows[1:] {
			size, err := strconv.Atoi(row.Cells[2].Value)
			if err != nil {
				return err
			}
			want = append(want, fmt.Sprintf("WARN %s|%s|%d", row.Cells[0].Value, row.Cells[1].Value, size))
		}
		if got := warnings(); !reflect.DeepEqual(got, want) {
			return fmt.Errorf("warnings: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the log should hold no warning$`, func() error {
		if got := warnings(); len(got) != 0 {
			return fmt.Errorf("warnings: got %q, want none", got)
		}
		return nil
	})

	// settings.feature

	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, func(name, value string) error { return os.Setenv(name, value) })
	sc.Step(`^the logging settings are loaded$`, func() error { cfg, loadErr = masked.LoadConfig(); return nil })
	sc.Step(`^LOG_PAYLOAD should be "([^"]*)" and the limit (\d+)$`, func(mode string, limit int) error {
		if loadErr != nil || cfg.Payload != mode || cfg.MaxBytes != limit {
			return fmt.Errorf("config %+v (err %v), want %s and %d", cfg, loadErr, mode, limit)
		}
		return nil
	})
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, func(text string) error {
		if loadErr == nil || !strings.Contains(loadErr.Error(), text) {
			return fmt.Errorf("error %v, want one mentioning %q", loadErr, text)
		}
		return nil
	})
}
