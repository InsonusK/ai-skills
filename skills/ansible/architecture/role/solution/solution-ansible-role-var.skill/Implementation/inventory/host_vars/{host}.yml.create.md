---
description: Consumer-side example of the host-tier override, set in inventory (not inside the role)
role_name: "{{ role_name }}"
name: "inventory/host_vars/{{ host }}.yml"
element_kind: host_vars
change_kind: create
---

# Goals
- Let a single host override a subset of a role's defaults (and any group override), without touching the role or the group's inventory file.

# Core Principles
- The variable set here is the highest-precedence input tier: defaults → group → host. It is never defined inside the role — see [[../../vars/main.yml.create.md|vars/main.yml]] for where it is consumed.
- Only the keys that need to differ for this specific host need to be listed.

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| host override tier | `{{ role_name }}` (bare, no suffix) | `vpshost_vm` | `inventory/host_vars/{{ host }}.yml` |

# Implementation changes

```yaml
# inventory/host_vars/web01.yml
vpshost_vm:
  port: 9090
```

# Rule changes

## MUST
- The host override variable must be named exactly `{{ role_name }}` — no `_defaults`, `_group`, or `_config` suffix.
- Only keys that differ from the merged defaults+group result need to be present.

## MUST NOT
- Must not define `_{{ role_name }}_defaults`, `{{ role_name }}_group`, or `_{{ role_name }}_config` in `host_vars` — each of those belongs to a different tier/location.

# Anti-patterns
- **Using the bare `{{ role_name }}` variable for something other than the host-override tier (e.g. as a flag or a string)**
  - Consequence: it collides with the reserved host-tier input that [[../../vars/main.yml.create.md|vars/main.yml]] expects to be a mapping, breaking the `combine()` call for every host that sets it.
  - Instead: reserve the bare `{{ role_name }}` name exclusively for the host-tier override mapping.

# Check list
- [ ] `inventory/host_vars/{{ host }}.yml` defines `{{ role_name }}` with only the keys this host overrides.
