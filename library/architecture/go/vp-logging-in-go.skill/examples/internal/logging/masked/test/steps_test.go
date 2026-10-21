package test

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"log/slog"
	"os"
	"strings"

	"github.com/cucumber/godog"
	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/reflect/protodesc"
	"google.golang.org/protobuf/reflect/protoreflect"
	"google.golang.org/protobuf/types/descriptorpb"
	"google.golang.org/protobuf/types/dynamicpb"

	"github.com/example/todo/internal/logging"
	"github.com/example/todo/internal/logging/masked"
)

var variables = []string{"LOG_PAYLOAD", "LOG_PAYLOAD_MAX_BYTES"}

// person stands for a command with a secret field and a field not to log.
type person struct {
	Name string `json:"name"`
	PIN  string `json:"pin" log:"mask"`
	Note string `json:"note" log:"omit"`
}

// pinMessage is a proto message built at run time - title, and pin marked
// [debug_redact = true] - so the scenarios need no proto file of their own.
func pinMessage(title, pin string) (proto.Message, error) {
	file := &descriptorpb.FileDescriptorProto{
		Name: proto.String("test/pin.proto"), Package: proto.String("test"), Syntax: proto.String("proto3"),
		MessageType: []*descriptorpb.DescriptorProto{{
			Name: proto.String("Pin"),
			Field: []*descriptorpb.FieldDescriptorProto{
				{Name: proto.String("title"), Number: proto.Int32(1), JsonName: proto.String("title"),
					Type: descriptorpb.FieldDescriptorProto_TYPE_STRING.Enum(), Label: descriptorpb.FieldDescriptorProto_LABEL_OPTIONAL.Enum()},
				{Name: proto.String("pin"), Number: proto.Int32(2), JsonName: proto.String("pin"),
					Type: descriptorpb.FieldDescriptorProto_TYPE_STRING.Enum(), Label: descriptorpb.FieldDescriptorProto_LABEL_OPTIONAL.Enum(),
					Options: &descriptorpb.FieldOptions{DebugRedact: proto.Bool(true)}},
			},
		}},
	}
	fd, err := protodesc.NewFile(file, nil)
	if err != nil {
		return nil, err
	}
	desc := fd.Messages().ByName("Pin")
	msg := dynamicpb.NewMessage(desc)
	msg.Set(desc.Fields().ByName("title"), protoreflect.ValueOfString(title))
	msg.Set(desc.Fields().ByName("pin"), protoreflect.ValueOfString(pin))
	return msg, nil
}

func registerSteps(sc *godog.ScenarioContext) {
	var rendered string
	var out bytes.Buffer
	var ctx context.Context
	var cfg masked.Config
	var loadErr error
	sc.Before(func(c context.Context, _ *godog.Scenario) (context.Context, error) {
		rendered, ctx = "", context.Background()
		out.Reset()
		cfg = masked.Config{Payload: masked.PayloadAlways, MaxBytes: 4096}
		for _, name := range variables {
			_ = os.Unsetenv(name)
		}
		return c, nil
	})
	render := func(v any) error {
		b, err := json.Marshal(masked.Mask(v))
		rendered = string(b)
		fmt.Printf("when: Mask -> %s\n", rendered)
		return err
	}
	sc.Step(`^the payload is a command with the name "([^"]*)", the pin "([^"]*)" tagged mask, and the note "([^"]*)" tagged omit$`,
		func(name, pin, note string) error { return render(person{Name: name, PIN: pin, Note: note}) })
	sc.Step(`^the payload is a map with "([^"]*)" = "([^"]*)" and "([^"]*)" = "([^"]*)"$`,
		func(k1, v1, k2, v2 string) error { return render(map[string]string{k1: v1, k2: v2}) })
	sc.Step(`^the payload is the body '([^']*)'$`, func(body string) error { return render([]byte(body)) })
	sc.Step(`^the payload is a proto message with the title "([^"]*)" and the pin "([^"]*)" marked debug_redact$`, func(title, pin string) error {
		msg, err := pinMessage(title, pin)
		if err != nil {
			return err
		}
		return render(msg)
	})
	sc.Step(`^the masked payload should be '([^']*)'$`, func(want string) error {
		if rendered != want {
			return fmt.Errorf("masked: got %s, want %s", rendered, want)
		}
		return nil
	})
	sc.Step(`^the masked payload should mention "([^"]*)"$`, func(text string) error {
		if !strings.Contains(rendered, text) {
			return fmt.Errorf("masked %s does not mention %q", rendered, text)
		}
		return nil
	})
	sc.Step(`^the masked payload should not mention "([^"]*)"$`, func(text string) error {
		if strings.Contains(rendered, text) {
			return fmt.Errorf("masked %s mentions %q", rendered, text)
		}
		return nil
	})

	logRecord := func(failed bool, payload any) {
		logger := slog.New(masked.NewHandler(&out, slog.LevelInfo, cfg))
		logger.LogAttrs(ctx, slog.LevelInfo, "command called", slog.String("command", "PlanItems"), logging.Failed(failed), logging.Payload(payload))
		fmt.Printf("when: logged -> %s", out.String())
	}
	sc.Step(`^the request id "([^"]*)" is in the context$`, func(id string) error { ctx = logging.WithRequestID(ctx, id); return nil })
	sc.Step(`^LOG_PAYLOAD is "([^"]*)"$`, func(mode string) error { cfg.Payload = mode; return nil })
	sc.Step(`^LOG_PAYLOAD_MAX_BYTES is (\d+)$`, func(n int) error { cfg.MaxBytes = n; return nil })
	sc.Step(`^a record is logged with the payload password "([^"]*)" and title "([^"]*)"$`, func(password, title string) error {
		logRecord(false, map[string]string{"password": password, "title": title})
		return nil
	})
	sc.Step(`^a record with failed "([^"]*)" is logged with the payload title "([^"]*)"$`, func(failed, title string) error {
		logRecord(failed == "true", map[string]string{"title": title})
		return nil
	})
	sc.Step(`^a record is logged with a payload of (\d+) characters$`, func(n int) error {
		logRecord(false, map[string]string{"title": strings.Repeat("a", n)})
		return nil
	})
	line := func() map[string]any {
		var rec map[string]any
		_ = json.Unmarshal(out.Bytes(), &rec)
		return rec
	}
	sc.Step(`^the line should have "([^"]*)" = "([^"]*)"$`, func(key, want string) error {
		if got := line()[key]; got != want {
			return fmt.Errorf("line %s: %s=%v, want %q", out.String(), key, got, want)
		}
		return nil
	})
	sc.Step(`^the line should (have|not have) "([^"]*)"$`, func(keep, key string) error {
		if _, has := line()[key]; has != (keep == "have") {
			return fmt.Errorf("line %s: %s present=%v, want %s", out.String(), key, has, keep)
		}
		return nil
	})
	sc.Step(`^the line should mention "([^"]*)"$`, func(text string) error {
		if !strings.Contains(out.String(), text) {
			return fmt.Errorf("line %s does not mention %q", out.String(), text)
		}
		return nil
	})
	sc.Step(`^the line should not mention "([^"]*)"$`, func(text string) error {
		if strings.Contains(out.String(), text) {
			return fmt.Errorf("line %s mentions %q", out.String(), text)
		}
		return nil
	})

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
