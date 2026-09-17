---
name: mutation-tool-choice
description: Which mutation-testing tool this solution uses for Go
problem: solution-conformance-testing requires a concrete mutation-testing tool per stack, recorded once in its own shared mutation-tool-per-stack ADR. Go needed an entry.
decision: gremlins (github.com/go-gremlins/gremlins)
tags:
  - solution/go-conformance-testing
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]'s own [[skills/common-workflow/test/solution-conformance-testing.skill/adr/mutation-tool-per-stack.md|mutation-tool-per-stack ADR]] records one mutation-testing tool per stack (Stryker for .NET/TypeScript, Mutmut for Python) so the choice is made once instead of re-argued per project. Go had no entry yet.

# Selected variant

**Selected variant:** [[#gremlins (selected)]]

# Searched variants

## gremlins (selected)

### Description

`github.com/go-gremlins/gremlins` — a Go-native mutation-testing tool, installed as a CLI (`go install .../gremlins/cmd/gremlins@<version>`) and run via `gremlins unleash`, with its own JSON report format and a `--diff <ref>` flag for delta-scoped runs.

### Benefits

- Go-native — no JVM/Node/Python runtime to install alongside the Go toolchain, matching the minimal-dependency spirit of this catalog's baseline.
- Ships a `--diff` flag that maps directly onto the parent contract's `ONLY_DELTA=true DELTA_BASE=<ref>` toggle, with no extra scripting needed to compute the changed-file set.
- Already the tool a real Go service on this family (`tmp/tg-bot-service`) uses in production, with a working `Makefile` integration to ground this solution's `Repository.extend.md` against.

### Costs

- Smaller community and slower release cadence than Stryker or Mutmut — this solution's `tools/normalize_mutation` reads gremlins' report defensively (see its own file's prose) because the exact JSON field names are more likely to shift between versions than a more mature tool's would.

## go-mutesting

### Description

An older, community Go mutation-testing tool (`github.com/zimmski/go-mutesting`), one of the first available for Go.

### Benefits

- Long-established, simple mutation operator set.

### Costs

- Effectively unmaintained (no active release activity), with known incompatibilities against modern Go module layouts.
- No delta/diff-scoped run mode, so every `ONLY_DELTA=true` request would need bespoke scripting this catalog would then own and maintain.

## No mutation testing for Go yet — leave the ADR's Go row unfilled

### Description

Ship `solution-go-conformance-testing` with only Cucumber and coverage; defer the mutation-testing tool decision to a later change.

### Benefits

- Avoids picking a smaller, faster-moving tool before it has proven itself further.

### Costs

- Violates the parent solution's own Core Principle ("Mutation testing verifies testing quality — coverage alone only proves a code path executed, not that its result was checked") and its MUST rule that coverage is never optional and the four `make` targets always exist — `mutation-test` is not an optional target this catalog can skip.
- The reference implementation this catalog is grounded in already has a working `gremlins` integration; deferring the decision would mean *not* following the concrete, already-proven ground truth for no real reason.
