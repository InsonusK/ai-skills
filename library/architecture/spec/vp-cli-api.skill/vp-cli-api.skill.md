---
name: vp-cli-api
description: The CLI API variation point — a command-line entry point attached to the base plateau that runs one command per call as `{app} {resource} {action} [flags]`, prints the result on standard output as one JSON line or as text, writes errors on standard error, exits by the error kind with usage errors as rejected, offers help at every level, and replaces the startup entry without editing any other base file
whenToUse: when an application built on the base plateau must be run from a command line by a person or a script — adding the CLI entry point, offering a new command as an action, or reviewing the CLI entry point of such an application
updated: 20261102
tags:
  - stack
  - concern/architecture
  - app-type/cli
---

# Goal
- **Commands reachable from a command line** - Every command the application offers is one action `{app} {resource} {action}`; its flags become the command's plain parameters.
- **Readable by a script and by a person** - The result is one JSON line on standard output by default and text with `--output text`; nothing but the result is written there.
- **Outcome in the exit code** - A script tells success (`0`), a rejected request or a usage error (`2`), and a failure a retry may fix (`1`) apart without reading the output.
- **Attached, not edited** - The entry point joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) by replacing the startup entry in the `entries` list and changes no other base file but the composition root and the module's dependency, build, and readme files.

# Core Principle
- **A thin translation** - The CLI entry point parses, calls, and prints; it makes no business decision, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) requires of every api layer.
- **The same resources as the API** - Actions are grouped by the resources an HTTP API of the same application would have, so `todo items plan` and `POST /v1/items/plan` name one operation.
- **One call, one command** - The process runs the action its arguments name and ends; it holds no state between calls.

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers a command-line application whose every call runs one command and ends. An interactive shell, a long-running daemon started from the command line, and reading a command's input from standard input are not covered.
- The implementation for each stack is described in its stack-specialized extension: `vp-cli-api-in-go`, `vp-cli-api-in-python`.

# Rule

## MUST

### One action per command, grouped by resource
Offer each command as exactly one action `{app} {resource} {action}`, with the actions of one resource together under it.
- Violation: an action that calls two commands or reads state to choose one; a flat list of every action under the application.
- Risk: business logic moves into the command line, and a help page listing dozens of actions cannot be read.
- Fix: one command per action; a choice that depends on state goes to a command factory per app-layers; one group per resource, named as the HTTP resource.

### Flags and output belong to the entry point
Declare each action's flags and the shape of its JSON output in the CLI entry point and map them to and from the command and its result.
- Violation: the command's result type printed directly as the output.
- Risk: renaming a field of the command's result silently breaks every script that reads the output.
- Fix: an output type per action, with the mapping beside the action.

### Write only the result on standard output
Print the result on standard output — one JSON line, or the action's text form with `--output text` — and help there too; print errors, prefixed `Error:`, and logs on standard error.
- Violation: a log line or a progress message on standard output; an error printed only to the log.
- Risk: a script that parses standard output breaks on the first log line; a person sees a JSON log entry instead of the reason.
- Fix: standard output for the result and the help, standard error for everything else.

### Exit by the error kind
Exit `0` on success, `2` for an error of a kind other than unavailable and for every usage error — an unknown action, an unknown or malformed flag, a flag the action cannot turn into its command — and `1` for unavailable and for an error with no kind, through the base plateau's exit code.
- Risk: a script retries a request that can never succeed, or gives up on one a retry would fix.
- Fix: a usage error is the invalid kind of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md); an error of the command keeps its kind.

### Keep the run log and the answer of every failure inside the entry
Build the run log and the answer of every failure inside the CLI entry point, from the logger the composition root gives it: every failure of a run — a usage error, an error of the command, an action that crashed — ends as one `Error: ...` line on standard error and the exit code of its kind, a crash as a failure with no kind (exit `1`), and is logged as a failed run. The command line takes no list of middleware: a step on every action is a command middleware. Decision recorded in [fixed-middleware-inside-the-entry](library/architecture/spec/base-plateau-extension.skill/adr/fixed-middleware-inside-the-entry.md).
- Violation: a crashed action left to the runtime, which ends the process with its own exit code and trace.
- Risk: Go ends a process on an unrecovered panic with exit code `2`, which a script reads as a rejected command and does not retry; the run leaves no record.
- Fix: the CLI's fixed code recovers a crash of an action and answers it as a failure; a scenario of a crashing action.

### Offer help at every level
Answer `--help` on the application, on a resource, and on an action — and the application or a resource called with no action — with what can be run and its flags, running no command; answer `--version` with the version of the binary.
- Risk: a person has to read the code to learn the actions and their flags.
- Fix: each action and resource has a one-line description; each flag a description.

### Log every run with what arrived
Write two records per run through the composition root's logger — one when it arrives, with what arrived as the payload, one when it is answered — the second with the action and the duration, whether it failed, and as the payload the flags given and the output as printed and a new request id in the context for the records of the run.
- Risk: a run that failed cannot be found or replayed from the log.
- Fix: an in record and an out record per run, so the log reads top-down with the command's records between them; the out record of a failure repeats the input; the data only as the payload attribute, so the base handler drops it and the logging variation point masks it.

### Attach by replacing the startup entry
Attach the entry point by adding its packages and replacing the startup entry in the composition root's `entries` list with one entry that reads the arguments, runs the action, and ends, deleting the startup entry, and changing no other base file than the composition root and the module's dependency, build, and readme files.
- Risk: any other base file changed by the CLI variation point must be described together with every variation point that touches it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Test with the commands faked
Test the fixed code once through a test resource of its own — a success, the text output and its fallback, a rejected command keeping its kind, a failure with no kind, each usage error, help at every level, `--version` — and each resource with its command handlers replaced by fakes.
- Risk: a test through the real command tests the domain again; an untested usage error exits `1`, and a script retries it forever.
- Fix: one fake handler per command; a resource's scenarios check only its own flags and output.

# Check list
- [ ] Every offered command is one action `{app} {resource} {action}`, grouped by resource.
- [ ] Flags and the JSON output of each action belong to the CLI entry point; `--output text` prints the action's text form.
- [ ] Standard output holds only the result and the help; errors and logs go to standard error.
- [ ] The exit code is `0`, `2` (a kind other than unavailable, every usage error), or `1` (unavailable, no kind).
- [ ] `--help` works on every level and runs no command; `--version` prints the version.
- [ ] The entry point is one entry of the `entries` list; the startup entry is gone; no base file changed but the composition root and the module's dependency, build, and readme files.
