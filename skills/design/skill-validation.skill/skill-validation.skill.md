---
name: skill-validation
description: Methodical oldest-first validation pass over the repository's skill files against reference standard skills, driven by a config file and per-standard validation logs
whenToUse: when a conformance pass over the repository's skills is due — after a reference standard skill changed, or periodically to catch drift — and you need the queue of skills due for validation, oldest-validated first
updated: 20260906
tags:
  - stack
  - concern/testing
  - workflow
adr:
  - adr/log-per-standard.md
---

# Goal
Run a methodical validation pass over the repository's skills. Concretely:
- **Due-skills queue** - For each validation rule, compute which matching files are due for re-validation: last validation (per the standard's log) older than the rule's reference standard last change or older than the file's own last change, sorted oldest-validated first.
- **Conforming skills** - Bring each taken skill into conformance with its rule's reference standard.
- **Up-to-date validation logs** - Stamp every validated skill in its standard's log, so the next pass starts from what is actually stale.

# Core Principle
- **The script owns the queue and the logs** - The agent never computes staleness by hand and never edits log files directly; the script reads and writes them.
- **Judge against the fresh standard** - Validate against the rule's reference skill read from disk in this pass, never against its rules as remembered.
- **One skill at a time** - Take the queue's top entry, validate it fully, stamp it, then take the next.

# Layout
- `validation-config.yaml` lives at the repository root (user-facing settings). Validation logs live in `.validation/` (technical files, kept out of user-facing ones) — one flat file per reference standard, `{validation-skill}-log.yaml`, mapping repository-root-relative skill path → last validation date (`YYYYMMDD`). See [adr/log-per-standard.md](./adr/log-per-standard.md) for why logs key on the standard, not the rule.
- The script lives at `scripts/validation_queue.py` inside this skill's folder; the repository root is always four directory levels up from the script, whether the skill runs deployed (`.agents/skills/skill-validation/`, `.claude/skills/skill-validation/`) or in-repo (`skills/design/skill-validation.skill/`).
- The config carries an optional `log-dir` key (default `./.validation`) and a `rules` list. Each rule has: `name` (unique — the CLI addresses rules by it), `filter` (glob from the repository root), `exclude` (globs subtracted from the filter's matches, e.g. generated plateau structure files), `validation-skill` (the reference standard's name).
- A rule's reference skill is found as `../{validation-skill}/` next to this skill's folder when deployed, or by repository glob in-repo. It must carry `updated: YYYYMMDD` in its frontmatter.
- Several rules sharing one `validation-skill` share one log file — a stamp means "validated against this standard", independent of which rule selected the file. Rule renames and refiltering never touch history. Entries whose files were deleted or fell out of the filter, and log files of standards no longer referenced by any rule, are pruned by the script, never by hand.

# How to validate
1. Bootstrap or refresh the logs: `python3 scripts/validation_queue.py queue --dry-run`. This prints every rule's queue, registers each rule's matching files in its standard's log as never-validated (`19700101`, existing dates are kept), prunes entries whose files were deleted or no longer match, and deletes orphan log files of standards no longer referenced by any rule.
2. Run `python3 scripts/validation_queue.py queue --top {N}` (optionally `--rule {name}` for one rule only). The script prints each rule's reference skill and its due queue, oldest-validated first.
3. Read the reference skill of the rule you are working fresh, including its `# Check list`.
4. Take that rule queue's top skill: read it, check it against the reference skill's rules, and fix every violation found — a skill touched by validation migrates fully to the current format, never partially. Ask the user when a fix is ambiguous or changes the skill's meaning.
5. Stamp the validated skill: `python3 scripts/validation_queue.py mark {rule} {path}` — the script resolves the rule to its standard's log.
6. Repeat from step 2 until the queue is empty or the user stops the pass.

# Rule

## MUST

### Script owns queue and logs
Compute the due-queue and update the validation logs only through `scripts/validation_queue.py` — never hand-pick files to validate from memory and never edit log files directly, including removing entries for deleted skills.
- Risk: a hand-picked queue skips files that silently went stale; a hand-edited log drifts from the real validation history, and later passes trust dates that were never earned.
- Fix: `queue` to learn what is due, `queue --dry-run` to register new files and prune deleted ones, `mark {rule} {path}` to record what was validated — no other writes to the logs.

### Judge against the fresh standard
Read the reference skill of the rule you are working from disk at the start of every pass and validate against that text, including its `# Check list`.
- Risk: validating from memory applies a stale standard — the reference skill may have changed since the agent last read it, which is the usual reason a pass is due at all.
- Fix: read the reference file the script prints for that rule before touching the first queued skill.

### Fix violations, escalate ambiguity
Fix every conformance violation found in a taken skill within the same pass; stop and ask the user when a fix is ambiguous or changes the skill's meaning.
- Risk: a validation that only reports violations leaves the queue unchanged — the same skill comes up stale again next pass, and the log cannot be honestly stamped.
- Fix: apply the fix directly when the standard prescribes it; ask when judgment is required.

### Stamp only what was validated
Mark a skill in the log only after it was actually checked against the rule's reference standard in this pass.
- Risk: stamping an unchecked skill removes it from the queue without validation — the exact failure the log exists to prevent.
- Fix: `mark` only paths that went through step 4 of the workflow, under the rule they were checked against.

### Rule names stay unique
Give every config rule a unique `name` — the CLI addresses rules by it. Renaming or refiltering a rule is safe: logs key on the standard, not the rule.
- Risk: duplicate names make `mark {rule}` ambiguous and queue output unreadable.
- Fix: pick distinct names; rename freely when meaning changes — history survives.

### Reference skill carries its change date
Keep `updated: YYYYMMDD` in every reference skill's frontmatter and bump it on every change to that skill — the queue's staleness rule compares against it.
- Risk: without a current `updated` date, a changed standard triggers no re-validation and the whole pass silently validates against nothing new.
- Fix: bump `updated` in the same commit that changes the reference skill's rules.

### Fixed paths for config, logs, script
Keep `validation-config.yaml` at the repository root, validation logs under `.validation/`, and the queue script at `scripts/` inside this skill's folder.
- Risk: moving any of the three breaks the four-levels-up path invariant that lets the same script run deployed and in-repo, or exposes technical files among user-facing ones.
- Fix: leave the files where they are; change the logs' location only through the config's `log-dir` key.

## SHOULD

### Oldest first
Validate strictly in queue order within a rule — the file untouched longest is validated next, not the most convenient one.
- Risk: cherry-picking easy skills lets the oldest, drift-prone ones age forever at the bottom of the queue.
- Fix: take the queue's top entry each time; skip only with the user's explicit call.

### Small batches
Take a small `--top {N}` (default 5) per pass rather than the whole queue.
- Risk: a huge batch dilutes review quality — late skills in the batch get a shallower check than the first.
- Fix: run short passes repeatedly; the logs make resuming free.

## MAY

### Narrow a rule's filter for a focused pass
Point a rule's `filter` at a subtree (e.g. `./skills/common-workflow/**/*.skill.md`) or adjust its `exclude` globs when only one area should be validated.

# Check list
- [ ] The queue came from `validation_queue.py queue`, not from hand-picking.
- [ ] The logs were bootstrapped/refreshed with `queue --dry-run` (new files registered, deleted files and orphan standard logs pruned).
- [ ] The reference skill of the rule being worked was read fresh in this pass, including its `# Check list`.
- [ ] Every taken skill was checked against that rule's reference standard.
- [ ] Every violation found was fixed, or escalated to the user when ambiguous.
- [ ] Logs were updated only through `validation_queue.py mark {rule} {path}`, and only for skills actually validated.
- [ ] Queue order (oldest-validated first) was kept within the rule.
