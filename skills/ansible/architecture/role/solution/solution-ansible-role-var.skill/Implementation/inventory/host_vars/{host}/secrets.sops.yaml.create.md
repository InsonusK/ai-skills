---
description: Consumer-side example of the SOPS-encrypted host-tier secret override, set in inventory (not inside the role)
role_name: "{{ role_name }}"
name: "inventory/host_vars/{{ host }}/secrets.sops.yaml"
element_kind: host_vars_secret
change_kind: create
---

# Goals
- Let a single host override a subset of a role's secret keys, encrypted at rest, without touching the role or the group's secret file.

# Core Principles
- The variable set here is the highest-precedence secret input tier: group-secret → host-secret. It is never defined inside the role — see [[../../../vars/main.yml.create.md|vars/main.yml]] for where it is consumed.
- The file must be encrypted with `sops` and use the `.sops.yaml` extension so the `community.sops.sops` vars plugin decrypts it automatically at inventory load time — see [[../../../../glossary/sops-secrets.md|SOPS-encrypted inventory secrets]].
- It lives alongside (not instead of) [[vars.yml.create.md|vars.yml]] in the same `host_vars/{{ host }}/` directory; Ansible merges both files for the host.

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| secret host override tier | `{{ role_name }}_secret` | `vpshost_vm_secret` | `inventory/host_vars/{{ host }}/secrets.sops.yaml` |

# Implementation changes

Logical content, before running `sops -e` (this is what the vars plugin resolves the file back to at runtime):

```yaml
# inventory/host_vars/web01/secrets.sops.yaml (decrypted view)
vpshost_vm_secret:
  api_key: "host-specific-override-key"
```

# Rule changes

## MUST
- This file must be encrypted with `sops` before it is committed; it must never be committed with plaintext secret values.
- The file name must end in `.sops.yaml` (or another extension recognized by the `community.sops.sops` vars plugin configuration) so it is auto-decrypted at inventory load.
- The secret host override variable must be named exactly `{{ role_name }}_secret` — no other suffix.

## MUST NOT
- Must not commit this file unencrypted.
- Must not define `{{ role_name }}` (the non-secret tier) in this file — that belongs in the sibling [[vars.yml.create.md|vars.yml]].

# Anti-patterns
- **Committing the file before running `sops -e`**
  - Consequence: the secret is now in plaintext in version control history, and rotating the leaked value does not undo the exposure — history must be scrubbed.
  - Instead: always encrypt with `sops` before `git add`.
- **Using host-tier secrets for values that are actually the same across the whole group**
  - Consequence: the same secret ends up duplicated (and re-encrypted separately) across every host file instead of set once at the group tier, multiplying rotation effort.
  - Instead: set group-wide secrets in [[../../group_vars/{group}/secrets.sops.yaml.create.md|group_vars/{{ group }}/secrets.sops.yaml]] and reserve host-tier secrets for genuinely host-specific values.

# Check list
- [ ] `inventory/host_vars/{{ host }}/secrets.sops.yaml` is encrypted with `sops` and defines `{{ role_name }}_secret`.
- [ ] The file is never committed unencrypted.
