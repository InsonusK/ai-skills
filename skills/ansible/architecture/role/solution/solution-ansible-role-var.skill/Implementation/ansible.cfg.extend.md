---
description: Enable the community.sops.sops vars plugin project-wide, so *.sops.yaml files in group_vars/host_vars auto-decrypt
role_name: # project-wide file, not tied to a single role
name: ansible.cfg
element_kind: config
change_kind: extend
---

# Goals
- Make every SOPS-encrypted `*.sops.yaml` file inside any `group_vars/`/`host_vars/` directory decrypt automatically when Ansible loads inventory variables, project-wide, once.

# Core Principles
- This is a one-time, project-wide change — it is not repeated per role. Every role that uses [[inventory/group_vars/{group}/secrets.sops.yaml.create.md|secrets.sops.yaml]] / [[inventory/host_vars/{host}/secrets.sops.yaml.create.md|secrets.sops.yaml]] depends on this being enabled once.
- See [[../glossary/sops-secrets.md|SOPS-encrypted inventory secrets]] for what the `community.sops.sops` vars plugin does and why the file extension matters.

# Implementation changes

```ini
# ansible.cfg
[defaults]
vars_plugins_enabled = host_group_vars,community.sops.sops

[community.sops]
vars_stage = inventory
```

- `host_group_vars` must remain in the list — it is the plugin that loads ordinary (non-SOPS) `group_vars`/`host_vars` files; removing it breaks every role's non-secret tiers.
- `community.sops.sops` is appended, not substituted, so both plugins run.

# Requirements
- The `community.sops` collection must be installed (e.g. via `requirements.yml` / `ansible-galaxy collection install community.sops`).
- The `sops` binary and the key backend it is configured for (age, PGP, or a cloud KMS) must be available wherever `ansible-playbook`/`ansible-inventory` actually runs — including CI.

# Rule changes

## MUST
- `vars_plugins_enabled` must include both `host_group_vars` and `community.sops.sops`.
- The `community.sops` collection must be declared as a project dependency (`requirements.yml`), not assumed to be pre-installed.

## MUST NOT
- Must not enable `community.sops.sops` without keeping `host_group_vars` enabled — doing so silently stops loading every plaintext `group_vars`/`host_vars` file in the project.

# Anti-patterns
- **Adding `*.sops.yaml` files to inventory before enabling the vars plugin**
  - Consequence: `ansible-inventory`/`ansible-playbook` either fails to parse the ciphertext as YAML, or (depending on Ansible version/config) silently skips the file, so the role's secret tier resolves empty and validation fails deep into a run instead of at setup time.
  - Instead: enable `community.sops.sops` in `ansible.cfg` (and install the collection) before any role relies on a `secrets.sops.yaml` file.

# Check list
- [ ] `ansible.cfg` enables both `host_group_vars` and `community.sops.sops` in `vars_plugins_enabled`.
- [ ] `community.sops` is listed in the project's collection requirements.
- [ ] CI/runner environments have the `sops` binary and decryption keys available.
