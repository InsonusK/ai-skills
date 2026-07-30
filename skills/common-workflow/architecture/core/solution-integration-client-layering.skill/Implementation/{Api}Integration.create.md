---
description: Calls one external API endpoint and maps its raw response into a typed object, handling contract mismatches
name: "{Api}Integration"
element_kind: class
change_kind: create
---

# Goals
- Call one external API endpoint and return a typed object, never a raw string/unparsed response
- Detect and reject responses that don't match the expected contract, instead of passing malformed data upstream

# Core Principles
- One Integration method call maps to one API endpoint call
- The typed object returned by Integration mirrors the API's contract, not the application's model — application-shape mapping belongs to [[./{Api}Client.create.md|Client]]
- Follows [[skills/common-workflow/develop/solid-decomposition.skill/solid-decomposition.skill.md|solid-decomposition]]: one Integration unit has exactly one reason to change — the API endpoint's contract

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ------------------ | --------- |
| Integration unit for one external API | {Api}Integration | PaymentsIntegration | {api}_integration | payments_integration |
| typed response object | {Api}{Resource}Response | PaymentsChargeResponse | {api}_{resource}_response | payments_charge_response |
| contract-mismatch error | {Api}ContractError | PaymentsContractError | {api}_contract_error | payments_contract_error |

# Implementation changes
Integration exposes one method per API endpoint it wraps. Each method:
1. calls the API endpoint (via whatever HTTP/transport client the target stack provides);
2. parses the raw response body into a typed object;
3. validates the parsed object against the expected shape (required fields present, types match);
4. returns the typed object on success, or raises a typed contract error naming the endpoint and what was unexpected.

```pseudocode
class PaymentsIntegration:
    constructor(httpClient, baseUrl):
        this.httpClient = httpClient
        this.baseUrl = baseUrl

    function getCharge(chargeId) -> PaymentsChargeResponse:
        rawResponse = this.httpClient.get(this.baseUrl + "/charges/" + chargeId)
        parsed = tryParseJson(rawResponse.body)

        if parsed is null:
            raise PaymentsContractError("getCharge: response body is not valid JSON")

        if not hasFields(parsed, ["id", "status", "amount", "currency"]):
            raise PaymentsContractError("getCharge: missing expected fields in response")

        return PaymentsChargeResponse(
            id: parsed.id,
            status: parsed.status,
            amountMinorUnits: parsed.amount,
            currency: parsed.currency
        )
```

# Rule changes

## MUST
- Every Integration method must return a typed object; it must never return a raw string, untyped dictionary, or unparsed response
- Every Integration method must validate the response shape and raise a typed contract error (naming the endpoint and the mismatch) when the shape doesn't match what's expected
- Integration must not transform its typed response into the application's model — that belongs to Client

## SHOULD
- Integration methods should be named after the API operation they wrap (e.g. `getCharge`, `listCharges`), not after the application's use case

## MUST NOT
- Integration must not contain application-specific business rules or decisions — only request construction, response parsing, and contract validation belong here
- Integration must not be called directly from application/business code — only Client may call Integration

# Anti-patterns
- **Returning the raw parsed JSON/dict instead of a typed object**
  - Consequence: every caller has to know the API's field names and types; a contract change breaks every call site silently (wrong type, missing field) instead of failing at one boundary
  - Instead: always map into a typed object with a fixed set of named fields, validated at parse time

- **Swallowing parse/validation failures and returning a partially-filled object**
  - Consequence: malformed data flows into the application and fails far away from its real cause, making the contract change hard to diagnose
  - Instead: raise a typed contract error immediately, naming the endpoint and what was unexpected

# Check list
- [ ] Every Integration method returns a typed object on success
- [ ] Every Integration method raises a typed contract error when the response shape doesn't match what's expected
- [ ] No application-model mapping happens inside Integration
- [ ] No business rule/decision happens inside Integration
- [ ] Nothing outside Client calls Integration directly

# Unittest TestCases
- [ ] WHEN the API returns a well-formed response THEN Integration returns a typed object with all expected fields populated
- [ ] WHEN the API returns a response missing an expected field THEN Integration raises a typed contract error naming the missing field
- [ ] WHEN the API returns a response with a field of an unexpected type THEN Integration raises a typed contract error
- [ ] WHEN the API returns a non-JSON/unparsable body THEN Integration raises a typed contract error instead of propagating a parse exception
- [ ] WHEN the API call itself fails (network/timeout/HTTP error status) THEN Integration surfaces a distinguishable error rather than a contract error
