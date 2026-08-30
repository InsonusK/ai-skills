---
name: plateau-v1--module-domain-rules-spec
description: Directory {Module}.Domain.Rules.Spec in the v1 plateau — not a project, holds one shared .feature file per rule
whenToUse: when writing or extending the Gherkin scenarios that describe a centralized rule, provable from every layer that redirects to it
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/directory
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give a rule one Gherkin source, provable from every layer that redirects to it — the rule itself, its VO/Entity fail-fast adapter, its DtoValidator collect-all adapter — without three copies of the same scenario text

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create.md|{Module}.Domain.Rules.Spec.create]]

# Core Principles
- Not a `.csproj` — produces no assembly, is never referenced by anything; a plain directory of `.feature` files, sibling to `{Module}.Domain.Rules`
- One `.feature` file per rule class, named after the rule
- Every scenario carries exactly one classification tag: `@format`, `@semantic`, or `@domain`, matching the rule's own classification in `{Module}.Domain.Rules`
- Every consuming test project links the physical `.feature` file in via its own `.csproj` (`<None Include>`), never copies scenario text

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create.md|{Module}.Domain.Rules.Spec.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules.Spec
```

## Project Structure
- /{Module}.Domain.Rules.Spec
  - {Rule}.feature   (see [[../{Module}.Domain.Rules.Tests/classes/plateau-v1--class-rule-rule-steps.skill.md|class-rule-rule-steps]] for the worked `.feature` examples)

## Who links this directory in
| Consumer | Scenarios taken | Adapter proven |
| --- | --- | --- |
| `{Module}.Domain.Rules.Tests` | all, regardless of tag | the rule's own `IsValid()`/`Check()` |
| `{Module}.Domain.Tests` | `@format` only | VO constructor / Entity method (fail-fast) |
| `{Module}.Application.Tests` | `@semantic`/`@domain` only | `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check` (collect-all) |

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create.md|{Module}.Domain.Rules.Spec.create]]

## What Does NOT Belong Here
- Any `.cs` file, any step definition, any `.csproj` — this directory compiles nothing

## Allowed Dependencies
- None — not a project, nothing to reference

# Rules
MUST:
- Contain only `.feature` files, one per rule class
- Every scenario carry exactly one of `@format`/`@semantic`/`@domain`
MUST NOT:
- Contain a `.csproj`, a `.cs` file, or any step definition
- Be referenced as a project by any other `.csproj`

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create.md|{Module}.Domain.Rules.Spec.create]]

# Check list
- [ ] Directory contains only `.feature` files, no code, no project file
- [ ] Every scenario has exactly one classification tag
- [ ] Every scenario is linked into and proven by every test project its tag routes it to

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create.md|{Module}.Domain.Rules.Spec.create]]
