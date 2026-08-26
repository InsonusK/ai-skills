# Generated-constant uniqueness and format

When rejection codes (or any other generated `public const string`) move from one central registry file to living next to each rule that produces them (see [solution-domain-rules.skill](../../../🧩validated/solution-domain-rules.skill/solution-domain-rules.skill.md)), the central file's one real benefit — a duplicate or malformed code was visible by eye in one place — disappears. This test replaces that eyeballing with a build-time check.

Reference implementation: `TaskModuleArchitectureTests.RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat`.

```csharp
private static readonly Regex RejectionCodeFormat = new(@"^[A-Za-z]+\.[A-Za-z]+\.[A-Za-z]+$", RegexOptions.Compiled);

[Fact]
public void RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat()
{
    using var rulesAssembly = LoadDomainRulesAssembly();

    var codes = rulesAssembly.MainModule.Types
        .Where(t => t.Namespace == RulesNamespace)
        .SelectMany(t => t.Fields)
        .Where(f => f.IsPublic && f.IsStatic && f.HasConstant && f.FieldType.FullName == "System.String" && f.Name.EndsWith("Code", StringComparison.Ordinal))
        .Select(f => (Declaration: $"{f.DeclaringType.Name}.{f.Name}", Value: (string)f.Constant))
        .ToList();

    Assert.NotEmpty(codes);

    var malformed = codes.Where(c => !RejectionCodeFormat.IsMatch(c.Value)).ToList();
    Assert.True(malformed.Count == 0,
        $"Rejection codes must match {{ModuleName}}.{{Class}}.{{Reason}}: {string.Join(", ", malformed.Select(c => $"{c.Declaration}={c.Value}"))}");

    var duplicates = codes.GroupBy(c => c.Value).Where(g => g.Count() > 1).ToList();
    Assert.True(duplicates.Count == 0,
        $"Rejection codes must be unique: {string.Join(", ", duplicates.Select(g => $"{g.Key} used by {string.Join(" & ", g.Select(c => c.Declaration))}"))}");
}
```

## Why `f.HasConstant`, not reading `Field.InitialValue` or the source

`HasConstant` + `f.Constant` reads the compiled `.field literal` metadata directly — the value a `const string` bakes into the assembly, not the source text. It works uniformly regardless of how the constant was built (concatenation, `nameof`, ...), because by the time it is compiled, a `const string` is always a single literal value in metadata.

## Adapting the naming convention

The `f.Name.EndsWith("Code")` filter and the `RejectionCodeFormat` regex both encode this module's actual naming convention (`{ModuleName}.{Class}.{Reason}`, fields named `...Code`). A module with a different convention changes both, together — they must describe the same real convention, not drift apart.
