---
uid: 1ab94d77-f065-4853-aab5-9266b23bf0a5
name: etagencoder-class
description: Encodes/decodes entity versions as base64 JSON ETags
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Encode a dictionary of entity versions as a base64 JSON string suitable for the HTTP `ETag` header
- Decode an `If-Match` header value back to the versions dictionary — return null on malformed input

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Core Principals
- Static class — no instance, no DI registration needed
- Encode: `Dictionary<string, Dictionary<int, uint>>` → base64 JSON string
- Decode: base64 JSON string → `Dictionary<string, Dictionary<int, uint>>?` — null on any error
- Decode swallows exceptions and returns null — malformed ETag handled by controller as 412

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| ETag encode/decode | `ETagEncoder` | `ETagEncoder` | `ETagEncoder.cs` | `ETagEncoder.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Implementation
```csharp
// BuildingBlocks/Concurrency/ETagEncoder.cs
public static class ETagEncoder
{
    public static string Encode(
        Dictionary<string, Dictionary<int, uint>> versions)
    {
        var json = JsonSerializer.Serialize(versions);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static Dictionary<string, Dictionary<int, uint>>? Decode(string etag)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(etag));
            return JsonSerializer
                .Deserialize<Dictionary<string, Dictionary<int, uint>>>(json);
        }
        catch
        {
            // malformed ETag — controller returns 412
            return null;
        }
    }
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Rules
MUST:
	- `Decode` returns null on any exception — never throws
	- `Encode` produces a string usable directly as an `ETag` header value (without surrounding quotes — controller adds quotes)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Anti-patterns
- `Decode` throws on malformed input — forces callers to catch exceptions

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Check list
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `Decode` returns null on any exception
- [ ] `Encode` produces valid base64 string

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Encode a dictionary of entity versions as a base64 JSON string suitable for the HTTP ETag header
- [ ] WHEN applied THEN Decode an If-Match header value back to the versions dictionary — return null on malformed input
- [ ] WHEN applied THEN Static class — no instance, no DI registration needed
- [ ] WHEN applied THEN Encode: Dictionary<string, Dictionary<int, uint>> → base64 JSON string
- [ ] WHEN applied THEN Decode: base64 JSON string → Dictionary<string, Dictionary<int, uint>>? — null on any error
- [ ] WHEN applied THEN Decode swallows exceptions and returns null — malformed ETag handled by controller as 412
- [ ] WHEN verified THEN ETagEncoder defined in BuildingBlocks/Concurrency/ETagEncoder.cs
- [ ] WHEN verified THEN Decode returns null on any exception
- [ ] WHEN verified THEN Encode produces valid base64 string
- [ ] WHEN naming 'ETag encode/decode' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]
