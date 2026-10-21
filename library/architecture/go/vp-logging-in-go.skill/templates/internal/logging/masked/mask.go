// Source: skill vp-logging-in-go

package masked

import (
	"encoding"
	"encoding/json"
	"reflect"
	"strings"

	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/reflect/protoreflect"
	"google.golang.org/protobuf/types/descriptorpb"
)

// Masked is what a masked value is written as.
const Masked = "***"

// secretNames mark a field, a map key, or a JSON key as secret whatever its
// value and whatever it is tagged: the safety net for a secret nobody marked.
// A name matches when, lower-cased and without "_" and "-", it contains one.
var secretNames = []string{"password", "passwd", "secret", "token", "authorization", "cookie", "apikey", "privatekey", "credential"}

// Mask returns v as plain data - maps, slices, and scalars that encode as
// JSON - with every secret replaced by Masked and every omitted field left
// out. A value is secret when
//
//   - a struct field is tagged `log:"mask"` (`log:"omit"` leaves it out);
//   - a field of a generated proto message has [debug_redact = true];
//   - its field name, map key, or JSON key matches secretNames.
//
// A []byte that holds JSON (a request body) is parsed and masked by its keys;
// any other []byte is written as a string.
func Mask(v any) any {
	return mask(reflect.ValueOf(v))
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

func mask(v reflect.Value) any {
	if !v.IsValid() {
		return nil
	}
	if v.CanInterface() {
		if msg, ok := v.Interface().(proto.Message); ok && v.Kind() == reflect.Pointer && !v.IsNil() {
			return maskMessage(msg.ProtoReflect())
		}
		if b, ok := v.Interface().([]byte); ok {
			return maskBytes(b)
		}
	}
	switch v.Kind() {
	case reflect.Pointer, reflect.Interface:
		if v.IsNil() {
			return nil
		}
		return mask(v.Elem())
	case reflect.Struct:
		if text, ok := asText(v); ok {
			return text
		}
		return maskStruct(v)
	case reflect.Map:
		out := map[string]any{}
		iter := v.MapRange()
		for iter.Next() {
			key := toString(iter.Key())
			if secret(key) {
				out[key] = Masked
			} else {
				out[key] = mask(iter.Value())
			}
		}
		return out
	case reflect.Slice, reflect.Array:
		out := make([]any, v.Len())
		for i := range v.Len() {
			out[i] = mask(v.Index(i))
		}
		return out
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
	if m, ok := v.Interface().(encoding.TextMarshaler); ok {
		if text, err := m.MarshalText(); err == nil {
			return string(text), true
		}
	}
	return "", false
}

func maskStruct(v reflect.Value) map[string]any {
	out := map[string]any{}
	t := v.Type()
	for i := range t.NumField() {
		field := t.Field(i)
		if !field.IsExported() {
			continue
		}
		name := field.Name
		if tag, _, _ := strings.Cut(field.Tag.Get("json"), ","); tag != "" && tag != "-" {
			name = tag
		}
		switch tag := field.Tag.Get("log"); {
		case tag == "omit":
			continue
		case tag == "mask", secret(name):
			out[name] = Masked
		default:
			out[name] = mask(v.Field(i))
		}
	}
	return out
}

// maskMessage walks a proto message by its descriptor: the JSON name of each
// set field, [debug_redact = true] masked.
func maskMessage(m protoreflect.Message) map[string]any {
	out := map[string]any{}
	m.Range(func(fd protoreflect.FieldDescriptor, value protoreflect.Value) bool {
		name := fd.JSONName()
		options, _ := fd.Options().(*descriptorpb.FieldOptions)
		if options.GetDebugRedact() || secret(name) {
			out[name] = Masked
			return true
		}
		out[name] = protoValue(fd, value)
		return true
	})
	return out
}

func protoValue(fd protoreflect.FieldDescriptor, value protoreflect.Value) any {
	switch {
	case fd.IsList():
		list := value.List()
		items := make([]any, list.Len())
		for i := range list.Len() {
			items[i] = protoScalar(fd, list.Get(i))
		}
		return items
	case fd.IsMap():
		entries := map[string]any{}
		value.Map().Range(func(k protoreflect.MapKey, v protoreflect.Value) bool {
			key := k.String()
			if secret(key) {
				entries[key] = Masked
			} else {
				entries[key] = protoScalar(fd.MapValue(), v)
			}
			return true
		})
		return entries
	default:
		return protoScalar(fd, value)
	}
}

func protoScalar(fd protoreflect.FieldDescriptor, value protoreflect.Value) any {
	switch fd.Kind() {
	case protoreflect.MessageKind, protoreflect.GroupKind:
		return maskMessage(value.Message())
	case protoreflect.EnumKind:
		if ev := fd.Enum().Values().ByNumber(value.Enum()); ev != nil {
			return string(ev.Name())
		}
		return int32(value.Enum())
	case protoreflect.BytesKind:
		return maskBytes(value.Bytes())
	default:
		return value.Interface()
	}
}

// maskBytes parses JSON and masks it by its keys; anything else is text.
func maskBytes(b []byte) any {
	var parsed any
	if len(b) > 0 && json.Unmarshal(b, &parsed) == nil {
		return mask(reflect.ValueOf(parsed))
	}
	return string(b)
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
