---
description: Normalizes gremlins' mutation-testing report into $TEST_KIND_DIR/result/mutation-test.json
project_name: tools/normalize_mutation
name: normalize_mutation
element_kind: functions
change_kind: create
tags:
  - solution/conformance-testing-in-go
  - element/tools-normalize-mutation-main-go
---

# Goals
- Turn `gremlins`' own JSON report into the normalized `{"killed","survived","timedout","noCoverage","score"}` shape [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent solution's report contract]] defines.

# Core Principles
- Reads `gremlins`' native report as its one input argument's path; never re-runs or shells out to `gremlins` itself — the `Makefile` target owns invoking the tool, this program only normalizes its output.
- `score` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated — exactly the parent contract's formula, not a re-derivation.

# Implementation changes
Copy verbatim to `tools/normalize_mutation/main.go`: [`assets/tools/normalize_mutation/main.go`](../../../assets/tools/normalize_mutation/main.go)

# Rule changes

## MUST
- Never shell out to `gremlins` from this program — it only reads the report path given as its argument.
  - Risk: this tool re-running the mutation tool duplicates the `Makefile` target's own invocation and doubles the run time.
  - Fix: `test-kind-mutation` runs `gremlins` itself and passes the resulting report's path as this program's sole argument.
- `score` must use the exact formula from [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent contract]] — `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `0.0` when the denominator is `0`.
  - Risk: a different rounding or a different denominator (e.g. including `NOT_VIABLE`/compile-error mutants) produces a badge value that does not match what a human reading the native `gremlins` report would compute.
  - Fix: sum exactly the four bucketed counts as the denominator; guard the zero-denominator case explicitly.

# Check list
- [ ] `$TEST_KIND_DIR/result/mutation-test.json` matches `{"killed","survived","timedout","noCoverage","score"}` with `score` a 1-decimal number.
- [ ] A report with zero mutants produces `"score": 0.0`, not `NaN` or a division-by-zero panic.

# Unittest TestCases
- [ ] WHEN the report has 3 killed and 1 survived mutant THEN `score` is `75.0`
- [ ] WHEN the report has zero mutants THEN `score` is `0.0` and the program does not panic
- [ ] WHEN a mutant's status is unrecognized THEN it is not counted in any bucket, and the denominator reflects only the four recognized buckets
