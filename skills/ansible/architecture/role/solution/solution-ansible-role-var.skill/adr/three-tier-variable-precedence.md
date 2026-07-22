---
name: architecture decision record
description: How to structure a role's variables so each role owns an isolated namespace and still supports layered overrides
problem: How should an Ansible role structure its variables so that (a) different roles never collide on the same variable name in inventory, and (b) a value can still be overridden per-group and per-host without editing the role itself?
decision: Give every role a single {{ role_name }} prefix and three input tiers (`_{{ role_name }}_defaults`, `{{ role_name }}_group`, `{{ role_name }}`) merged with `combine(recursive=True)` into one effective variable, `_{{ role_name }}_config`
---

# Problem

Roles that read variables directly by short, generic names (`port`, `enabled`, `config`) collide with other roles in the same inventory, and changing one role's variable structure forces edits in multiple unrelated `group_vars`/`host_vars` files. A structure was needed that:

- gives each role its own variable namespace,
- lets defaults live in the role, group-wide values live in `group_vars`, and per-host values live in `host_vars`,
- resolves all three into one value the role's tasks/templates read from.

# Selected variant

**Selected variant:** [[#Three-tier prefixed combine (selected)]]
- Every tier shares the exact same `{{ role_name }}` prefix, so the anti-collision guarantee holds even as roles multiply.
- The merge is computed once, in `vars/main.yml`, instead of being repeated ad-hoc across tasks.

# Searched variants

## Two-tier defaults + override (existing pattern)

### Description
The pattern already documented in [ansible-role-requirements.skill.md §6](../../../../../ansible-role-requirements/SKILL.md): `{{ structure }}_defaults` combined with a single `{{ structure }}_override` variable.

### Benefits
- Simple: one override variable, one combine call.
- Already documented and used elsewhere in the repo.

### Costs
- Does not distinguish "set for the whole group" from "set for this one host" — both land in the same `_override` variable, so a host-level `vars:` entry and a `group_vars` entry compete for the same name with no dedicated per-tier precedence.
- Does not solve the specific requirement here: a group tier and a host tier that must both be able to override the defaults independently, with a fixed precedence between them.

## Flat variables shared across roles (no per-role namespace)

### Description
Roles read directly from generic variable names (e.g. `port`, `retention_days`) with no role-specific prefix.

### Benefits
- Fewer characters to type; no prefix to remember.

### Costs
- Two roles that both need a `port` or `enabled` variable collide the moment they are used in the same play or inventory group.
- Renaming or restructuring one role's variables risks silently breaking another role that happens to read the same name.
- This is the exact problem the solution exists to prevent — kept here only as the rejected baseline.

## Runtime `set_fact` merge inside tasks

### Description
Instead of a templated variable in `vars/main.yml`, a task at the top of `tasks/main.yml` computes the merged value with `set_fact`, e.g. `set_fact: "{{ role_name }}_config={{ ... | combine(...) }}"`.

### Benefits
- Merge logic is visible inline where the role starts executing.

### Costs
- The merged value does not exist until that task has run, so it cannot be used in `when:` conditions or `vars:` on earlier tasks, in `meta: end_play`/`import_role` boundaries evaluated before task execution, or by other roles that reference this role's variables before it runs.
- Duplicates a computation that `vars/main.yml` already performs automatically and lazily for every task in the role.

## Three-tier prefixed combine (selected)

### Description
Every tier — role defaults, group override, host override — shares one `{{ role_name }}` prefix:
- `_{{ role_name }}_defaults` (defined in `defaults/main.yml`, full safe key set)
- `{{ role_name }}_group` (reserved for `group_vars/*`, never defined by the role)
- `{{ role_name }}` (reserved for `host_vars/*` or host-level `vars:`, never defined by the role)

`vars/main.yml` combines them once, in fixed order, into `_{{ role_name }}_config`, which is the only variable tasks/templates read from.

### Benefits
- One unambiguous namespace per role prevents cross-role collisions in inventory.
- Group and host overrides are independent inputs with a fixed, documented precedence (defaults → group → host), instead of competing for one `_override` slot.
- `vars/main.yml` templates are evaluated lazily per use, so the merged value is available to every task/template in the role without an explicit "run this task first" step.
- `recursive=True` lets a consumer override a single nested key without having to restate every sibling key.

### Costs
- Four related variable names per role to keep straight (`_defaults`, `_group`, bare, `_config`) instead of two.
- Requires discipline: nothing in the role may read `{{ role_name }}_group` or `{{ role_name }}` directly, only `_{{ role_name }}_config`.
