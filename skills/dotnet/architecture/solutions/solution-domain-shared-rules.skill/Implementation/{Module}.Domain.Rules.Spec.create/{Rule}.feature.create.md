---
description: One .feature file per rule — the shared Gherkin source every redirecting layer (rule itself, VO/Entity, DtoValidator) proves against
project_name: "{Module}.Domain.Rules.Spec"
name: "{Rule}.feature"
element_kind: feature
change_kind: create
tags:
  - solution/domain-shared-rules
  - element/rule-feature
---

# Goals
- Describe one rule's condition once, in plain language, provable by every layer that redirects to it
- Make a rule's classification (Format/Semantic/Domain) machine-readable via tags, not only documented in prose
- Stay a portable artifact — copyable unchanged into a frontend or other-language repo that re-proves the same rule with its own step definitions

# Naming convention
| use case | file name pattern | file name |
| -------- | ------------------ | ---------- |
| Feature file for a rule on a named wrapper | `{classification}/{Concept}.feature` | `format/Complexity.feature` |
| Feature file for a rule on an anonymous-tuple wrapper | `{classification}/{Concept}.feature` | `domain/AccountWithdrawal.feature` |

# Implementation changes

Worked example — `format/Complexity.feature` (Format-classified, one layer: the rule itself and the VO both prove it):

```gherkin
Feature: Complexity must be non-negative

  @format
  Scenario: Negative complexity is rejected
    Given a complexity value of -1
    When the complexity value is checked
    Then the check fails with error code "TaskModule.Complexity.NonNegative"

  @format
  Scenario: Zero complexity is accepted
    Given a complexity value of 0
    When the complexity value is checked
    Then the check passes
```

Worked example — `domain/AccountWithdrawal.feature` (Domain-classified, proven at the rule itself and at the async `{Feature}Check`):

```gherkin
Feature: Withdrawal amount must not exceed account balance

  @domain
  Scenario: Withdrawal exceeding balance is rejected
    Given an account balance of 100
    And a withdrawal amount of 150
    When the withdrawal is checked
    Then the check fails with error code "TaskModule.AccountWithdrawal.InsufficientBalance"

  @domain
  Scenario: Withdrawal within balance is accepted
    Given an account balance of 100
    And a withdrawal amount of 50
    When the withdrawal is checked
    Then the check passes
```

A rule reused at two layers gets one file per classification folder — e.g. a `Schedule` rule proven both as a VO-level Format check and, ad hoc, as a DTO-level Semantic check, has `format/Schedule.feature` (checking `SoftSchedule` in isolation) and `semantic/Schedule.feature` (checking two of a DTO's own separate date fields) — never one file mixing both tags, since each consuming project links whole files by folder and would receive scenarios it has no step definitions for.

# Rule changes

## MUST
- Carry exactly one classification tag (`@format`/`@semantic`/`@domain`) per scenario, equal to the file's folder
- Assert a specific error code on the invalid path, not just "is invalid"
- Describe the rule's condition only — never the mechanics of which class/adapter proves it (that's the step definition's job, in whichever test project binds it)
- Never mix classification tags on one scenario, or across the scenarios of one file
- Never reference a specific layer's type (`SoftComplexity`, `ComplexityPropertyValidator`), a .NET/C#/FluentValidation term, or the adapter that proves it — keep the Gherkin text domain-language only, so the file is portable both across this service's layers and into a consumer repo written in another language; the step definition is what's layer- and language-specific
- Keep the rejection-code string exact — a consumer that re-proves this scenario asserts the same code, so it is part of the contract

# Check list
- [ ] Every scenario has exactly one classification tag, and all of a file's scenarios share the tag of its folder
- [ ] Every invalid-path scenario asserts a specific error code, verbatim
- [ ] Gherkin text is domain language only — no .NET types, no adapter references — so the file stays portable to another language
- [ ] File name matches the rule class it describes
