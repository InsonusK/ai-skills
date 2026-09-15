---
name: allow-extra-top-level-sections
description: Record why skill files may use optional Scope, Workflow, and Example top-level sections in addition to the mandatory four.
problem: The skill-design rule allowed only four top-level sections (# Goal, # Core Principle, # Rule, # Check list), but practical skills need a scope statement, a descriptive process overview, and a dedicated example pointer. Forcing all of that into the mandatory sections either buried rules under prose or removed useful structure.
decision: Allow at most one optional # Scope, # Workflow, and # Example top-level section in addition to the mandatory four, with strict constraints on what each may contain.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The original skill-design rule permitted only four top-level sections in a skill file: `# Goal`, `# Core Principle`, `# Rule`, and `# Check list`. In practice, that constraint was too tight:

- A skill like `skill-design` itself needed a `# Scope` section to state what it covers and what it does not.
- Skills that orchestrate a process (for example, CI workflow setup) benefit from a descriptive `# Workflow` section.
- Skills that reference large examples need a dedicated `# Example` section instead of scattering links inside `# Rule`.

Without a permitted place for this content, authors either pushed process descriptions and example pointers into `# Rule` (making normative rules harder to scan) or removed them entirely.

# Selected variant

**Selected variant:** [[#Allow optional Scope, Workflow, and Example sections]]

Keep the mandatory four sections exactly as before and add at most one `# Scope`, at most one `# Workflow`, and at most one `# Example` top-level section. Each optional section has a strict purpose:

- `# Scope` states coverage boundaries only.
- `# Workflow` describes the process; every normative requirement must also appear as a `## MUST`/`## SHOULD`/`## MAY` bullet under `# Rule`.
- `# Example` contains only one-line links to files inside the skill's own `examples/` or `templates/` folder.

# Searched variants

## Allow optional Scope, Workflow, and Example sections (selected)

### Description

Permit up to three optional top-level sections in addition to `# Goal`, `# Core Principle`, `# Rule`, and `# Check list`, each with a defined purpose and constraints.

### Benefits

- Skills can state scope, describe a process, and point to examples without polluting `# Rule`.
- The allowed set is closed, so structure stays predictable for agents scanning headings.
- Existing skills that already use `# Scope` or `# Example` become compliant without rewriting.

### Costs

- Agents must check two more places for content, although `# Rule` remains the only place for normative requirements.
- The rule needs extra prose to keep `# Workflow` and `# Example` from drifting into secondary rule sets.

## Keep only the four mandatory sections

### Description

Maintain the original rule: `# Goal`, `# Core Principle`, `# Rule`, `# Check list` are the only allowed top-level sections; everything else must be folded into them.

### Benefits

- Minimal structure; the skill file is easy to scan.
- No risk of optional sections becoming hidden rule sets.

### Costs

- Process descriptions and example links must be squeezed into `# Rule` or removed, hurting readability.
- Meta-skills like `skill-design` itself cannot use a `# Scope` section without violating their own rule.
- Large examples have no natural home outside `# Rule`, so they end up inline and overwhelm the rules.

## Allow arbitrary extra top-level sections

### Description

Let skill authors add any number of top-level sections with any name, as long as the mandatory four are present.

### Benefits

- Maximum flexibility for unusual skill layouts.
- No need to maintain a closed list of allowed section names.

### Costs

- The skill file can grow hidden rule sets, anti-pattern sections, or duplicated example blocks.
- Tooling and agents that scan for a predictable set of headings break or miss content.
- The repository ends up with inconsistent skill shapes that are harder to review.
