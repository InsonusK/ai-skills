---
description: Define the role's default variable tier — the full, safe key set every other tier overrides against
role_name: "{{ role_name }}"
name: defaults/main.yml
element_kind: defaults
change_kind: create
---

# Goals
- Give the role exactly one variable that carries every key it consumes, each with a safe default value.
- Make that variable the sole source of truth for "what keys does this role accept."

# Core Principles
- The default-tier variable is named `_{{ role_name }}_defaults` — prefixed with the role's own name and a leading underscore to mark it as role-internal (not meant to be set directly by a consumer).
- Every key the role will ever read through `_{{ role_name }}_config` (see [[../vars/main.yml.create.md|vars/main.yml]]) must appear here first, with a safe value.

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| role default tier | `_{{ role_name }}_defaults` | `_vpshost_vm_defaults` | `roles/{{ role_name }}/defaults/main.yml` |

# Implementation changes

```yaml
# roles/{{ role_name }}/defaults/main.yml
_{{ role_name }}_defaults:
  enabled: true
  port: 8080
  retention_days: 7
  paths:
    install: "/opt/{{ role_name }}"
    data: "/var/lib/{{ role_name }}"
```

# Rule changes

## MUST
- `_{{ role_name }}_defaults` must define every key the role reads via `_{{ role_name }}_config`; no key may be introduced only at the group or host tier.
- `_{{ role_name }}_defaults` must live in `defaults/main.yml` (Ansible's lowest-precedence variable file), never in `vars/main.yml`.

## SHOULD
- Group related keys under a nested mapping (e.g. `paths:`) when they are always set together, so a consumer can override the whole group at once — see [[../../glossary/ansible-combine-filter.md|combine filter]] recursive merge behavior.

## MUST NOT
- Must not name this variable without the `_{{ role_name }}_defaults` prefix — an unprefixed or generic name (`defaults`, `config_defaults`) collides with other roles' defaults in the same variable namespace.

# Anti-patterns
- **Adding a key only to `group_vars`/`host_vars`, skipping `_{{ role_name }}_defaults`**
  - Consequence: any host/group that doesn't set the key fails with an undefined-variable error the first time a task reads `_{{ role_name }}_config.<key>`, instead of falling back to a safe value.
  - Instead: add the key to `_{{ role_name }}_defaults` first, with a safe value, then override it selectively at the group or host tier.

# Check list
- [ ] `roles/{{ role_name }}/defaults/main.yml` defines `_{{ role_name }}_defaults` with every key the role consumes.
- [ ] Every key has a safe, non-placeholder default value.
