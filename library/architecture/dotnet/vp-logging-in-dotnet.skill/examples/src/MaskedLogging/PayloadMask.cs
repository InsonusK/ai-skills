// Source: skill vp-logging-in-dotnet

using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Any = Google.Protobuf.WellKnownTypes.Any;
using Shared.Logging;

namespace MaskedLogging;

/// <summary>
/// A body the mask could not read - bytes that are not JSON, or a proto Any
/// of a type no loaded assembly knows - and so wrote as
/// <c>&lt;unparsed body, N bytes&gt;</c> instead of as it is.
/// </summary>
/// <param name="Path">Where the body is in the payload: <c>$</c>, <c>$.body</c>, <c>$.items[0].blob</c>.</param>
/// <param name="Bytes">The size of the body.</param>
public sealed record Unparsed(string Path, int Bytes);

/// <summary>A payload as the mask wrote it, and the bodies it could not read.</summary>
public sealed record MaskedPayload(JsonNode? Value, IReadOnlyList<Unparsed> Unparsed);

/// <summary>
/// Turns a payload into JSON with every secret replaced by <see cref="Masked"/>
/// and every omitted property left out. A value is secret when
/// <list type="bullet">
/// <item>its property is marked <see cref="LogMaskAttribute"/>
/// (<see cref="LogOmitAttribute"/> or <see cref="JsonIgnoreAttribute"/> leaves it out);</item>
/// <item>a field of a proto message has <c>[debug_redact = true]</c>;</item>
/// <item>its property name, dictionary key, or JSON key matches a safety-net name.</item>
/// </list>
/// Bytes (a request body) are parsed as JSON and masked by their keys, and so
/// is a text that holds a JSON object or array; bytes that are not JSON are
/// never written as they are, since nothing in them can be masked.
/// </summary>
public static class PayloadMask
{
    /// <summary>What a masked value is written as.</summary>
    public const string Masked = "***";

    /// <summary>
    /// What a value more than <see cref="MaxDepth"/> levels below the payload
    /// is written as: an object graph with a cycle ends here instead of in a
    /// stack overflow.
    /// </summary>
    public const string TooDeep = "<too deep>";

    /// <summary>How many levels below the payload are written.</summary>
    public const int MaxDepth = 32;

    // Mark a property, a key, or a JSON key as secret whatever its value and
    // whatever it is marked: the safety net for a secret nobody marked. A name
    // matches when, lower-cased and without "_" and "-", it contains one.
    private static readonly string[] SecretNames =
    [
        "password", "passwd", "pwd", "passphrase", "secret", "token", "authorization",
        "cookie", "apikey", "privatekey", "credential", "session", "cvv",
    ];

    /// <summary>Masks <paramref name="payload"/>: a command, a result, a map of a request's parts, a proto message.</summary>
    public static MaskedPayload Apply(object? payload)
    {
        var walk = new Walk();
        var value = walk.Value(payload, "$", 0);
        return new MaskedPayload(value, walk.Unparsed);
    }

    /// <summary>Whether <paramref name="name"/> matches a safety-net name.</summary>
    public static bool IsSecret(string name)
    {
        var normalized = name.ToLowerInvariant().Replace("_", "").Replace("-", "");
        return SecretNames.Any(normalized.Contains);
    }

    /// <summary>One walk over one payload, collecting the bodies it could not read.</summary>
    private sealed class Walk
    {
        public List<Unparsed> Unparsed { get; } = [];

        public JsonNode? Value(object? value, string path, int depth)
        {
            if (value is null)
            {
                return null;
            }
            if (depth > MaxDepth)
            {
                return TooDeep;
            }
            switch (value)
            {
                case IMessage message:
                    return Message(message, path, depth);
                case byte[] bytes:
                    return Body(bytes, path, depth);
                case ReadOnlyMemory<byte> memory:
                    return Body(memory.ToArray(), path, depth);
                case string text:
                    return Text(text, depth);
                case JsonNode node:
                    return Json(node, depth);
                case JsonElement element:
                    return Json(JsonSerializer.SerializeToNode(element), depth);
                case Delegate or Type or MemberInfo or Stream or Task:
                    return $"<unloggable {value.GetType().Name}>";
            }
            var type = value.GetType();
            if (Options.Values.GetTypeInfo(type).Kind == JsonTypeInfoKind.None)
            {
                // written as one JSON value - a number, a time, an id, an enum,
                // or a type with a converter of its own, which chose how it is shown
                return JsonSerializer.SerializeToNode(value, type, Options.Values);
            }
            if (KeyValueType(type) is { } pair && value is IEnumerable pairs)
            {
                // a dictionary, generic or read-only, or a header map
                var key = pair.GetProperty("Key")!;
                var item = pair.GetProperty("Value")!;
                return Dictionary(pairs.Cast<object>().Select(p => (Key(key.GetValue(p)), item.GetValue(p))), path, depth);
            }
            if (value is IDictionary dictionary)
            {
                // a non-generic dictionary: a Hashtable, the Data of an exception
                return Dictionary(Entries(dictionary), path, depth);
            }
            if (value is IEnumerable items)
            {
                var list = new JsonArray();
                var i = 0;
                foreach (var each in items)
                {
                    list.Add(Value(each, $"{path}[{i++}]", depth + 1));
                }
                return list;
            }
            return Object(value, path, depth);
        }

        private JsonObject Dictionary(IEnumerable<(string Key, object? Value)> entries, string path, int depth)
        {
            var result = new JsonObject();
            foreach (var (key, value) in entries)
            {
                result[key] = IsSecret(key) ? Masked : Value(value, $"{path}.{key}", depth + 1);
            }
            return result;
        }

        private JsonObject Object(object value, string path, int depth)
        {
            var result = new JsonObject();
            foreach (var property in WrittenProperty.Of(value.GetType()))
            {
                if (property.Omit)
                {
                    continue;
                }
                if (property.Mask)
                {
                    result[property.Name] = Masked;
                    continue;
                }
                result[property.Name] = property.TryGet(value, out var field)
                    ? Value(field, $"{path}.{property.Name}", depth + 1)
                    : "<unreadable>"; // a throwing getter never loses the record
            }
            return result;
        }

        // A text as it is, unless it holds a JSON object or array: then that
        // JSON masked by its keys. Nothing inside JSON can be unparsed, so the
        // walk below a text or a body needs no path.
        private static JsonNode? Text(string text, int depth)
        {
            var trimmed = text.Trim();
            if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
            {
                try
                {
                    return Json(JsonNode.Parse(trimmed), depth);
                }
                catch (JsonException)
                {
                    // not JSON after all: a text such as "[urgent] buy milk"
                }
            }
            return text;
        }

        // Bytes parsed as JSON and masked by their keys; bytes that are not
        // JSON are reported and written as their size only. Empty bytes are "".
        private JsonNode? Body(byte[] bytes, string path, int depth)
        {
            if (bytes.Length == 0)
            {
                return "";
            }
            try
            {
                return Json(JsonNode.Parse(bytes), depth);
            }
            catch (JsonException)
            {
                return Unreadable(bytes.Length, path);
            }
        }

        private string Unreadable(int bytes, string path)
        {
            Unparsed.Add(new Unparsed(path, bytes));
            return $"<unparsed body, {bytes} bytes>";
        }

        // JSON already parsed, walked like any value: keys checked, texts read.
        private static JsonNode? Json(JsonNode? node, int depth)
        {
            if (node is null)
            {
                return null;
            }
            if (depth > MaxDepth)
            {
                return TooDeep;
            }
            switch (node)
            {
                case JsonObject json:
                    var result = new JsonObject();
                    foreach (var (key, value) in json)
                    {
                        result[key] = IsSecret(key) ? Masked : Json(value, depth + 1);
                    }
                    return result;
                case JsonArray json:
                    var list = new JsonArray();
                    for (var i = 0; i < json.Count; i++)
                    {
                        list.Add(Json(json[i], depth + 1));
                    }
                    return list;
                default:
                    return node.GetValueKind() == JsonValueKind.String ? Text(node.GetValue<string>(), depth) : node.DeepClone();
            }
        }

        // A proto message by its descriptor: the JSON name of each set field,
        // [debug_redact = true] masked.
        private JsonNode Message(IMessage message, string path, int depth)
        {
            if (message is Any any)
            {
                return AnyMessage(any, path, depth);
            }
            var result = new JsonObject();
            foreach (var field in message.Descriptor.Fields.InFieldNumberOrder())
            {
                var value = field.Accessor.GetValue(message);
                if (!IsSet(field, message, value))
                {
                    continue;
                }
                var name = field.JsonName;
                result[name] = field.GetOptions()?.DebugRedact == true || IsSecret(name)
                    ? Masked
                    : Field(field, value, $"{path}.{name}", depth + 1);
            }
            return result;
        }

        // An Any whose type a loaded assembly knows - every generated message
        // is - unpacked and written like proto JSON, with its "@type"; an Any
        // of an unknown type is bytes nothing can be masked in.
        private JsonNode AnyMessage(Any any, string path, int depth)
        {
            var descriptor = ProtoTypes.Find(Any.GetTypeName(any.TypeUrl));
            if (descriptor is null)
            {
                return Unreadable(any.Value.Length, path);
            }
            IMessage inner;
            try
            {
                inner = descriptor.Parser.ParseFrom(any.Value);
            }
            catch (InvalidProtocolBufferException)
            {
                return Unreadable(any.Value.Length, path);
            }
            var written = Message(inner, path, depth);
            if (written is not JsonObject fields)
            {
                return written; // an Any holding an Any it could not read
            }
            var result = new JsonObject { ["@type"] = any.TypeUrl };
            foreach (var (key, value) in fields)
            {
                result[key] = value?.DeepClone();
            }
            return result;
        }

        private JsonNode? Field(FieldDescriptor field, object value, string path, int depth)
        {
            if (field.IsMap)
            {
                var valueField = field.MessageType.FindFieldByNumber(2);
                var entries = new JsonObject();
                foreach (DictionaryEntry entry in (IDictionary)value)
                {
                    var key = Key(entry.Key);
                    entries[key] = IsSecret(key) ? Masked : Scalar(valueField, entry.Value!, $"{path}.{key}", depth + 1);
                }
                return entries;
            }
            if (field.IsRepeated)
            {
                var list = new JsonArray();
                var i = 0;
                foreach (var item in (IList)value)
                {
                    list.Add(Scalar(field, item, $"{path}[{i++}]", depth + 1));
                }
                return list;
            }
            return Scalar(field, value, path, depth);
        }

        private JsonNode? Scalar(FieldDescriptor field, object value, string path, int depth)
        {
            if (depth > MaxDepth)
            {
                return TooDeep;
            }
            switch (field.FieldType)
            {
                case FieldType.Message or FieldType.Group:
                    // a wrapper type (StringValue, ...) is given unwrapped
                    return value is IMessage message ? Message(message, path, depth) : Value(value, path, depth);
                case FieldType.Enum:
                    var number = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    // a number the enum does not know is written as the number
                    return field.EnumType.FindValueByNumber(number)?.Name is { } name ? (JsonNode)name : number;
                case FieldType.Bytes:
                    return Body(((ByteString)value).ToByteArray(), path, depth);
                case FieldType.String:
                    return Text((string)value, depth);
                default:
                    return JsonSerializer.SerializeToNode(value, value.GetType(), Options.Values);
            }
        }

        // Proto JSON writes the fields that are set: with presence, the ones
        // that have a value; without, the ones not at their default.
        private static bool IsSet(FieldDescriptor field, IMessage message, object value) => value switch
        {
            IDictionary entries => entries.Count > 0, // a map
            IList items => items.Count > 0,           // a repeated field
            _ when field.HasPresence => field.Accessor.HasValue(message),
            string text => text.Length > 0,
            ByteString bytes => !bytes.IsEmpty,
            // a number, a bool, or an enum without presence: set when not 0
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0,
        };
    }

    // The entries of a non-generic dictionary: its own enumerator gives
    // DictionaryEntry, the one of IEnumerable may give KeyValuePair.
    private static IEnumerable<(string Key, object? Value)> Entries(IDictionary dictionary)
    {
        var entries = dictionary.GetEnumerator();
        while (entries.MoveNext())
        {
            yield return (Key(entries.Key), entries.Value);
        }
    }

    // The text of a key: the secret names are matched on the text written.
    private static string Key(object? key) => string.Create(CultureInfo.InvariantCulture, $"{key}");

    // The KeyValuePair<TKey, TValue> a type enumerates, if it is a dictionary
    // without the non-generic IDictionary (IReadOnlyDictionary, a header map).
    private static Type? KeyValueType(Type type) => type.GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        .Select(i => i.GetGenericArguments()[0])
        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>));

    private static class Options
    {
        // How a single value is written: the names and numbers of the base provider.
        public static readonly JsonSerializerOptions Values = new(JsonSerializerOptions.Web);
    }

    /// <summary>How each property of a type is written, read once per type.</summary>
    private sealed record WrittenProperty(string Name, bool Mask, bool Omit, Func<object, object?> Get)
    {
        public bool TryGet(object owner, out object? value)
        {
            try
            {
                value = Get(owner);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        private static readonly ConcurrentDictionary<Type, WrittenProperty[]> Known = new();

        public static WrittenProperty[] Of(Type type) => Known.GetOrAdd(type, static type => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(p =>
            {
                var name = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? JsonNamingPolicy.CamelCase.ConvertName(p.Name);
                var ignored = p.GetCustomAttribute<JsonIgnoreAttribute>() is { Condition: JsonIgnoreCondition.Always };
                return new WrittenProperty(
                    name,
                    // the name in the code and in the JSON form: a renamed secret is still one
                    Mask: p.GetCustomAttribute<LogMaskAttribute>() is not null || IsSecret(name) || IsSecret(p.Name),
                    Omit: ignored || p.GetCustomAttribute<LogOmitAttribute>() is not null,
                    Get: p.GetValue);
            })
            .ToArray());
    }
}
