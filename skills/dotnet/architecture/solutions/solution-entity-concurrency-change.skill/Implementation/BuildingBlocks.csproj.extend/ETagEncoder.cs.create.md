---
description: Encodes/decodes entity versions as base64 JSON ETags
project_name: BuildingBlocks
name: ETagEncoder.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-concurrency-change
  - element/etagencoder-cs
---

# Goals
- Encode a dictionary of entity versions as a base64 JSON string suitable for the HTTP `ETag` header
- Decode an `If-Match` header value back to the versions dictionary — return null on malformed input

# Core Principles
- Static class — no instance, no DI registration needed
- Encode: `Dictionary<string, Dictionary<int, uint>>` → base64 JSON string
- Decode: base64 JSON string → `Dictionary<string, Dictionary<int, uint>>?` — null on any error
- Decode swallows exceptions and returns null — malformed ETag handled by controller as 412

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| ETag encode/decode | `ETagEncoder` | `ETagEncoder` | `ETagEncoder.cs` | `ETagEncoder.cs` |

# Implementation changes

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

# Rule changes

## MUST
- `Decode` returns null on any exception — never throws
- `Encode` produces a string usable directly as an `ETag` header value (without surrounding quotes — controller adds quotes)

## SHOULD
- Avoid having `Decode` throw on malformed input — forces callers to catch exceptions

# Check list
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `Decode` returns null on any exception
- [ ] `Encode` produces valid base64 string

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
