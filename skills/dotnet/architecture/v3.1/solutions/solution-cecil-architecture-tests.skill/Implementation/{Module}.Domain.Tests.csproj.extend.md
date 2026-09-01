---
description: Add the Architecture/ folder holding all four Mono.Cecil structural checks to {Module}.Domain.Tests
project_name: "{Module}.Domain.Tests"
name: "{Module}.Domain.Tests.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/cecil-architecture-tests
  - element/module-domain-tests-csproj
---

# Goals
- Give `{Module}.Domain.Tests` a build-time guarantee, over compiled IL, that plain unit/BDD tests cannot give by construction

# Rule changes

## MUST
- Add `/Architecture` under `{Module}.Domain.Tests`, holding the two test classes and their companion `.feature` files
- Reference `Mono.Cecil`
- Never load any assembly other than the module's own `{Module}.Domain`/`{Module}.Domain.Rules` from these tests

