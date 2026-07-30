---
name: no-test-theater-python
description: pytest-specific rules for assertion strength — naming, parametrize vs. copy-paste, specific exception assertions, mock call assertions, branch coverage, mutation testing, and CLI bad-input coverage.
whenToUse: When writing or reviewing pytest tests in a Python project.
tags:
  - python
  - pytest
  - unit-testing
  - workflow/test
---

# Goal
- Add pytest/Python-specific rules on top of the language-agnostic test-quality rules.

# Scope
This skill extends [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md) — apply both together; this skill only adds Python-specific rules.

# Core Principle
- A bare `except Exception` or an unchecked mock call hides exactly the information a reviewer needs to trust the test.

# Rule

## MUST
- Name tests `test_<behavior>_when_<condition>` — not `test_1`, `test_edge_case`.
- Assert exceptions only via `pytest.raises(SpecificExceptionType, match="...")`, never a bare `Exception`.
- When a call's arguments matter (e.g. a call to an external API), assert them with `unittest.mock`/`pytest-mock`'s `assert_called_once_with(...)` using the concrete expected arguments.
- Measure coverage with `coverage run --branch` + `coverage report -m` (branch coverage), not line coverage alone.
- For CLI applications, maintain a dedicated negative-input test category: bad file paths, malformed YAML/JSON, missing required fields. This is the case most often skipped.

## SHOULD
- Use `@pytest.mark.parametrize` instead of copy-pasting near-duplicate tests with different data — this shortens the file, not the number of scenarios that must appear in the trace matrix (see [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md)).
- Run mutation testing (`mutmut` or `cosmic-ray`) on modules with business logic — parsing/validation branches are the highest-value targets.

# Anti-patterns
- **Bare `except Exception` assertion**
  - Example: `with pytest.raises(Exception): ...`
  - Consequence: passes for any failure, including an unrelated bug that raises the wrong exception type.
  - Instead: `pytest.raises(SpecificExceptionType, match="...")`.

- **Mock call unchecked**
  - Example: `mock_client.post.assert_called()` for a test whose name claims a specific payload was sent.
  - Consequence: passes even if the payload sent is wrong.
  - Instead: `mock_client.post.assert_called_once_with(url, json=expected_payload)`.

- **Copy-pasted tests instead of parametrize**
  - Example: `test_parse_1`, `test_parse_2`, `test_parse_3` differing only in the input string.
  - Consequence: illusion of many tests without proportional scenario coverage; hard to see what's actually different between them.
  - Instead: `@pytest.mark.parametrize("input,expected", [...])`.

# Check list
- [ ] No test names are `test_1`, `test_edge_case`, or similarly non-descriptive.
- [ ] Every `pytest.raises` uses a specific exception type, and a `match=` where the message matters.
- [ ] Every assertion on a mock that matters for the test's claim checks concrete call arguments.
- [ ] Coverage reviewed is branch coverage (`--branch`), not line coverage.
- [ ] CLI/parsing modules have a dedicated bad-input test category.
