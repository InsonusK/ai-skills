// Source: skill vp-logging-in-go

package masked

import (
	"bytes"
	"encoding"
	"encoding/json"
	"fmt"
	"reflect"
	"strings"

	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/reflect/protoreflect"
	"google.golang.org/protobuf/reflect/protoregistry"
	"google.golang.org/protobuf/types/descriptorpb"
)

// Masked is what a masked value is written as.
const Masked = "***"

// TooDeep is what a value more than MaxDepth levels below the payload is
// written as: a cycle of pointers ends here instead of in a stack overflow.
const TooDeep = "<too deep>"

// MaxDepth is how many levels below the payload are written.
const MaxDepth = 32

// secretNames mark a field, a map key, or a JSON key as secret whatever its
// value and whatever it is tagged: the safety net for a secret nobody marked.
// A name matches when, lower-cased and without "_" and "-", it contains one.
var secretNames = []string{
	"password", "passwd", "pwd", "passphrase", "secret", "token", "authorization",
	"cookie", "apikey", "privatekey", "credential", "session", "cvv",
}

// Unparsed is a body the mask could not read - bytes that are not JSON, or a
// proto Any of a type this binary does not know - and so wrote as
// "<unparsed body, N bytes>" instead of as it is.
type Unparsed struct {
	// Path is where the body is in the payload: $, $.body, $.items[0].blob.
	Path  string
	Bytes int
}

// Mask returns v as plain data - maps, slices, and scalars that encode as
// JSON - with every secret replaced by Masked and every omitted field left
// out, and the bodies it could not read. A value is secret when
//
//   - a struct field is tagged `log:"mask"` (`log:"omit"` or `json:"-"`
//     leaves it out);
//   - a field of a proto message has [debug_redact = true];
//   - its field name, map key, or JSON key matches secretNames.
//
// Bytes (a request body) are parsed as JSON and masked by their keys, and so
// is a string that holds a JSON object or array; bytes that are not JSON are
// never written as they are, since nothing in them can be masked.
func Mask(v any) (any, []Unparsed) {
	m := &masker{}
	return m.value(reflect.ValueOf(v), "$", 0), m.unparsed
}

func secret(name string) bool {
	n := strings.NewReplacer("_", "", "-", "").Replace(strings.ToLower(name))
	for _, s := range secretNames {
		if strings.Contains(n, s) {
			return true
		}
	}
	return false
}

// Unmask returns v as plain data like Mask, but masks nothing: marks, secret
// names, and debug_redact are ignored, and a body that is not JSON is its
// text. Only for debugging, with LOG_PAYLOAD_UNMASKED: a field marked to omit
// or left out of its JSON form stays out, and the depth limit holds.
func Unmask(v any) any {
	m := &masker{unmasked: true}
	return m.value(reflect.ValueOf(v), "$", 0)
}

// masker walks one payload, collecting the bodies it could not read.
type masker struct {
	unparsed []Unparsed
	unmasked bool // Unmask: nothing is masked
}

// secret reports whether name is masked in this walk.
func (m *masker) secret(name string) bool {
	return !m.unmasked && secret(name)
}

func (m *masker) value(v reflect.Value, path string, depth int) any {
	if !v.IsValid() {
		return nil
	}
	if depth > MaxDepth {
		return TooDeep
	}
	if v.CanInterface() {
		if msg, ok := v.Interface().(proto.Message); ok && v.Kind() == reflect.Pointer && !v.IsNil() {
			return m.message(msg.ProtoReflect(), path, depth)
		}
		if b, ok := v.Interface().([]byte); ok {
			return m.body(b, path, depth)
		}
		if n, ok := v.Interface().(json.Number); ok {
			return n // a number of a parsed body, written as it was
		}
	}
	switch v.Kind() {
	case reflect.Pointer, reflect.Interface:
		if v.IsNil() {
			return nil
		}
		return m.value(v.Elem(), path, depth)
	case reflect.Struct:
		if text, ok := asText(v); ok {
			return text
		}
		return m.object(v, path, depth)
	case reflect.Map:
		out := map[string]any{}
		iter := v.MapRange()
		for iter.Next() {
			key := toString(iter.Key())
			if m.secret(key) {
				out[key] = Masked
			} else {
				out[key] = m.value(iter.Value(), path+"."+key, depth+1)
			}
		}
		return out
	case reflect.Slice, reflect.Array:
		out := make([]any, v.Len())
		for i := range v.Len() {
			out[i] = m.value(v.Index(i), fmt.Sprintf("%s[%d]", path, i), depth+1)
		}
		return out
	case reflect.String:
		return m.text(v.String(), path, depth)
	default:
		if v.CanInterface() {
			return v.Interface()
		}
		return nil
	}
}

// asText is a struct that says how it is written - a time.Time - as that text.
func asText(v reflect.Value) (string, bool) {
	if !v.CanInterface() {
		return "", false
	}
	if t, ok := v.Interface().(encoding.TextMarshaler); ok {
		if text, err := t.MarshalText(); err == nil {
			return string(text), true
		}
	}
	return "", false
}

func (m *masker) object(v reflect.Value, path string, depth int) map[string]any {
	out := map[string]any{}
	t := v.Type()
	for i := range t.NumField() {
		field := t.Field(i)
		if !field.IsExported() {
			continue
		}
		jsonTag := field.Tag.Get("json")
		if jsonTag == "-" {
			continue // left out of its JSON form: not part of what the code shows
		}
		name := field.Name
		if tag, _, _ := strings.Cut(jsonTag, ","); tag != "" {
			name = tag
		}
		switch tag := field.Tag.Get("log"); {
		case tag == "omit":
			continue
		case !m.unmasked && tag == "mask", m.secret(name):
			out[name] = Masked
		default:
			out[name] = m.value(v.Field(i), path+"."+name, depth+1)
		}
	}
	return out
}

// text is a string as it is, unless it holds a JSON object or array: then
// that JSON masked by its keys.
func (m *masker) text(s string, path string, depth int) any {
	trimmed := strings.TrimSpace(s)
	if !strings.HasPrefix(trimmed, "{") && !strings.HasPrefix(trimmed, "[") {
		return s
	}
	if parsed, ok := parseJSON([]byte(trimmed)); ok {
		return m.value(reflect.ValueOf(parsed), path, depth)
	}
	return s
}

// body is bytes parsed as JSON and masked by their keys; bytes that are not
// JSON are reported and written as their size only. Empty bytes are "".
func (m *masker) body(b []byte, path string, depth int) any {
	if len(b) == 0 {
		return ""
	}
	if parsed, ok := parseJSON(b); ok {
		return m.value(reflect.ValueOf(parsed), path, depth)
	}
	return m.unreadable(b, path)
}

// unreadable is bytes nothing can be masked in: reported and written as their
// size only - or, unmasked, as their text.
func (m *masker) unreadable(raw []byte, path string) string {
	if m.unmasked {
		return string(raw)
	}
	m.unparsed = append(m.unparsed, Unparsed{Path: path, Bytes: len(raw)})
	return fmt.Sprintf("<unparsed body, %d bytes>", len(raw))
}

// parseJSON keeps numbers as they were written, so a large id is not rounded.
func parseJSON(b []byte) (any, bool) {
	dec := json.NewDecoder(bytes.NewReader(b))
	dec.UseNumber()
	var parsed any
	if dec.Decode(&parsed) != nil || dec.More() {
		return nil, false
	}
	return parsed, true
}

// message walks a proto message by its descriptor: the JSON name of each set
// field, [debug_redact = true] masked.
func (m *masker) message(msg protoreflect.Message, path string, depth int) any {
	if msg.Descriptor().FullName() == "google.protobuf.Any" {
		return m.anyMessage(msg, path, depth)
	}
	out := map[string]any{}
	msg.Range(func(fd protoreflect.FieldDescriptor, value protoreflect.Value) bool {
		name := fd.JSONName()
		options, _ := fd.Options().(*descriptorpb.FieldOptions)
		if !m.unmasked && options.GetDebugRedact() || m.secret(name) {
			out[name] = Masked
			return true
		}
		out[name] = m.field(fd, value, path+"."+name, depth+1)
		return true
	})
	return out
}

// anyMessage unpacks an Any whose type is registered in this binary - every
// generated message is - and writes it like protojson, with its "@type"; an
// Any of an unknown type is bytes nothing can be masked in.
func (m *masker) anyMessage(msg protoreflect.Message, path string, depth int) any {
	fields := msg.Descriptor().Fields()
	url := msg.Get(fields.ByName("type_url")).String()
	value := msg.Get(fields.ByName("value")).Bytes()
	mt, err := protoregistry.GlobalTypes.FindMessageByURL(url)
	if err != nil {
		return m.unreadable(value, path)
	}
	inner := mt.New()
	if proto.Unmarshal(value, inner.Interface()) != nil {
		return m.unreadable(value, path)
	}
	out, _ := m.message(inner, path, depth).(map[string]any)
	out["@type"] = url
	return out
}

func (m *masker) field(fd protoreflect.FieldDescriptor, value protoreflect.Value, path string, depth int) any {
	if depth > MaxDepth {
		return TooDeep
	}
	switch {
	case fd.IsList():
		list := value.List()
		items := make([]any, list.Len())
		for i := range list.Len() {
			items[i] = m.scalar(fd, list.Get(i), fmt.Sprintf("%s[%d]", path, i), depth+1)
		}
		return items
	case fd.IsMap():
		entries := map[string]any{}
		value.Map().Range(func(k protoreflect.MapKey, v protoreflect.Value) bool {
			key := k.String()
			if m.secret(key) {
				entries[key] = Masked
			} else {
				entries[key] = m.scalar(fd.MapValue(), v, path+"."+key, depth+1)
			}
			return true
		})
		return entries
	default:
		return m.scalar(fd, value, path, depth)
	}
}

func (m *masker) scalar(fd protoreflect.FieldDescriptor, value protoreflect.Value, path string, depth int) any {
	if depth > MaxDepth {
		return TooDeep
	}
	switch fd.Kind() {
	case protoreflect.MessageKind, protoreflect.GroupKind:
		return m.message(value.Message(), path, depth)
	case protoreflect.EnumKind:
		if ev := fd.Enum().Values().ByNumber(value.Enum()); ev != nil {
			return string(ev.Name())
		}
		return int32(value.Enum())
	case protoreflect.BytesKind:
		return m.body(value.Bytes(), path, depth)
	case protoreflect.StringKind:
		return m.text(value.String(), path, depth)
	default:
		return value.Interface()
	}
}

func toString(v reflect.Value) string {
	if v.Kind() == reflect.String {
		return v.String()
	}
	if v.CanInterface() {
		if s, ok := v.Interface().(interface{ String() string }); ok {
			return s.String()
		}
	}
	b, _ := json.Marshal(v.Interface())
	return string(b)
}
