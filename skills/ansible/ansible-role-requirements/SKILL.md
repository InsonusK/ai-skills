---
name: ansible-role-requirements
description: >
  Architecture rules for Ansible roles.
  Covers purpose, validation, command execution, and structural boundaries.
type: guide
domain: infrastructure
tags:
  - ansible
  - role
  - capabilities
  - validation
  - requirements
---

# Ansible Role Requirements

This skill defines requirements for **Ansible roles** — the capability and state layer of the architecture.

---

## 1. Purpose

Roles define:
- infrastructure configuration rules
- reusable system components
- state definitions
- capabilities of a host
- embedded operational functions (optional, bounded)

Role is NOT only configuration — it also defines what the host is able to do.

Examples of capabilities:
- backup execution capability
- restore capability
- rotation capability
- verification capability

These capabilities MAY be implemented as internal role commands.

---

## 2. Validation requirement

Every role MUST
- include `validation.yaml` IF:
  - not all input variables have safe defaults
  - or external dependencies exist
- validation MUST be executed at role entry
- separate:
  - state layer (idempotent configuration)
  - capability layer (commands / procedures)

Recommended structure:
```
roles/{{ role_name }}/
  tasks/
    main.yaml
    validation.yaml

    state/
      install.yaml
      configure.yaml

    commands/
      backup.yaml
      restore.yaml
      rotate.yaml
      verify.yaml
```

---

## 3. Command execution inside role

Roles MAY expose internal actions via:

*tasks/validation.yaml*
```yaml
- name: Validate action
  assert:
    that:
      - server_action in supported_actions
    fail_msg: >
      Invalid server_action '{{ server_action }}'.
      Supported: {{ supported_actions | join(', ') }}
```

*tasks/main.yaml*
```yaml
- name: Execute action
  include_tasks: "actions/{{ server_action }}.yaml"
  when: server_action is defined
```

*vars/main.yaml*
```yaml
supported_actions:
  - deploy
  - backup
```

Rules:
- commands are part of the role boundary (NOT external playbook logic)
- commands MUST be explicit files, not inline logic
- commands MUST remain idempotent where possible
- list of `supported_actions` store in `vars` values

---

## 4. When NOT to put actions in role

Actions MUST NOT be placed in role if:
- they require cross-host orchestration
- they depend on global workflow state
- they require distributed coordination (cluster-wide backup, migration, etc.)

In these cases:
- use playbook/task list orchestration layer
