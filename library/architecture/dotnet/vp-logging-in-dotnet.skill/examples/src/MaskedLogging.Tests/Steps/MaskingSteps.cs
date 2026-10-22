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
public sealed class MaskingSteps(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions Written = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    // Proto JSON of the messages of masking.proto, an Any of them included.
    private static readonly JsonParser ProtoJson = new(JsonParser.Settings.Default
        .WithTypeRegistry(TypeRegistry.FromFiles(MaskingReflection.Descriptor)));

    private string _rendered = "";
    private IReadOnlyList<Unparsed> _unparsed = [];

    private void Render(object? payload)
    {
        var masked = PayloadMask.Apply(payload);
        _rendered = masked.Value?.ToJsonString(Written) ?? "null";
        _unparsed = masked.Unparsed;
        output.WriteLine($"when: Apply -> {_rendered}, unparsed [{string.Join(", ", _unparsed)}]");
    }

    [When("the payload is a person with the name {string}, the pin {string}, the note {string}, and the draft {string}")]
    public void WhenAPerson(string name, string pin, string note, string draft) => Render(new Person(name, pin, note, draft));

    [When("the payload is a team {string} led by {string} with the pin {string}, a member {string} with the pin {string}, and the vault code {string}")]
    public void WhenATeam(string title, string lead, string leadPin, string member, string memberPin, string code) =>
        Render(new Team(title, new Person(lead, leadPin, "", ""), [new Person(member, memberPin, "", "")], new Vault(code)));

    [When("the payload is a login with the user {string} and the password {string}")]
    public void WhenALogin(string user, string password) => Render(new Login(user, password));

    [When("the payload is an account with the user {string} and the password {string}")]
    public void WhenAnAccount(string user, string password) => Render(new Account(user, password));

    [When("the payload is a map with {string} holding nothing")]
    public void WhenAMapHoldingNothing(string key) => Render(new Dictionary<string, object?> { [key] = null });

    [When("the payload is a map with {string} holding the bodies {string}, {string}, and {string}")]
    public void WhenAMapHoldingBodies(string key, string b1, string b2, string b3) =>
        Render(new Dictionary<string, object?> { [key] = new[] { b1, b2, b3 }.Select(Encoding.UTF8.GetBytes).ToList() });

    [When("the payload is a map with {string} = {string} and {string} = {string}")]
    public void WhenAMap(string k1, string v1, string k2, string v2) => Render(new Dictionary<string, string> { [k1] = v1, [k2] = v2 });

    [When("the payload is a map of lists with {string} = {string} and {string} = {string}")]
    public void WhenAMapOfLists(string k1, string v1, string k2, string v2) =>
        Render(new Dictionary<string, string[]> { [k1] = [v1], [k2] = [v2] });

    [When("the payload is the body {string}")]
    public void WhenTheBody(string body) => Render(Encoding.UTF8.GetBytes(body));

    [When("the payload is a map with {string} holding the text {string}")]
    public void WhenAMapHoldingTheText(string key, string text) => Render(new Dictionary<string, object?> { [key] = text });

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

    [When("the payload is an upload {string} with the content {string}")]
    public void WhenAnUpload(string name, string content) => Render(new Upload(name, Encoding.UTF8.GetBytes(content)));

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
        Assert.Equal(table.Rows.Select(row => new Unparsed(row["path"], int.Parse(row["bytes"]))).ToList(), _unparsed);

    [Then("the mask should report the unparsed body {string} of {int} bytes")]
    public void ThenTheUnparsedBody(string path, int bytes) => Assert.Equal([new Unparsed(path, bytes)], _unparsed);

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
