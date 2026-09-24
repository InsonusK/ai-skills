---
name: tooling-moved-to-stack-testing-skill
description: Where the .NET test tooling (Makefile, scripts, Reqnroll/coverlet/Stryker.NET choice, report) lives relative to the dotnet catalog's test-project layout
problem: This solution mixed two concerns — the stack-generic .NET test tooling that implements solution-conformance-testing, and the dotnet plateau catalog's own rule of one test project per production project. The go, python, and typescript variants of the same tooling are pure tooling and live under skills/{stack}/testing/, next to cucmber-testing-in-{stack}.
decision: Move the tooling into skills/dotnet/testing/solution-conformance-testing-in-dotnet.skill; keep only the test-project layout here, depending on it. The solution name stays so the plateaus' created_by links to its csproj Implementation files stay valid.
tags:
  - solution/dotnet-conformance-testing
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The Makefile, the `unit-test.sh`/`mutation-test.sh`/`test-report.sh` scripts, `reqnroll.json`, `report-template/index.html`, and the Reqnroll/coverlet/Stryker.NET tool choice are the same for any .NET solution, regardless of how it is split into projects. The one-test-project-per-production-project rule, by contrast, depends on the dotnet plateau catalog's `solution-sln-structure` and its Variation Points (`{Module}.Domain.Tests` only with VP1). Keeping both in one solution under `architecture/solutions/` hid the reusable tooling inside one catalog, and made a stack-generic testing skill depend on catalog-specific project names.

# Selected variant

**Selected variant:** [[#Split tooling into the stack testing skill, keep layout here (selected)]]

# Searched variants

## Split tooling into the stack testing skill, keep layout here (selected)

### Description

`Implementation/Repository.extend.md`, `templates/`, and `adr/testing-tool-choice.md` move to [[skills/dotnet/testing/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]]. This solution keeps the per-production-project `*.Tests.csproj` Implementation files and `{Rule}Steps.cs`, and `depends_on` the testing skill.

### Benefits

- Matches the go/python/typescript layout: `skills/{stack}/testing/solution-conformance-testing-in-{stack}.skill`.
- The testing skill is usable by any .NET solution, not only one built from this catalog.
- The name `solution-dotnet-conformance-testing` stays, so the 39 plateau structure skills that list it in `created_by` for their test csproj files keep resolving.

### Costs

- Two skills to read instead of one when setting up a new .NET catalog's tests.
- The name `solution-dotnet-conformance-testing` now describes a test-project layout rather than the whole conformance gate.

## Move the whole solution to skills/dotnet/testing/

### Description

Move every file, including the per-production-project csproj Implementation files, to the stack testing folder.

### Benefits

- One skill, one place.

### Costs

- The stack testing skill would carry `{Module}.Domain`/VP1/`solution-sln-structure` knowledge, which belongs to one catalog only.
- Every plateau `created_by` link to the csproj Implementation files would have to be repointed.

## Leave everything in architecture/solutions/

### Description

Keep the current single solution under `skills/dotnet/architecture/solutions/`.

### Benefits

- No change.

### Costs

- .NET would remain the only stack whose conformance-testing tooling is not under `skills/{stack}/testing/`, and the tooling stays unreachable for .NET projects outside this catalog.
