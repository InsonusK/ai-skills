---
name: feature-tag-contract
description: Apply the shared testing tag contract to technical architecture checks
problem: Generated features without type and category tags fail their first unit-kind tag check
decision: Keep the existing proof/classification mechanism and add the mandatory testing tags to feature templates
tags:
  - solution/cecil-architecture-tests
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The testing contract requires one feature type and one scenario/Examples category. These templates omitted them, so generated projects failed their first tag check. Existing classification tags and proof mechanisms retain their meanings; test layout/catalog ownership stays outside W16.

# Selected variant
[[#Add the shared tags to the existing templates]]

# Searched variants

## Add the shared tags to the existing templates

**Selected.**

### Description
Apply `cucumber-testing`'s type/category contract to technical architecture checks. Shared rules keep their Format/Semantic/Domain classifications; documentary architecture features keep their explicit named plain-test exception.

### Benefits
- Generated projects pass the mandatory tag check without manual repairs.
- Existing proofs and architecture classification remain stable.

### Costs
- Documentary architecture entries still appear as not-run because the real proof is the named plain test, as its existing complexity exception states.
- Tags describe the intended test category and cannot prove assertion strength.

## Exempt catalog templates from the tag check

### Description
Keep untagged templates and make the unit tag check ignore them.

### Benefits
- No template edits.

### Costs
- Splits the contract by catalog and permits missing classifications to pass silently.
