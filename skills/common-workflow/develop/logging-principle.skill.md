---
name: logging-principle
description: Describes logging rules for development
whenToUse: when writing code that requires logging or choosing a log level
tags:
  - skill/develop
  - logging
  - stack
  - concern/documentation

---

# Goal
- Standardize logging rules during development.

# Core Principle
- We classify errors and logs.
- Log messages must provide enough information to understand exactly what happened.

# Rule
## MUST
- Always add logging to code.
- Always provide a parameter that enables `debug` level logs. It MUST be disabled by default.
- Classify logs by level:
  - **Info**: logging key workflow milestones (e.g., "scan completed, found the following results") or important integration events (e.g., "message received", "database record created", "response sent").
  - **Warning**: errors that do not stop execution but deserve attention.
  - **Error**: business errors that are expected to interrupt program execution (e.g., "could not find files in the provided directory", "system X did not respond to the request", "system X rejected our request", "system X responded with a status code != 2xx").
  - **Critical**: exceptions that were caught.
- Include specific, concrete information in log messages, not abstract text.
  - Violation: `print("scan completed")` | `Console.WriteLine("record created")` with abstract text like `task completed` or `record created`.
  - Risk: the log message does not clarify what actually happened.
  - Fix: include information that makes the context clear (e.g., `task 'clear db' completed` | `record 1234 created`).

## SHOULD
- Use the logging mechanisms provided by the language or framework (e.g., `logging`, `loguru`, `NLog`, `Serilog`, etc.) instead of `print`, `write`, or similar plain output mechanisms.
  - Risk: plain output mechanisms offer limited logging capabilities, and if a log aggregation system is added later, all logging code will have to be rewritten.

# Check list
- [ ] Logging is added for all meaningful operations.
- [ ] There is a parameter to enable/disable `debug` logs and it is disabled by default.
- [ ] Logs are classified into Info / Warning / Error / Critical.
- [ ] Log messages contain specific information about the event.
- [ ] `print`, `write`, and similar constructs are not used for logging.
