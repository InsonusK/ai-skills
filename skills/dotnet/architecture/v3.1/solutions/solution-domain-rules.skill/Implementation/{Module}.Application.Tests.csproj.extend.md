---
description: Link {Module}.Domain.Rules.Spec's @semantic/@domain-tagged scenarios in and add a step-definition class proving the DtoValidator/{Feature}Check collect-all adapter redirects to the same rule
project_name: "{Module}.Application.Tests"
name: "{Module}.Application.Tests.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-rules
  - element/module-application-tests-csproj
---

# Goals
- Prove that the `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check` a rule was redirected into still enforces the exact same condition described in `{Module}.Domain.Rules.Spec`, without duplicating that scenario's text

# Rule changes

## MUST
- Link `{Module}.Domain.Rules.Spec`'s `@semantic`/`@domain`-tagged scenarios in via `<None Include>` — never copy their text into a local `.feature` file
- Add one step-definition class per redirected rule, calling the real validator/`{Feature}Check`, asserting `ValidationResult` on the outcome
- Never also prove an `@format`-tagged scenario here — those belong to `{Module}.Domain.Tests`

