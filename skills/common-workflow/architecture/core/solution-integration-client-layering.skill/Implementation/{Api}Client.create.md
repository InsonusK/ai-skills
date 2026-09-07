---
description: Composes 1..n Integration calls into the application's own model, hiding the API's shape and its Integration calls from the rest of the application
name: "{Api}Client"
element_kind: class
change_kind: create
---

# Goals
- Give the application a single, stable entry point per external API, independent of that API's response shape
- Compose one or more Integration calls into one application-facing result

# Core Principles
- Client is the only caller of [[./{Api}Integration.create.md|Integration]]; application/business code never calls Integration directly
- Client's return type is an application model — it must never expose an Integration response type to its caller
- Follows [[skills/common-workflow/develop/solid-decomposition.skill/solid-decomposition.skill.md|solid-decomposition]]: one Client unit has exactly one reason to change — the application's data needs for that API, not the API's own contract

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ------------------ | --------- |
| Client unit for one external API | {Api}Client | PaymentsClient | {api}_client | payments_client |
| application model returned to callers | {Resource} | Charge | {resource} | charge |

# Implementation changes
Client exposes one method per application use case that needs data from the API. Each method:
1. calls one or more Integration methods;
2. maps their typed responses into the application's own model (renaming/reshaping/combining fields as the application needs, independent of the API's naming);
3. returns the application model.

```pseudocode
class PaymentsClient:
    constructor(paymentsIntegration):
        this.paymentsIntegration = paymentsIntegration

    function getCharge(chargeId) -> Charge:
        response = this.paymentsIntegration.getCharge(chargeId)

        return Charge(
            id: response.id,
            isSettled: response.status == "succeeded",
            amount: Money(response.amountMinorUnits, response.currency)
        )

    function getChargeWithRefunds(chargeId) -> ChargeWithRefunds:
        charge = this.getCharge(chargeId)
        refunds = this.paymentsIntegration.listRefunds(chargeId)

        return ChargeWithRefunds(
            charge: charge,
            refundedAmount: sumAmounts(refunds)
        )
```

# Rule changes

## MUST
- Every Client method must return an application model, never an Integration response type
  - Violation: Client returning the Integration response type unchanged.
  - Risk: the application ends up coupled to the API's shape anyway, defeating the point of the split — a contract change still ripples into application code.
  - Fix: always map into an application model, even when it currently looks identical to the Integration response.
- Client must call Integration for every piece of external data it needs — it must never call the raw HTTP/API client itself, and it must never parse raw API responses itself; response parsing and contract validation belong to Integration
- A Client method may call 1..n Integration methods to assemble one application model
- Route every external-data need through Client — application/business code must never call Integration or the raw API client directly, bypassing Client
  - Risk: application code becomes coupled to the API's contract in multiple places; tests for that code can no longer mock a single stable boundary.
  - Fix: route every external-data need through Client.

## SHOULD
- Client methods should be named after the application's use case (e.g. `getChargeWithRefunds`), not after the underlying API operation

# Check list
- [ ] Every Client method returns an application model, not an Integration response type
- [ ] Client never calls the raw HTTP/API client directly — only Integration
- [ ] No parsing or contract validation happens inside Client
- [ ] Application/business code depends only on Client, never on Integration

# Unittest TestCases
- [ ] WHEN Integration returns a typed response THEN Client maps it into the expected application model
- [ ] WHEN a Client method needs more than one Integration call THEN it combines their results into a single application model
- [ ] WHEN Integration raises a contract error THEN Client either propagates it or translates it into an application-level error, without swallowing it silently
- [ ] WHEN application code under test depends on Client THEN mocking Client alone is sufficient — no Integration or HTTP mocking is required
