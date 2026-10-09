---
description: A tagged specification beside the TypeScript module implementing one rule
project_name: "{Package}"
name: "{rule}.feature"
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing-in-typescript
  - element/file
---

# Goals
- Describe the module's observable rule with correctly tagged happy and negative scenarios.

# Naming convention
`src/{package}/features/{rule}.feature`, beside `src/{package}/test/{rule}.steps.ts`.

# Implementation changes
```gherkin
@type/domain
Feature: Email format
  A caller checks the address before using it.

  @category/happy
  Scenario: A well-formed address is accepted
    Given the input "reader@example.com"
    When the email format rule validates it
    Then the result is valid

  @category/negative
  Scenario: An address without a host is rejected
    Given the input "reader@"
    When the email format rule validates it
    Then the result is invalid with error "MISSING_HOST"
```

# Rule changes
- Replace the illustrative email rule and its error code with the production module's actual behavior, keeping exactly one type tag per feature and one category per scenario or Examples block.
- Set no `@status/validated` tag; only a person confirms a specification. A todo or broken scenario carries its reason in the adjacent comment and stays excluded from execution.

# Check list
- [ ] Every step calls and asserts the real implementation rather than reproducing its rule.
- [ ] Feature and scenario tags come from the closed lists in [cucumber-testing](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md).
