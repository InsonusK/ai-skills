---
name: architecture decision record
description: Whether secret variable tiers should merge into the same effective config as non-secret variables, or into their own separate variable
problem: How should the group-secret and host-secret variable tiers (`{{ role_name }}_group_secret`, `{{ role_name }}_secret`) be merged, so that a secret can never leak through an ordinary debug/verbose dump of a role's config?
decision: Merge secret tiers into a separate `_{{ role_name }}_secret_config`, never into `_{{ role_name }}_config`; source both tiers from SOPS-encrypted inventory files decrypted by the `community.sops.sops` vars plugin
---

# Problem

The three-tier config pattern (see [[three-tier-variable-precedence.md|Three-tier variable precedence]]) merges `_{{ role_name }}_defaults`, `{{ role_name }}_group`, and `{{ role_name }}` into one `_{{ role_name }}_config`. Some roles also need group-level and host-level secret material (API keys, passwords, license keys). Two questions had to be settled: where do encrypted secret values live at rest in inventory, and do they merge into the same effective config variable as everything else, or a separate one?

# Selected variant

**Selected variant:** [[#Separate _secret_config, sourced from SOPS-encrypted files (selected)]]
- Keeps every accidental `debug: var=_{{ role_name }}_config` or verbose task-argument dump free of secret material by construction.
- Reuses the same `community.sops.sops` vars plugin mechanism the project already needs for any SOPS-based secret, rather than inventing a bespoke encryption scheme per role.

# Searched variants

## Merge secrets into the single `_{{ role_name }}_config`

### Description
Add `{{ role_name }}_group_secret` and `{{ role_name }}_secret` as two more inputs to the existing combine chain, producing one variable that carries both ordinary config and secret values.

### Benefits
- Tasks only ever need to remember one variable name.
- Slightly shorter combine chain (one `_config` instead of two).

### Costs
- Every place that reads or dumps `_{{ role_name }}_config` (a `debug` task during development, `ansible-playbook -v`/`-vvv`, a registered result printed on failure, a CI log) becomes a place a secret can leak from, regardless of whether that particular task actually needed the secret.
- Makes it impossible to `no_log: true` narrowly around only the tasks that touch secrets, since the same variable also carries non-secret values other tasks legitimately want visible in logs.

## Separate `_secret_config`, but stored as `!vault`-style inline encrypted scalars in the same file as non-secret vars

### Description
Keep one `group_vars/{{ group }}.yml` file per group/host; encrypt individual secret scalar values in place (Ansible Vault supports `!vault |` inline encrypted strings) rather than moving secrets to a separate file.

### Benefits
- No new file, no new directory structure (`group_vars/{{ group }}.yml` stays a single file).

### Costs
- Encrypting scalars inline with `ansible-vault encrypt_string` is materially more manual/error-prone to author and rotate than running `sops -e` over a whole file; every secret value needs its own individual vault-encrypt invocation.
- The user's stated storage requirement for this solution is SOPS specifically, not Ansible Vault; SOPS does not have an established "inline encrypted scalar mixed into a plaintext file" convention the way Ansible Vault's `!vault` tag does — SOPS instead operates at file granularity, encrypting every leaf value in whichever file it is pointed at.

## Separate `_secret_config`, sourced from SOPS-encrypted files (selected)

### Description
`{{ role_name }}_group_secret` lives in `group_vars/{{ group }}/secrets.sops.yaml`; `{{ role_name }}_secret` lives in `host_vars/{{ host }}/secrets.sops.yaml` — both encrypted with `sops` and auto-decrypted at inventory-load time by the `community.sops.sops` vars plugin (see [[../glossary/sops-secrets.md|SOPS-encrypted inventory secrets]]). The role's `vars/main.yml` combines these two inputs into `_{{ role_name }}_secret_config`, a variable structurally separate from `_{{ role_name }}_config`. This requires switching `group_vars`/`host_vars` from single files to per-group/per-host directories, so the encrypted secrets file can sit alongside the plaintext vars file.

### Benefits
- Secrets are structurally isolated: nothing can dump a secret by accident while inspecting `_{{ role_name }}_config`.
- Reuses `sops`'s whole-file encryption model as-is — no per-scalar vault-encrypt workflow.
- `no_log: true` can be applied narrowly, exactly to the tasks that actually read `_{{ role_name }}_secret_config`.

### Costs
- Requires every group/host that uses secrets to move from a flat `group_vars/{{ group }}.yml` / `host_vars/{{ host }}.yml` file to a `group_vars/{{ group }}/` / `host_vars/{{ host }}/` directory containing both `vars.yml` and `secrets.sops.yaml`.
- Requires the `community.sops` collection and the `sops` binary/key backend to be available everywhere Ansible actually runs, including CI — see [[../Implementation/ansible.cfg.extend.md|ansible.cfg]].
- No defaults tier exists for secrets (there is no safe default password), so required secret keys must be asserted present in role validation instead of falling back silently.
