---
name: solution-ansible-role-var
description: Give every Ansible role its own variable namespace — a three-tier non-secret config (role defaults, group override, host override) plus a parallel, structurally separate SOPS-encrypted secret config (group-secret, host-secret) — merged via `combine`, so no two roles collide on a variable name, one variable change never ripples across unrelated files, and secrets can never leak through an ordinary config dump
domain: skill
type: architecture
version: 1.1.0
tags:
  - skill/architecture/solution
  - ansible
  - role
  - variables
  - inventory
  - secrets
  - stack/ansible
  - concern/architecture

triggers:
  - Creating a new Ansible role that accepts configurable parameters
  - A role's variable name collides with another role's variable in the same group_vars/host_vars scope
  - Changing one role's variable structure currently requires editing multiple unrelated inventory files
  - A role needs a credential, API key, or other secret sourced from inventory
creates:
  - "roles/{{ role_name }}/defaults/main.yml"
  - "roles/{{ role_name }}/vars/main.yml"
extends:
  - "inventory/group_vars/{{ group }}/vars.yml"
  - "inventory/group_vars/{{ group }}/secrets.sops.yaml"
  - "inventory/host_vars/{{ host }}/vars.yml"
  - "inventory/host_vars/{{ host }}/secrets.sops.yaml"
  - "ansible.cfg"
depends_on:
adr:
  - "[[adr/three-tier-variable-precedence.md|Three-tier variable precedence]]"
  - "[[adr/secret-tier-isolation.md|Secret tier isolation]]"
---

# Goal
- Give every role a single, unique variable namespace so no two roles can collide on the same variable name in inventory.
- Let a variable be set at the right altitude — role default, whole group, or a single host — without ever editing the role itself.
- Make changing one role's variable structure a one-role change, not a multi-file hunt across the inventory.
- Let a role accept secrets (credentials, API keys) from inventory without ever risking one leaking through an ordinary config dump.

# Capabilities
- A role's variables can never be shadowed by another role's variables, because every tier is prefixed with the role's own name.
- Consumers (playbooks/inventory) can override just the group or just the host, or both, or neither, and always get a fully-defined effective configuration.
- Tasks and templates read one variable (`_{{ role_name }}_config`) regardless of which tier actually supplied a given key.
- Secrets are sourced from SOPS-encrypted inventory files and merged into their own `_{{ role_name }}_secret_config`, kept structurally apart from the non-secret config.

# Core Principles
- Each role owns its own variable structure — every tier is prefixed with the role's own `{{ role_name }}`, so renaming or restructuring one role's variables cannot silently affect another role.
- Precedence is fixed: non-secret tiers merge defaults → group → host; secret tiers merge group-secret → host-secret. Neither chain is ever reordered per role.
- Each chain is computed once, in the role's `vars/main.yml`, never duplicated in tasks.
- Secret and non-secret data never share a variable — `_{{ role_name }}_config` and `_{{ role_name }}_secret_config` are always separate, so nothing that can legitimately be logged/dumped ever carries something that can't.

# Adr
- [[adr/three-tier-variable-precedence.md|Three-tier variable precedence]]
  - Selected variant: [[adr/three-tier-variable-precedence.md#Three-tier prefixed combine (selected)|Three-tier prefixed combine]]
- [[adr/secret-tier-isolation.md|Secret tier isolation]]
  - Selected variant: [[adr/secret-tier-isolation.md#Separate _secret_config, sourced from SOPS-encrypted files (selected)|Separate `_secret_config`, sourced from SOPS-encrypted files]]

# Requirements
SOLUTION:
- Related, narrower guidance already in the repo: [ansible-role-requirements.skill.md §6](../../../../ansible-role-requirements/SKILL.md) documents a two-tier `_defaults` + `_override` combine pattern for a single override input. This solution generalizes that pattern to three fixed tiers (defaults, group, host) when a role needs group-level and host-level overrides to be independent, ordered inputs rather than one shared `_override` variable.
- [ansible-role-requirements.skill.md §2](../../../../ansible-role-requirements/SKILL.md) (validation requirement) — since the secret tier has no defaults, required secret keys must be asserted present in the role's `tasks/validation.yaml`.

ANSIBLE-CORE:
- `combine` filter (built-in, no collection required) — see [[glossary/ansible-combine-filter.md|Ansible combine filter]] for merge semantics, `recursive=True`, and `default({})`.

ANSIBLE GALAXY COLLECTION:
- `community.sops` — provides the `community.sops.sops` vars plugin used to auto-decrypt `*.sops.yaml` files in `group_vars`/`host_vars`. See [[glossary/sops-secrets.md|SOPS-encrypted inventory secrets]] and [[Implementation/ansible.cfg.extend.md|ansible.cfg]] for the one-time project setup this depends on.

# Template Skill Mutations
FILES:
- [[./Implementation/defaults/main.yml.create.md|defaults/main.yml]] - create - defines `_{{ role_name }}_defaults`, the full safe key set
- [[./Implementation/vars/main.yml.create.md|vars/main.yml]] - create - combines defaults → group → host into `_{{ role_name }}_config`, and group-secret → host-secret into `_{{ role_name }}_secret_config`
- [[./Implementation/inventory/group_vars/{group}/vars.yml.create.md|inventory/group_vars/{{ group }}/vars.yml]] - create - consumer-side example of the plaintext group override tier
- [[./Implementation/inventory/group_vars/{group}/secrets.sops.yaml.create.md|inventory/group_vars/{{ group }}/secrets.sops.yaml]] - create - consumer-side example of the SOPS-encrypted group secret tier
- [[./Implementation/inventory/host_vars/{host}/vars.yml.create.md|inventory/host_vars/{{ host }}/vars.yml]] - create - consumer-side example of the plaintext host override tier
- [[./Implementation/inventory/host_vars/{host}/secrets.sops.yaml.create.md|inventory/host_vars/{{ host }}/secrets.sops.yaml]] - create - consumer-side example of the SOPS-encrypted host secret tier
- [[./Implementation/ansible.cfg.extend.md|ansible.cfg]] - extend - enables the `community.sops.sops` vars plugin project-wide, once

# Workflow

## Define a role's variables (happy path)
1. Author adds `_{{ role_name }}_defaults` to `roles/{{ role_name }}/defaults/main.yml` with every key the role will consume, each given a safe value.
2. Author adds the combine expression to `roles/{{ role_name }}/vars/main.yml`, producing `_{{ role_name }}_config` from `_{{ role_name }}_defaults`, `{{ role_name }}_group`, and `{{ role_name }}`.
3. Role tasks/templates read exclusively from `_{{ role_name }}_config.*`.
4. A consumer optionally sets `{{ role_name }}_group` in `group_vars/{{ group }}/vars.yml` to override keys for a whole group.
5. A consumer optionally sets `{{ role_name }}` in `host_vars/{{ host }}/vars.yml` to override keys for one host.
6. At play time, Ansible resolves `_{{ role_name }}_config` by evaluating the combine chain against whichever of the three tiers are actually defined.

```mermaid
flowchart LR
    D["_{{ role_name }}_defaults<br/>(defaults/main.yml)"] -->|combine, recursive=True| M1["merge step 1"]
    G["{{ role_name }}_group<br/>(group_vars, optional)"] -->|combine, recursive=True| M1
    M1 --> M2["merge step 2"]
    H["{{ role_name }}<br/>(host_vars, optional)"] --> M2
    M2 --> C["_{{ role_name }}_config<br/>(vars/main.yml)"]
    C --> T["tasks / templates"]
```

## No group or host override defined
1. `{{ role_name }}_group` and `{{ role_name }}` are both undefined for the host being played.
2. `| default({})` in both `combine()` calls substitutes an empty mapping for each, so the merge does not fail.
3. `_{{ role_name }}_config` equals `_{{ role_name }}_defaults` unchanged.

## Overriding a nested key
1. `_{{ role_name }}_defaults` defines a nested mapping, e.g. `paths: {install: ..., data: ...}`.
2. `{{ role_name }}_group` sets only `paths: {data: /custom}`.
3. Because both `combine()` calls use `recursive=True`, the merge keeps `paths.install` from the defaults and only overwrites `paths.data` — see [[glossary/ansible-combine-filter.md|combine filter]].
4. Without `recursive=True`, step 3 would instead replace the whole `paths` mapping, silently losing `paths.install`.

## Define a role's secrets (happy path)
1. Author adds the secret combine expression to `roles/{{ role_name }}/vars/main.yml`, producing `_{{ role_name }}_secret_config` from `{{ role_name }}_group_secret` and `{{ role_name }}_secret` — see [[./Implementation/vars/main.yml.create.md|vars/main.yml]].
2. Author adds an assertion to `tasks/validation.yaml` that every secret key the role actually requires is present in `_{{ role_name }}_secret_config`, since there is no defaults tier to fall back on.
3. A consumer runs `sops -e` on `group_vars/{{ group }}/secrets.sops.yaml` and/or `host_vars/{{ host }}/secrets.sops.yaml` to set `{{ role_name }}_group_secret` / `{{ role_name }}_secret`, encrypted at rest.
4. At play time, the `community.sops.sops` vars plugin decrypts those files and merges the result into the normal variable namespace, exactly like a plaintext `group_vars`/`host_vars` file — see [[glossary/sops-secrets.md|SOPS-encrypted inventory secrets]].
5. `roles/{{ role_name }}/vars/main.yml` resolves `_{{ role_name }}_secret_config` from the decrypted values.
6. Any task that reads `_{{ role_name }}_secret_config.*` sets `no_log: true`.

```mermaid
flowchart LR
    GS["{{ role_name }}_group_secret<br/>(group_vars/*/secrets.sops.yaml, SOPS-encrypted)"] -->|decrypt via community.sops.sops| GSd["decrypted"]
    HS["{{ role_name }}_secret<br/>(host_vars/*/secrets.sops.yaml, SOPS-encrypted)"] -->|decrypt via community.sops.sops| HSd["decrypted"]
    GSd -->|combine, recursive=True| SC["_{{ role_name }}_secret_config<br/>(vars/main.yml)"]
    HSd --> SC
    SC -->|no_log: true| T["tasks that need the secret"]
```

## Required secret missing
1. Neither `{{ role_name }}_group_secret` nor `{{ role_name }}_secret` sets a key the role actually requires.
2. `_{{ role_name }}_secret_config` resolves without that key (there is no defaults tier to fall back to).
3. `tasks/validation.yaml`'s assertion for that key fails at role entry, with a clear error — instead of the role running with a missing/empty credential.

# Rules

## MUST
- [[./Implementation/defaults/main.yml.create.md#MUST|defaults/main.yml]]
- [[./Implementation/vars/main.yml.create.md#MUST|vars/main.yml]]
- [[./Implementation/inventory/group_vars/{group}/vars.yml.create.md#MUST|group_vars/{{ group }}/vars.yml]]
- [[./Implementation/inventory/group_vars/{group}/secrets.sops.yaml.create.md#MUST|group_vars/{{ group }}/secrets.sops.yaml]]
- [[./Implementation/inventory/host_vars/{host}/vars.yml.create.md#MUST|host_vars/{{ host }}/vars.yml]]
- [[./Implementation/inventory/host_vars/{host}/secrets.sops.yaml.create.md#MUST|host_vars/{{ host }}/secrets.sops.yaml]]
- [[./Implementation/ansible.cfg.extend.md#MUST|ansible.cfg]]

## SHOULD
- [[./Implementation/defaults/main.yml.create.md#SHOULD|defaults/main.yml]]

## MUST NOT
- [[./Implementation/defaults/main.yml.create.md#MUST NOT|defaults/main.yml]]
- [[./Implementation/vars/main.yml.create.md#MUST NOT|vars/main.yml]]
- [[./Implementation/inventory/group_vars/{group}/vars.yml.create.md#MUST NOT|group_vars/{{ group }}/vars.yml]]
- [[./Implementation/inventory/group_vars/{group}/secrets.sops.yaml.create.md#MUST NOT|group_vars/{{ group }}/secrets.sops.yaml]]
- [[./Implementation/inventory/host_vars/{host}/vars.yml.create.md#MUST NOT|host_vars/{{ host }}/vars.yml]]
- [[./Implementation/inventory/host_vars/{host}/secrets.sops.yaml.create.md#MUST NOT|host_vars/{{ host }}/secrets.sops.yaml]]
- [[./Implementation/ansible.cfg.extend.md#MUST NOT|ansible.cfg]]

# Anti-patterns
- **Reusing a generic, unprefixed variable name across roles (e.g. `port`, `config`, `enabled`)**
  - Consequence: two roles collide the moment they are used together in the same inventory scope; overriding one role's `port` in `group_vars` can accidentally feed the wrong value into another role.
  - Instead: prefix every tier with the role's own unique name, as in [[./Implementation/defaults/main.yml.create.md|defaults/main.yml]].
- **Reading `{{ role_name }}_group` or `{{ role_name }}` directly inside tasks instead of `_{{ role_name }}_config`**
  - Consequence: the task breaks with an undefined-variable error the moment a host or group has no override defined, even though the role has perfectly good defaults.
  - Instead: always read from `_{{ role_name }}_config`, per [[./Implementation/vars/main.yml.create.md|vars/main.yml]].
- **Naming the merged config the same as one of its own input tiers**
  - Consequence: Ansible's `vars/main.yml` precedence over `host_vars` means the role's own file wins over the host tier it was supposed to merge in, silently discarding host-level overrides.
  - Instead: keep four distinct names — `_{{ role_name }}_defaults`, `{{ role_name }}_group`, `{{ role_name }}`, `_{{ role_name }}_config` — see [[./Implementation/vars/main.yml.create.md|vars/main.yml]].
- **Omitting `recursive=True` when a tier's value is a nested mapping**
  - Consequence: a group or host override that sets a single nested key wholesale replaces the entire nested mapping, silently dropping untouched sibling keys — see [[glossary/ansible-combine-filter.md|combine filter]].
  - Instead: always pass `recursive=True` when any tier value is a mapping.
- **Adding a new key only at the group or host tier, never in `_{{ role_name }}_defaults`**
  - Consequence: the role has no safe fallback; running it against a host/group that forgot to set the key fails deep inside a task instead of surfacing a clear, documented default.
  - Instead: add every new key to `_{{ role_name }}_defaults` first, per [[./Implementation/defaults/main.yml.create.md|defaults/main.yml]].
- **Recomputing the merge again in a task via `set_fact`**
  - Consequence: a second, independent merge computation can drift out of sync with the one in `vars/main.yml`, and the actual effective precedence becomes unclear.
  - Instead: compute `_{{ role_name }}_config` once, in `vars/main.yml`.
- **Merging secret tiers into `_{{ role_name }}_config` instead of a separate `_{{ role_name }}_secret_config`**
  - Consequence: any `debug`/verbose dump, registered-and-printed result, or CI log of the ordinary config now risks leaking a live secret.
  - Instead: keep secrets in their own `_{{ role_name }}_secret_config`, per [[./Implementation/vars/main.yml.create.md|vars/main.yml]] and [[adr/secret-tier-isolation.md|Secret tier isolation]].
- **Committing a `secrets.sops.yaml` file before running `sops -e`**
  - Consequence: the secret is now in plaintext in version control history; rotating the value afterward does not undo the exposure.
  - Instead: always encrypt with `sops` before `git add` — see [[./Implementation/inventory/group_vars/{group}/secrets.sops.yaml.create.md|secrets.sops.yaml]].
- **Inventing a "safe default" secret value (e.g. an empty string or `changeme`) instead of leaving it unset**
  - Consequence: the role runs with a bogus credential instead of failing fast at validation.
  - Instead: leave required secret keys undefined at the defaults level and assert their presence in `tasks/validation.yaml`.

# Check list
- [ ] `roles/{{ role_name }}/defaults/main.yml` defines `_{{ role_name }}_defaults` with every key the role consumes.
- [ ] `roles/{{ role_name }}/vars/main.yml` defines `_{{ role_name }}_config`, combining defaults → group → host with `recursive=True` on both combines.
- [ ] No task or template in the role reads `{{ role_name }}_group` or `{{ role_name }}` directly — only `_{{ role_name }}_config`.
- [ ] Any `group_vars`/`host_vars` overrides in the role's `example/` folder use `{{ role_name }}_group` / `{{ role_name }}` and list only the keys they actually override.
- [ ] No other role in the project reuses this role's `{{ role_name }}` prefix.
- [ ] `roles/{{ role_name }}/vars/main.yml` defines `_{{ role_name }}_secret_config` combining group-secret → host-secret, structurally separate from `_{{ role_name }}_config`.
- [ ] Every `secrets.sops.yaml` file is encrypted with `sops`; none is committed in plaintext.
- [ ] `tasks/validation.yaml` asserts every secret key the role requires is present in `_{{ role_name }}_secret_config`.
- [ ] Every task reading `_{{ role_name }}_secret_config.*` sets `no_log: true`.
- [ ] `ansible.cfg` enables `community.sops.sops` alongside `host_group_vars` in `vars_plugins_enabled`.
