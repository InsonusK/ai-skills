---
description: Merge the defaults/group/host variable tiers into the single effective config the role's tasks and templates read from
role_name: "{{ role_name }}"
name: vars/main.yml
element_kind: vars
change_kind: create
---

# Goals
- Compute the role's effective configuration exactly once, in one place, instead of repeating merge logic across tasks.
- Give tasks and templates a single variable to read from, regardless of which tier actually set a given key.
- Keep secret material in a variable that is structurally separate from the non-secret config, so a `debug`/verbose dump of the ordinary config can never accidentally print a secret.

# Core Principles
- Precedence is fixed and always applied in the same order: defaults → group → host (non-secret), and group-secret → host-secret (secret).
- The merge uses [[../../glossary/ansible-combine-filter.md|`combine`]] with `recursive=True`, so overriding one nested key at the group or host tier does not erase its untouched sibling keys.
- `| default({})` on every override input means a role works even when no `group_vars`/`host_vars` override exists at all.
- Secrets never flow through `_{{ role_name }}_config`. They have their own parallel chain and their own merged variable, `_{{ role_name }}_secret_config` — see [[../../glossary/sops-secrets.md|SOPS-encrypted inventory secrets]] for how `{{ role_name }}_group_secret` / `{{ role_name }}_secret` reach the inventory undecrypted at rest.
- The secret chain has no defaults tier: there is no safe default for a password or key, so `_{{ role_name }}_secret_defaults` deliberately does not exist. Required secret keys must instead be asserted present in `tasks/validation.yaml`, per [ansible-role-requirements.skill.md §2](../../../../../../ansible-role-requirements/SKILL.md).

# Naming convention

| use case | variable name pattern | example | file |
| --- | --- | --- | --- |
| group override tier (consumer-set, never defined by the role) | `{{ role_name }}_group` | `vpshost_vm_group` | `inventory/group_vars/{{ group }}/vars.yml` |
| host override tier (consumer-set, never defined by the role) | `{{ role_name }}` | `vpshost_vm` | `inventory/host_vars/{{ host }}/vars.yml` |
| secret group override tier (consumer-set, SOPS-encrypted, never defined by the role) | `{{ role_name }}_group_secret` | `vpshost_vm_group_secret` | `inventory/group_vars/{{ group }}/secrets.sops.yaml` |
| secret host override tier (consumer-set, SOPS-encrypted, never defined by the role) | `{{ role_name }}_secret` | `vpshost_vm_secret` | `inventory/host_vars/{{ host }}/secrets.sops.yaml` |
| merged/effective config (role-internal, read-only for tasks) | `_{{ role_name }}_config` | `_vpshost_vm_config` | `roles/{{ role_name }}/vars/main.yml` |
| merged/effective secret config (role-internal, read-only for tasks) | `_{{ role_name }}_secret_config` | `_vpshost_vm_secret_config` | `roles/{{ role_name }}/vars/main.yml` |

# Implementation changes

```yaml
# roles/{{ role_name }}/vars/main.yml
_{{ role_name }}_config: >-
  {{
    _{{ role_name }}_defaults
    | combine({{ role_name }}_group | default({}), recursive=True)
    | combine({{ role_name }} | default({}), recursive=True)
  }}

_{{ role_name }}_secret_config: >-
  {{
    {{ role_name }}_group_secret | default({})
    | combine({{ role_name }}_secret | default({}), recursive=True)
  }}
```

Tasks and templates then read only from `_{{ role_name }}_config` and `_{{ role_name }}_secret_config`. For a role literally named `vpshost_vm`:

```yaml
- name: Ensure vpshost_vm data directory exists
  file:
    path: "{{ _vpshost_vm_config.paths.data }}"
    state: directory

- name: Register vpshost_vm with the license server
  uri:
    url: "{{ _vpshost_vm_config.license_url }}"
    body:
      api_key: "{{ _vpshost_vm_secret_config.api_key }}"
    method: POST
  no_log: true
```

# Rule changes

## MUST
- The combine chain must be applied in the fixed order defaults → group → host; reversing the order silently changes which tier wins.
- Both `combine()` calls must pass `recursive=True` whenever any tier's value is a nested mapping.
- Both `{{ role_name }}_group` and `{{ role_name }}` inputs must be wrapped in `| default({})`.
- The merged variable must be named `_{{ role_name }}_config` — a name distinct from all three input tiers (`_{{ role_name }}_defaults`, `{{ role_name }}_group`, `{{ role_name }}`).
- Every task and template in the role must read from `_{{ role_name }}_config.*` only; the raw input tiers must not be referenced anywhere else in the role.
- The secret combine chain must be defined as its own variable, `_{{ role_name }}_secret_config`, distinct and separate from `_{{ role_name }}_config`.
- The secret combine chain must be applied in the fixed order group-secret → host-secret, with `recursive=True` and `| default({})` on both inputs, matching the non-secret chain's rules.
- Any task that logs, registers, or otherwise surfaces a value read from `_{{ role_name }}_secret_config` must set `no_log: true`.

## MUST NOT
- Must not reuse one of the input tier names (especially the bare `{{ role_name }}`) as the merged variable's name — Ansible's role-`vars/main.yml` precedence would then make the role's own file win over the host tier it is supposed to be merging in, silently discarding the host override.
- Must not recompute the merge again in a task (e.g. via `set_fact`) — a second computation can drift out of sync with this one and hides the actual precedence in a task file instead of `vars/main.yml`.
- Must not combine any secret input (`{{ role_name }}_group_secret`, `{{ role_name }}_secret`) into `_{{ role_name }}_config` — that would let an ordinary `debug: var=_{{ role_name }}_config` dump print a secret.
- Must not invent a `_{{ role_name }}_secret_defaults` variable containing placeholder or real secret values — there is no safe default for a secret; missing required secret keys must fail validation instead, per [ansible-role-requirements.skill.md §2](../../../../../../ansible-role-requirements/SKILL.md).

# Anti-patterns
- **Naming the merged config the same as one of its own inputs**
  - Consequence: because `vars/main.yml` variables have higher precedence than `host_vars`, defining `{{ role_name }}` as both the host-override input and the merge output makes the role's own file win over any host override for that name — the "host tier" silently stops working.
  - Instead: keep four distinct names — `_{{ role_name }}_defaults`, `{{ role_name }}_group`, `{{ role_name }}`, `_{{ role_name }}_config` — as documented in the naming convention table above.
- **Omitting `recursive=True` on a nested default structure**
  - Consequence: a group or host override that sets only one nested key (e.g. `paths: {data: /custom}`) wholesale replaces the entire `paths` mapping, silently dropping `paths.install` from the effective config.
  - Instead: always pass `recursive=True` when any tier value is a mapping — see [[../../glossary/ansible-combine-filter.md|combine filter]].
- **Reading `{{ role_name }}_group` or `{{ role_name }}` directly in a task instead of `_{{ role_name }}_config`**
  - Consequence: the task breaks the moment a host has no group/host override defined (the variable is simply undefined), even though the role has perfectly good defaults.
  - Instead: read only from `_{{ role_name }}_config`, which is always defined because it is anchored on `_{{ role_name }}_defaults`.
- **Merging secret tiers into the same `_{{ role_name }}_config` as everything else**
  - Consequence: any task, handler, or ad-hoc `-e`/`debug`/`ansible-playbook -v` dump of the ordinary config now risks printing a live secret, and every consumer of `_{{ role_name }}_config` becomes a place secrets could leak from.
  - Instead: keep secrets in their own `_{{ role_name }}_secret_config`, read only where a secret is actually needed, and mark that task `no_log: true`.
- **Giving the secret tier a fake "safe" default (e.g. an empty string or `changeme`)**
  - Consequence: the role silently runs with a bogus credential instead of failing fast, which can succeed against a test system and then fail — or worse, "succeed" insecurely — in production.
  - Instead: leave required secret keys undefined at the defaults level and assert their presence in `tasks/validation.yaml`.

# Check list
- [ ] `roles/{{ role_name }}/vars/main.yml` defines `_{{ role_name }}_config` combining defaults → group → host, in that order, with `recursive=True` on both combines.
- [ ] No task or template in the role references `{{ role_name }}_group` or `{{ role_name }}` directly.
- [ ] `roles/{{ role_name }}/vars/main.yml` defines `_{{ role_name }}_secret_config` combining group-secret → host-secret, separate from `_{{ role_name }}_config`.
- [ ] No task combines a secret input into `_{{ role_name }}_config`.
- [ ] Every task reading `_{{ role_name }}_secret_config.*` sets `no_log: true`.
- [ ] `tasks/validation.yaml` asserts every required key of `_{{ role_name }}_secret_config` is actually defined.
