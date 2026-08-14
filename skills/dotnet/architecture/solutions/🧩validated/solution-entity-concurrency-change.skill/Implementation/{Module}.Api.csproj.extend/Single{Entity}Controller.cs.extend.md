---
description: Add ETag encoding on GET and If-Match decoding on PUT/PATCH
project_name: "{Module}.Api"
name: "Single{Entity}Controller.cs"
element_kind: class
change_kind: extend
---

# Goals
- Encode entity `Version` into `ETag` response header on every GET for a mutable entity
- Decode `If-Match` request header and return 412 before dispatch if missing or malformed
- Populate `Versions` on the update command from the decoded `If-Match` value

# Core Principles
- ETag format: `"<base64>"` — surrounding double quotes are part of the HTTP ETag format
- `ETagEncoder.Encode` builds the versions dictionary — entity name string must match `EntityVersionResolverFactory` keys exactly
- If `If-Match` missing or `ETagEncoder.Decode` returns null → return `StatusCode(412)` immediately, before `_sender.Send()`
- `Versions` passed directly as command constructor argument — no manual construction in controller

# Full ETag flow
```
GET /{entity}/2
    ↓
Handler returns {Entity}Dto with Version = 3
    ↓
Controller calls ETagEncoder.Encode({"{Entity}": {"2": 3}})
    ↓
Response.Headers.ETag = "\"eyJUYXNrIjp7IjIiOjN9fQ==\""
← 200 OK + ETag header

PUT /{entity}/2  { title: "New title" }
If-Match: "eyJUYXNrIjp7IjIiOjN9fQ=="
    ↓
Controller: ifMatch missing? → 412
Controller: ETagEncoder.Decode(ifMatch) null? → 412
Controller: versions = {"{Entity}": {"2": 3}}
Controller: _sender.Send(new Update{Entity}Command(2, "New title", versions))
    ↓
ConcurrencyBehavior: loads {Entity}#2, checks Version == 3
    ↓ match
Handler: updates entity
UnitOfWorkBehavior: commits
← 204 No Content

    ↓ mismatch ({Entity}#2 updated by another client in the meantime)
ConcurrencyBehavior: returns Result.Conflict
← 409 Conflict + ProblemDetails
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Single entity controller | `Single{Entity}Controller` | `Single{Entity}Controller` | `Single{Entity}Controller.cs` | `Single{Entity}Controller.cs` |

# Implementation changes

GET extended with ETag encoding:

```csharp
// {Module}.Api/Controllers/{Entity}/Single{Entity}Controller.cs
[HttpGet]
[ProducesResponseType(typeof({Entity}Dto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<{Entity}Dto>> Get(int id, CancellationToken ct)
{
    var result = await _sender.Send(new Get{Entity}Query(id), ct);

    return result.Status switch
    {
        ResultStatus.Ok => BuildOkWithETag(result.Value, id),
        ResultStatus.NotFound => NotFound(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Error => StatusCode(
            StatusCodes.Status500InternalServerError,
            ResultExtensions.ToProblemDetails(result.Errors)),
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}' for Get{Entity}Query.")
    };
}

private OkObjectResult BuildOkWithETag({Entity}Dto dto, int id)
{
    var etag = ETagEncoder.Encode(new()
    {
        ["{Entity}"] = new() { [id] = dto.Version }
    });
    Response.Headers.ETag = $"\"{etag}\"";
    return Ok(dto);
}
```

PUT extended with If-Match guard and Versions population:

```csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Update(
    int id,
    [FromBody] Update{Entity}Request request,
    [FromHeader(Name = "If-Match")] string? ifMatch,
    CancellationToken ct)
{
    // 412 — If-Match not supplied or malformed
    if (string.IsNullOrEmpty(ifMatch))
        return StatusCode(StatusCodes.Status412PreconditionFailed);

    var versions = ETagEncoder.Decode(ifMatch);
    if (versions is null)
        return StatusCode(StatusCodes.Status412PreconditionFailed);

    var command = new Update{Entity}Command(id, request.Title, versions);
    var result = await _sender.Send(command, ct);

    return result.Status switch
    {
        ResultStatus.NoContent => NoContent(),
        ResultStatus.Invalid => BadRequest(
            ResultExtensions.ToProblemDetails(result.ValidationErrors)),
        ResultStatus.NotFound => NotFound(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Conflict => Conflict(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Error => StatusCode(
            StatusCodes.Status500InternalServerError,
            ResultExtensions.ToProblemDetails(result.Errors)),
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}' for Update{Entity}Command.")
    };
}
```

> **Note:** `{Entity}Dto` must include `Version` as a property so the controller can encode it into the ETag:
> ```csharp
> public record {Entity}Dto(int Id, string Title, string Status, int AssigneeId, uint Version);
> ```
# Rule changes

## MUST
- GET for mutable entity sets `Response.Headers.ETag` with encoded versions
- PUT/PATCH checks `If-Match` presence — returns 412 immediately if missing
- PUT/PATCH calls `ETagEncoder.Decode` — returns 412 if result is null
- `Versions` passed to command from decoded `If-Match` — never constructed in controller
- 412 added to `[ProducesResponseType]` on all PUT/PATCH endpoints for mutable entities
- DTO returned by GET for mutable entity includes `Version` field

## MUST NOT
- GET for immutable entity set ETag header — immutable entities have no version
- `Versions` hardcoded or constructed in controller — always from decoded `If-Match`
- Controller return 400 for missing `If-Match` — 412 Precondition Failed is correct

# Anti-patterns
- ETag encoding only primary entity version — misses secondary entity conflicts when command touches multiple entities
- Controller returns 400 for missing `If-Match` — 412 Precondition Failed is correct

# Check list
- [ ] GET sets `Response.Headers.ETag`
- [ ] PUT/PATCH checks `If-Match`
- [ ] 412 returned if `If-Match` missing or malformed
- [ ] `Versions` passed to command from decoded `If-Match`
- [ ] 412 declared in `[ProducesResponseType]`
- [ ] DTO includes `Version` field
- [ ] `switch` default arm throws `InvalidOperationException`

# Unittest TestCases
- [ ] WHEN applied THEN Encode entity Version into ETag response header on every GET for a mutable entity
- [ ] WHEN applied THEN Decode If-Match request header and return 412 before dispatch if missing or malformed
- [ ] WHEN applied THEN Populate Versions on the update command from the decoded If-Match value
- [ ] WHEN applied THEN ETag format: "<base64>" — surrounding double quotes are part of the HTTP ETag format
- [ ] WHEN applied THEN ETagEncoder.Encode builds the versions dictionary — entity name string must match EntityVersionResolverFactory keys exactly
- [ ] WHEN applied THEN If If-Match missing or ETagEncoder.Decode returns null → return StatusCode(412) immediately, before _sender.Send()
- [ ] WHEN applied THEN Versions passed directly as command constructor argument — no manual construction in controller
- [ ] WHEN verified THEN GET sets Response.Headers.ETag
- [ ] WHEN verified THEN PUT/PATCH checks If-Match
- [ ] WHEN verified THEN 412 returned if If-Match missing or malformed
- [ ] WHEN verified THEN Versions passed to command from decoded If-Match
- [ ] WHEN verified THEN 412 declared in [ProducesResponseType]
- [ ] WHEN verified THEN DTO includes Version field
- [ ] WHEN verified THEN switch default arm throws InvalidOperationException
- [ ] WHEN naming 'Single entity controller' THEN pattern matches convention
