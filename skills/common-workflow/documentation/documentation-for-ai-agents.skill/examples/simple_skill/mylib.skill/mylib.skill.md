---
name: mylib
description: How to install and call mylib — a small Python data-processing library
whenToUse: when an agent needs to install, import, or call mylib's process_data or fetch_records methods
tags:
  - skill/documentation/for-ai
  - skill/example
  - stack
  - concern/documentation

---

# Goal
Give an agent everything needed to install mylib and call its two entry points correctly.

# Core Principle
- mylib works with local files and in-memory record stores.
- All methods accept Python primitives; no authentication is required.

# Installation and access
See [installation.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/simple_skill/mylib.skill/installation.md) for the install command, import statement, and environment prerequisites.

# Methods
- [process_data](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/simple_skill/mylib.skill/method-process_data.md) — process a raw input source into a list of records.
- [fetch_records](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/simple_skill/mylib.skill/method-fetch_records.md) — retrieve stored records with optional filtering and pagination.

# Rule

## MUST
- Install mylib before calling any method.
- Handle `FileNotFoundError` when the input path does not exist.
- Validate that pagination parameters are non-negative.
- Never call `fetch_records` before the record store has been populated (for example, by `process_data`).

# Check list
- [ ] mylib is installed and imported.
- [ ] The right method is chosen for the task (`process_data` for new input, `fetch_records` for stored records).
- [ ] Parameters are validated before calling.
