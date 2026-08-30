---
description: One .feature file per rule — the shared Gherkin source every redirecting layer (rule itself, VO/Entity, DtoValidator) proves against
project_name: "{Module}.Domain.Rules.Spec"
name: "{Rule}.feature"
element_kind: feature
change_kind: create
tags:
  - solution/domain-rules
  - element/rule-feature
---

# Goals
- Describe one rule's condition once, in plain language, provable by every layer that redirects to it
- Make a rule's classification (Format/Semantic/Domain) machine-readable via tags, not only documented in prose

# Naming convention
| use case | file name pattern | file name |
| -------- | ------------------ | ---------- |
| Feature file for a rule on a named wrapper | `{Concept}.feature` | `Complexity.feature` |
| Feature file for a rule on an anonymous-tuple wrapper | `{Concept}.feature` | `AccountWithdrawal.feature` |

# Implementation changes

Worked example — `Complexity.feature` (Format-classified, one layer: the rule itself and the VO both prove it):

```gherkin
Feature: Complexity must be non-negative

  @format
  Scenario: Negative complexity is rejected
    Given a complexity value of -1
    When ComplexityRules validates it
    Then the result is invalid with error code "TaskModule.Complexity.NonNegative"

  @format
  Scenario: Zero complexity is accepted
    Given a complexity value of 0
    When ComplexityRules validates it
    Then the result is valid
```

Worked example — `AccountWithdrawal.feature` (Domain-classified, proven at the rule itself and at the async `{Feature}Check`):

```gherkin
Feature: Withdrawal amount must not exceed account balance

  @domain
  Scenario: Withdrawal exceeding balance is rejected
    Given an account balance of 100
    And a withdrawal amount of 150
    When AccountWithdrawalRule validates it
    Then the result is invalid with error code "TaskModule.AccountWithdrawal.InsufficientBalance"

  @domain
  Scenario: Withdrawal within balance is accepted
    Given an account balance of 100
    And a withdrawal amount of 50
    When AccountWithdrawalRule validates it
    Then the result is valid
```

A rule reused at two layers gets scenarios of both tags in the same file — e.g. a `Schedule` rule proven both as a VO-level Format check and, ad hoc, as a DTO-level Semantic check, would carry both an `@format` scenario (checking `SoftSchedule` in isolation) and an `@semantic` scenario (checking two of a DTO's own separate date fields) — never one scenario claimed to prove both.

# Rule changes

## MUST
- Carry exactly one classification tag (`@format`/`@semantic`/`@domain`) per scenario
- Assert a specific error code on the invalid path, not just "is invalid"
- Describe the rule's condition only — never the mechanics of which class/adapter proves it (that's the step definition's job, in whichever test project binds it)

## MUST NOT
- Mix classification tags on one scenario
- Reference a specific layer's type (`SoftComplexity`, `ComplexityPropertyValidator`) in the Gherkin text itself — keep the feature file layer-agnostic; the step definition is what's layer-specific

# Check list
- [ ] Every scenario has exactly one classification tag
- [ ] Every invalid-path scenario asserts a specific error code
- [ ] File name matches the rule class it describes
