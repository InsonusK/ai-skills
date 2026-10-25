using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Conformance.Masking;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace MaskedLogging.Tests.Steps;

/// <summary>The steps of masking.feature: a payload given to <see cref="PayloadMask"/>, and what it wrote.</summary>
[Binding]
public sealed class MaskingSteps(ITestOutputHelper output, MaskingSwitch masking)
{
    private static readonly JsonSerializerOptions Written = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    // Proto JSON of the messages of masking.proto, an Any of them included.
    private static readonly JsonParser ProtoJson = new(JsonParser.Settings.Default
        .WithTypeRegistry(TypeRegistry.FromFiles(MaskingReflection.Descriptor)));

    private string _rendered = "";
    private IReadOnlyList<Unparsed> _unparsed = [];

    private void Render(object? payload)
    {
        var masked = masking.Off ? new MaskedPayload(PayloadMask.Unmask(payload), []) : PayloadMask.Apply(payload);
        _rendered = masked.Value?.ToJsonString(Written) ?? "null";
        _unparsed = masked.Unparsed;
        output.WriteLine($"when: Apply -> {_rendered}, unparsed [{string.Join(", ", _unparsed)}]");
    }

    // Payloads given as JSON in a doc string; "// ..." says what is special about a field.

    [When(@"^the payload is an? (Person|Team|Login|Account|Upload):$")]
    public void WhenAFixture(string type, string given) => Render(Fixture(type, Given(given)!.AsObject()));

    [When("the payload is the body:")]
    public void WhenTheBodyGiven(string given) => Render(Encoding.UTF8.GetBytes(WithoutComments(given).Trim()));

    [When("the payload is a map:")]
    public void WhenAMapGiven(string given) => Render(Plain(Given(given)));

    [When("the payload is a map of lists:")]
    public void WhenAMapOfListsGiven(string given) => Render(JsonSerializer.Deserialize<Dictionary<string, string[]>>(WithoutComments(given)));

    [When(@"^the payload is a (\w+) message:$")]
    public void WhenAMessageGiven(string name, string given) =>
        Render(ProtoJson.Parse(WithoutComments(given), MaskingReflection.Descriptor.MessageTypes.Single(m => m.Name == name)));

    [When("the payload is a map with {string} holding the bodies {string}, {string}, and {string}")]
    public void WhenAMapHoldingBodies(string key, string b1, string b2, string b3) =>
        Render(new Dictionary<string, object?> { [key] = new[] { b1, b2, b3 }.Select(Encoding.UTF8.GetBytes).ToList() });

    [When("the payload is a map with {string} = {string} and {string} = {string}")]
    public void WhenAMap(string k1, string v1, string k2, string v2) => Render(new Dictionary<string, string> { [k1] = v1, [k2] = v2 });

    [When("the payload is the body {string}")]
    public void WhenTheBody(string body) => Render(Encoding.UTF8.GetBytes(body));

    [When("the payload is a map with {string} holding the body {string}")]
    public void WhenAMapHoldingTheBody(string key, string body) => Render(new Dictionary<string, object?> { [key] = Encoding.UTF8.GetBytes(body) });

    [When("the payload is a {word} message given as {string}")]
    public void WhenAMessage(string name, string json) =>
        Render(ProtoJson.Parse(json, MaskingReflection.Descriptor.MessageTypes.Single(m => m.Name == name)));

    [When("the payload is a Wallet message whose extra is an Any of the type {string} holding {int} bytes")]
    public void WhenAnUnknownAny(string url, int size) =>
        Render(new Wallet { Extra = new Any { TypeUrl = url, Value = ByteString.CopyFrom(new byte[size]) } });

    // levels maps or lists, each holding the next, the innermost the text "end"
    [When(@"^the payload is (maps|lists) nested (\d+) levels deep$")]
    public void WhenNested(string shape, int levels)
    {
        object? value = "end";
        for (var i = 0; i < levels; i++)
        {
            value = shape == "maps" ? new Dictionary<string, object?> { ["next"] = value } : new List<object?> { value };
        }
        Render(value);
    }

    [When(@"^the payload is a JSON body of (objects|lists) nested (\d+) levels deep$")]
    public void WhenAJsonBodyNested(string shape, int levels)
    {
        var (open, end) = shape == "lists" ? ("[", ']') : ("{\"next\":", '}');
        Render(Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat(open, levels)) + "\"end\"" + new string(end, levels)));
    }

    [When("the payload is a non-generic dictionary with {string} = {string} and {string} = {string}")]
    public void WhenAHashtable(string k1, string v1, string k2, string v2) => Render(new System.Collections.Hashtable { [k1] = v1, [k2] = v2 });

    [When("the payload is an object {string} whose other properties cannot be read")]
    public void WhenBroken(string name) => Render(new Broken(name));

    // Payload.feature

    [When("the payload is headers with {string} = {string} and {string} = {string}")]
    public void WhenHeaders(string k1, string v1, string k2, string v2) =>
        Render(new Headers(new Dictionary<string, string[]> { [k1] = [v1], [k2] = [v2] }));

    [When("the payload is a map with {string} holding a delegate")]
    public void WhenAMapHoldingADelegate(string key) => Render(new Dictionary<string, object?> { [key] = new Func<int>(() => 1) });

    [When("the payload is the time {string}")]
    public void WhenTheTime(string at) => Render(DateTimeOffset.Parse(at, System.Globalization.CultureInfo.InvariantCulture));

    // levels Node messages, each in the field of the one before, the innermost with the title "end"
    [When(@"^the payload is Node messages (?:linked by|each in) (next|children|named) nested (\d+) levels deep$")]
    public void WhenNestedNodes(string field, int levels)
    {
        var node = new Node { Title = "end" };
        for (var i = 1; i < levels; i++)
        {
            var parent = new Node();
            switch (field)
            {
                case "next":
                    parent.Next = node;
                    break;
                case "children":
                    parent.Children.Add(node);
                    break;
                default:
                    parent.Named["n"] = node;
                    break;
            }
            node = parent;
        }
        Render(node);
    }

    [When("the payload is a link {string} whose next is itself")]
    public void WhenALinkToItself(string name)
    {
        var link = new Link(name);
        link.Next = link;
        Render(link);
    }

    [Given("masking is switched off")]
    public void GivenMaskingIsSwitchedOff() => masking.Off = true;

    [Then("the payload should be written as:")]
    public void ThenThePayloadShouldBeWrittenAs(string want) => Assert.Equal(Canonical(want), Canonical(_rendered));

    [Then("the masked payload should be:")]
    public void ThenTheMaskedPayloadShouldBeGiven(string want) => Assert.Equal(Canonical(want), Canonical(_rendered));

    [Then("the masked payload should be {string}")]
    public void ThenTheMaskedPayloadShouldBe(string want) => Assert.Equal(Canonical(want), Canonical(_rendered));

    [Then("the masked payload should mention {string}")]
    public void ThenTheMaskedPayloadShouldMention(string text) => Assert.Contains(text, _rendered);

    [Then("the masked payload should not mention {string}")]
    public void ThenTheMaskedPayloadShouldNotMention(string text) => Assert.DoesNotContain(text, _rendered);

    [Then("the mask should report no unparsed body")]
    public void ThenNoUnparsedBody() => Assert.Empty(_unparsed);

    [Then("the mask should report these unparsed bodies:")]
    public void ThenTheseUnparsedBodies(DataTable table) =>
        // in any order: the order a payload is walked in is not part of the rule
        Assert.Equal(
            table.Rows.Select(row => new Unparsed(row["path"], int.Parse(row["bytes"]))).OrderBy(u => u.Path, StringComparer.Ordinal).ToList(),
            _unparsed.OrderBy(u => u.Path, StringComparer.Ordinal).ToList());

    [Then("the mask should report the unparsed body {string} of {int} bytes")]
    public void ThenTheUnparsedBody(string path, int bytes) => Assert.Equal([new Unparsed(path, bytes)], _unparsed);

    // The doc string without what follows "//" outside a JSON string on each line.
    internal static string WithoutComments(string text) => string.Join('\n', text.Split('\n').Select(line =>
    {
        var (inString, escaped) = (false, false);
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (escaped)
            {
                escaped = false;
            }
            else if (inString && c == '\\')
            {
                escaped = true;
            }
            else if (c == '"')
            {
                inString = !inString;
            }
            else if (!inString && c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                return line[..i].TrimEnd();
            }
        }
        return line.TrimEnd();
    }));

    private static JsonNode? Given(string text) => JsonNode.Parse(WithoutComments(text));

    // JSON as the plain values a part of the application logs: a dictionary, a list, a text, a number.
    private static object? Plain(JsonNode? node) => node switch
    {
        null => null,
        JsonObject json => json.ToDictionary(p => p.Key, p => Plain(p.Value)),
        JsonArray json => json.Select(Plain).ToList(),
        JsonValue value when value.GetValueKind() == JsonValueKind.String => value.GetValue<string>(),
        JsonValue value when value.GetValueKind() is JsonValueKind.True or JsonValueKind.False => value.GetValue<bool>(),
        _ => node.GetValue<decimal>(),
    };

    // The type a scenario names, built from the JSON of its fields; a field
    // left out of the type's JSON form (Person.draft) or a body (Upload.content)
    // is set by its name.
    private static object Fixture(string type, JsonObject fields)
    {
        string Text(JsonObject from, string key) => from[key]?.GetValue<string>() ?? "";
        Person PersonOf(JsonObject from) => new(Text(from, "name"), Text(from, "pin"), Text(from, "note"), Text(from, "draft"));
        return type switch
        {
            "Person" => PersonOf(fields),
            "Team" => new Team(
                Text(fields, "title"),
                PersonOf(fields["lead"]!.AsObject()),
                fields["members"]!.AsArray().Select(m => PersonOf(m!.AsObject())).ToList(),
                new Vault(Text(fields["vault"]!.AsObject(), "code"))),
            "Login" => new Login(Text(fields, "user"), Text(fields, "password")),
            "Account" => new Account(Text(fields, "user"), Text(fields, "p")),
            _ => new Upload(Text(fields, "name"), Encoding.UTF8.GetBytes(Text(fields, "content"))),
        };
    }

    // A JSON text with the keys of every object sorted, so two texts compare by
    // what they say, not by the order of their keys.
    internal static string Canonical(string json) => Sorted(JsonNode.Parse(json))?.ToJsonString(Written) ?? "null";

    private static JsonNode? Sorted(JsonNode? node) => node switch
    {
        JsonObject json => new JsonObject(json.OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => KeyValuePair.Create(p.Key, Sorted(p.Value)))),
        JsonArray json => new JsonArray(json.Select(Sorted).ToArray()),
        _ => node?.DeepClone(),
    };
}
