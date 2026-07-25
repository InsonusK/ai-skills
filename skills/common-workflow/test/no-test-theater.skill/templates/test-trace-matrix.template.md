# Test Trace Matrix

Purpose: answer "what is covered by tests, and what is not" at a glance, without reading code.
Keep this file next to the code (e.g. `docs/test-trace-matrix.md` or `<module>/TESTS.md`) and update it
**in the same commit/PR** where tests are added or changed. A PR that changes tests without touching this
file is incomplete — see [no-test-theater](../no-test-theater.skill.md).

## How to fill it

- One row = one behavior scenario, not one test method. A single test may cover several scenario rows,
  and a complex scenario may need several tests.
- The "Type" column is mandatory — it lets a reviewer visually spot a skew (e.g. only happy-path rows).
- The "Assert checks" column is not "what the test does" but **exactly what it verifies** (a concrete
  value/state/call). If there is nothing concrete to write there, the test is probably weak.
- Status `⚠️ weak` means the test exists and is green, but does not verify a real result — see
  [no-test-theater](../no-test-theater.skill.md), section `# Anti-patterns`.

## Legend

- Type: `happy` | `boundary` | `negative` | `error` | `concurrency` | `security` | `regression`
- Status: ✅ covered | ⚠️ weak test | ❌ not covered | 🔧 planned

---

## Module: `<module/feature name>`

| # | Scenario (Given-When-Then) | Type | Test(s) | Assert checks | Status |
|---|------------------------------|------|---------|-------------------|--------|
| 1 | Given a valid request, When creating an entity, Then it returns 201 and the body contains an Id | happy | `Create_Returns201_WithId` | response status code + non-null Id in body | ✅ |
| 2 | Given an entity with this Id already exists, When creating again, Then it returns 409 | negative | `Create_ReturnsConflict_WhenDuplicate` | status code == 409, body error code == `DUPLICATE` | ✅ |
| 3 | Given the external service is unavailable, When creating an entity, Then it retries 3 times and returns 503 | error | — | — | ❌ |
| 4 | Given two parallel create requests for the same Id, When both complete, Then only one succeeds | concurrency | — | — | ❌ |

> Rows 3-4 are an example of a visible gap: happy + negative are covered, error/concurrency were forgotten.

---

## Module summary (fill automatically or manually at review time)

| Type | Total scenarios | ✅ Covered | ⚠️ Weak | ❌ Not covered |
|-----|-----------------|-----------|----------|---------------|
| happy | | | | |
| boundary | | | | |
| negative | | | | |
| error | | | | |
| concurrency | | | | |
| security | | | | |

**Acceptance rule:** a PR does not merge if the summary has `❌` for happy/negative/error without an
explicit "why not needed" comment on the corresponding row.

---

## Function → Test (reverse breakdown)

Filled by the agent in a **separate pass after** writing tests — not the same prompt that wrote the
tests. This catches branches that line coverage would not show as a problem.

| Public method / logic branch | Covering test(s) | Comment |
|----------------------------------|--------------------|-------------|
| `OrderService.Cancel()` — "already cancelled" branch | `Cancel_Throws_WhenAlreadyCancelled` | |
| `OrderService.Cancel()` — "already delivered" branch | — | ❌ gap |
