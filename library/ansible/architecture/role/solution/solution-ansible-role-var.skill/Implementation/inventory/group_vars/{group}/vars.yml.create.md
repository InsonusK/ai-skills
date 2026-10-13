---
description: Consumer-side example of the plaintext group-tier override, set in inventory (not inside the role)
role_name: "{{ role_name }}"
name: "inventory/group_vars/{{ group }}/vars.yml"
element_kind: group_vars
change_kind: create
---

# Goals
- Let an inventory group override a subset of a role's non-secret defaults for every host in that group, without touching the role itself.

# Core Principles
- The variable set here has exactly one job: override non-secret keys for a whole group. It is never defined inside the role — see [[../../../vars/main.yml.create.md|vars/main.yml]] for where it is consumed.
- Only the keys that need to differ from `_{{ role_name }}_defaults` need to be listed; every other key falls through from the defaults tier.
- This file lives in `group_vars/{{ group }}/` (a directory, not a single `group_vars/{{ group }}.yml` file) specifically so a sibling `secrets.sops.yaml` file can hold the group's secret tier — see [[secrets.sops.yaml.create.md|secrets.sops.yaml]]. Ansible merges every file inside `group_vars/{{ group }}/` for that group automatically.

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| group override tier | `{{ role_name }}_group` | `vpshost_vm_group` | `inventory/group_vars/{{ group }}/vars.yml` |

# Implementation changes

```yaml
# inventory/group_vars/webservers/vars.yml
vpshost_vm_group:
  retention_days: 14
  paths:
    data: "/mnt/data/vpshost_vm"
```

# Rule changes

## MUST
- The group override variable must be named `{{ role_name }}_group`, matching the role it targets.
- Only keys that differ from `_{{ role_name }}_defaults` need to be present; omitted keys keep their default value.
- This file must contain only plaintext, non-secret values.

## MUST NOT
- Must not define `_{{ role_name }}_defaults` or `_{{ role_name }}_config` in `group_vars` — those belong to the role's own `defaults/main.yml` and `vars/main.yml` respectively.
- Must not contain any secret value — secrets belong in the sibling [[secrets.sops.yaml.create.md|secrets.sops.yaml]] file instead.

# Anti-patterns
- **Restating every default key in the group override "to be explicit"**
  - Consequence: the override file drifts out of sync whenever a new key is added to `_{{ role_name }}_defaults`, and it becomes unclear which keys were actually meant to differ per group.
  - Instead: list only the keys this group genuinely overrides.
- **Putting a secret value in this file "just this once"**
  - Consequence: the secret is now committed in plaintext in a file that is not SOPS-encrypted, defeating the entire purpose of the separate secret tier.
  - Instead: put every secret value in the sibling `secrets.sops.yaml` file under `{{ role_name }}_group_secret`.

# Check list
- [ ] `inventory/group_vars/{{ group }}/vars.yml` defines `{{ role_name }}_group` with only the overridden, non-secret keys.
