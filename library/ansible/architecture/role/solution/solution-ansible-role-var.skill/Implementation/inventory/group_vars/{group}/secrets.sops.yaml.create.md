---
description: Consumer-side example of the SOPS-encrypted group-tier secret override, set in inventory (not inside the role)
role_name: "{{ role_name }}"
name: "inventory/group_vars/{{ group }}/secrets.sops.yaml"
element_kind: group_vars_secret
change_kind: create
---

# Goals
- Let an inventory group override a subset of a role's secret keys for every host in that group, encrypted at rest, without touching the role itself.

# Core Principles
- The variable set here has exactly one job: override secret keys for a whole group. It is never defined inside the role — see [[../../../vars/main.yml.create.md|vars/main.yml]] for where it is consumed.
- The file must be encrypted with `sops` and use the `.sops.yaml` extension so the `community.sops.sops` vars plugin decrypts it automatically at inventory load time — see [[../../../../glossary/sops-secrets.md|SOPS-encrypted inventory secrets]].
- It lives alongside (not instead of) [[vars.yml.create.md|vars.yml]] in the same `group_vars/{{ group }}/` directory; Ansible merges both files for the group.

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| secret group override tier | `{{ role_name }}_group_secret` | `vpshost_vm_group_secret` | `inventory/group_vars/{{ group }}/secrets.sops.yaml` |

# Implementation changes

Logical content, before running `sops -e` (this is what the vars plugin resolves the file back to at runtime):

```yaml
# inventory/group_vars/webservers/secrets.sops.yaml (decrypted view)
vpshost_vm_group_secret:
  license_key: "S-1234-ABCD"
```

What is actually committed to the repository (abbreviated — exact ciphertext/metadata will differ per file):

```yaml
# inventory/group_vars/webservers/secrets.sops.yaml (as stored on disk)
vpshost_vm_group_secret:
    license_key: ENC[AES256_GCM,data:Vv1x...,iv:...,tag:...,type:str]
sops:
    age:
        - recipient: age1exampleu9z...
          enc: |
            -----BEGIN AGE ENCRYPTED FILE-----
            ...
            -----END AGE ENCRYPTED FILE-----
    lastmodified: "2026-07-23T00:00:00Z"
    mac: ENC[AES256_GCM,data:...,iv:...,tag:...,type:str]
    version: 3.9.0
```

# Rule changes

## MUST
- This file must be encrypted with `sops` before it is committed; it must never be committed with plaintext secret values.
- The file name must end in `.sops.yaml` (or another extension recognized by the `community.sops.sops` vars plugin configuration) so it is auto-decrypted at inventory load.
- The secret group override variable must be named `{{ role_name }}_group_secret`, matching the role it targets.
- Only keys that differ from the role's required secret keys for this group need to be present.

## MUST NOT
- Must not commit this file unencrypted.
- Must not define `{{ role_name }}_group` (the non-secret tier) in this file — that belongs in the sibling [[vars.yml.create.md|vars.yml]].

# Anti-patterns
- **Committing the file before running `sops -e`**
  - Consequence: the secret is now in plaintext in version control history, and rotating the leaked value does not undo the exposure — history must be scrubbed.
  - Instead: always encrypt with `sops` before `git add`; consider a pre-commit hook or `.sops.yaml` creation-rule that refuses unencrypted `*.sops.yaml` files.
- **Forgetting to enable `community.sops.sops` in `ansible.cfg`**
  - Consequence: the file is either read as raw ciphertext (breaking the role with garbled values) or silently ignored, and the role falls back to whatever `_{{ role_name }}_secret_config` resolves to without this group's secrets — which, since there is no secret defaults tier, likely fails validation instead of silently using a wrong value.
  - Instead: enable the vars plugin project-wide once — see [[../../../ansible.cfg.extend.md|ansible.cfg]].

# Check list
- [ ] `inventory/group_vars/{{ group }}/secrets.sops.yaml` is encrypted with `sops` and defines `{{ role_name }}_group_secret`.
- [ ] The file is never committed unencrypted.
