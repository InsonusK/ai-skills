---
description: Consumer-side example of the plaintext host-tier override, set in inventory (not inside the role)
role_name: "{{ role_name }}"
name: "inventory/host_vars/{{ host }}/vars.yml"
element_kind: host_vars
change_kind: create
---

# Goals
- Let a single host override a subset of a role's non-secret defaults (and any group override), without touching the role or the group's inventory file.

# Core Principles
- The variable set here is the highest-precedence non-secret input tier: defaults → group → host. It is never defined inside the role — see [[../../../vars/main.yml.create.md|vars/main.yml]] for where it is consumed.
- Only the keys that need to differ for this specific host need to be listed.
- This file lives in `host_vars/{{ host }}/` (a directory, not a single `host_vars/{{ host }}.yml` file) specifically so a sibling `secrets.sops.yaml` file can hold the host's secret tier — see [[secrets.sops.yaml.create.md|secrets.sops.yaml]]. Ansible merges every file inside `host_vars/{{ host }}/` for that host automatically.

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| host override tier | `{{ role_name }}` (bare, no suffix) | `vpshost_vm` | `inventory/host_vars/{{ host }}/vars.yml` |

# Implementation changes

```yaml
# inventory/host_vars/web01/vars.yml
vpshost_vm:
  port: 9090
```

# Rule changes

## MUST
- The host override variable must be named exactly `{{ role_name }}` — no `_defaults`, `_group`, `_secret`, or `_config` suffix.
- Only keys that differ from the merged defaults+group result need to be present.
- This file must contain only plaintext, non-secret values.

## MUST NOT
- Must not define `_{{ role_name }}_defaults`, `{{ role_name }}_group`, `_{{ role_name }}_config`, or `{{ role_name }}_secret` in `host_vars` — each belongs to a different tier/location.
- Must not contain any secret value — secrets belong in the sibling [[secrets.sops.yaml.create.md|secrets.sops.yaml]] file instead.

# Anti-patterns
- **Using the bare `{{ role_name }}` variable for something other than the host-override tier (e.g. as a flag or a string)**
  - Consequence: it collides with the reserved host-tier input that [[../../../vars/main.yml.create.md|vars/main.yml]] expects to be a mapping, breaking the `combine()` call for every host that sets it.
  - Instead: reserve the bare `{{ role_name }}` name exclusively for the host-tier override mapping.
- **Putting a secret value in this file "just this once"**
  - Consequence: the secret is now committed in plaintext in a file that is not SOPS-encrypted, defeating the entire purpose of the separate secret tier.
  - Instead: put every secret value in the sibling `secrets.sops.yaml` file under `{{ role_name }}_secret`.

# Check list
- [ ] `inventory/host_vars/{{ host }}/vars.yml` defines `{{ role_name }}` with only the overridden, non-secret keys.
