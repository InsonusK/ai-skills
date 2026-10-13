---
name: log-per-standard
description: Validation history lives in one flat log file per reference standard (./.validation/{validation-skill}-log.yaml), not in a shared rule-keyed log
problem: With several validation rules covering overlapping file sets, where does validation history live, and what identifies a log entry
decision: One flat log file per reference standard; entries keyed by file path alone; rules reference standards, so rule renames and rule filters never touch validation history
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The validation config supports several rules, each selecting files (possibly overlapping sets) and naming its own reference standard. The validation log must record when each file was last validated against each standard. Two questions: how are log files organized, and what key identifies an entry.

# Selected variant
**Selected variant:** [[#Per-standard log files]]
- Revised 20260906: the initial decision was [[#Shared log keyed by rule and path]]; it was replaced once the per-standard naming was proposed, because keying by rule conflated the rule's identity with the standard's identity.

# Searched variants

## Per-standard log files

### Description
One file per reference standard: `./.validation/{validation-skill}-log.yaml`, each a flat map of repository-root-relative file path → last validation date.

### Benefits
- The entry semantics match reality: "file F was validated against standard S on date D" — validation is against the standard, not against the rule that selected the file.
- Flat maps only — no nested YAML, the simplest possible parser.
- Rules can be renamed, refiltered, or split freely; history is untouched because logs never reference rules.
- Several rules sharing one standard share one log — a file validated against that standard is stamped once, honestly.
- Removing a standard removes one visible file; an orphaned log file (standard no longer referenced by any rule) is pruned by `queue --dry-run`.

### Costs
- No single aggregate file — the overview is a directory listing plus small files (accepted: the directory is technical, read mostly by the script).
- A rule switching its `validation-skill` leaves the old standard's file orphaned (mitigated by dry-run pruning).

## Shared log keyed by rule and path

### Description
A single `validation-log.yaml` holding a nested map: rule name → file path → date.

### Benefits
- One file to inspect and diff; centralized pruning.

### Costs
- Conflates rule identity with standard identity: two rules validated against the same standard would stamp the same file twice, once per rule — duplicate records of one fact.
- Renaming a rule orphans its history (the log keys entries by rule name).
- Nested format needs a structured parser.

## Per-rule log files

### Description
One file per rule, e.g. `.validation/skill-design.yaml`, keyed by rule name.

### Benefits
- Rules fully independent; flat maps.

### Costs
- Same rule-vs-standard conflation as the shared rule-keyed log.
- File ↔ rule mapping bookkeeping; orphaned files on rule renames are harder to spot.

## Shared log keyed by path only

### Description
One log file, entries keyed by file path alone.

### Benefits
- Simplest format.

### Costs
- Wrong under overlapping rules with different standards: a file validated against standard A appears validated for standard B without ever being checked against B — a silent false negative in the queue.
