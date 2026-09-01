---
description: Link {Module}.Domain.Rules.Spec's @format-tagged scenarios in and add a step-definition class proving the VO/Entity fail-fast adapter redirects to the same rule
project_name: "{Module}.Domain.Tests"
name: "{Module}.Domain.Tests.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-rules
  - element/module-domain-tests-csproj
---

# Goals
- Prove that the VO constructor / Entity method a rule was redirected into still enforces the exact same condition described in `{Module}.Domain.Rules.Spec`, without duplicating that scenario's text

# Rule changes

## MUST
- Link `{Module}.Domain.Rules.Spec`'s `@format`-tagged scenarios in via `<None Include>` — never copy their text into a local `.feature` file
- Add one step-definition class per redirected rule, calling the VO constructor / Entity method, asserting `DomainException` on the invalid path
- Never also prove an `@semantic`/`@domain`-tagged scenario here — those belong to `{Module}.Application.Tests`

