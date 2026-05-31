---
name: ansible-role-requirements

description: Architecture rules for Ansible roles. Covers purpose, validation, command execution, and structural boundaries.

metadata:
  domain: infrastructure
  tags:
  - ansible
  - role
  - capabilities
  - validation
  - requirements
  ai_hints:
    category: guide
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
- include validation IF:
  - not all input variables have safe defaults
  - or external dependencies exist
- validation MUST be executed at role entry
- separate:
  - state layer (idempotent configuration)
  - capability layer (commands / procedures)

> **HIGHLIGHT:** All validation, assertions, and pre-flight checks that can be performed **before** executing any command **MUST** be done in validation. Do NOT defer checks to runtime if they can be validated upfront.

### 2.1 Simple validation (single `validation.yaml`)

Use when validation logic is the same across all actions:

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

### 2.2 Per-action validation (split validation files)

Use when validation differs between commands/actions. Store split validations under `tasks/validations/`:

```
roles/{{ role_name }}/
  tasks/
    main.yaml
    validation.yaml          # optional: common/shared validation

    validations/
      action1-validation.yaml
      action2-validation.yaml
      action3-validation.yaml

    state/
      install.yaml
      configure.yaml

    commands/
      backup.yaml
      restore.yaml
      rotate.yaml
      verify.yaml
```

*`tasks/validation.yaml` (example dispatcher or common validation)*
```yaml
- name: Validate common inputs
  assert:
    that:
      - required_var is defined
    fail_msg: "required_var must be defined"

- name: Include per-action validation
  include_tasks: "validations/{{ server_action }}-validation.yaml"
  when: server_action is defined
```

*`tasks/validations/backup-validation.yaml`*
```yaml
- name: Validate backup-specific inputs
  assert:
    that:
      - backup_retention_days is defined
      - backup_retention_days | int > 0
    fail_msg: "backup_retention_days must be a positive integer"
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

---

## 5. Example requirement

Every role MUST contain an `example/` folder at the role root with working demonstration playbooks.

### 5.1 Folder structure

```
roles/{{ role_name }}/
  example/
    playbook.yaml
    inventory/
      hosts.yaml
    group_vars/
      all.yaml
    host_vars/
      node1.yaml
      node2.yaml
```

### 5.2 Example playbook requirements

The example playbook MUST:

- call **every action** exposed by the role (`deploy`, `backup`, `restore`, `rotate`, `verify`, etc.)
- demonstrate **different variable sets** (minimal, full, override)
- be executable without modification for local testing (e.g. `ansible-playbook -i inventory/hosts.yaml playbook.yaml`)

### 5.3 Example content

*`example/playbook.yaml`*
```yaml
- name: Demonstrate all role capabilities
  hosts: all
  gather_facts: true
  roles:
    - role: "{{ playbook_dir }}/.."
      vars:
        server_action: deploy
        app_version: "1.2.3"

    - role: "{{ playbook_dir }}/.."
      vars:
        server_action: backup
        backup_retention_days: 14

    - role: "{{ playbook_dir }}/.."
      vars:
        server_action: restore
        restore_source: "/backups/latest.tar.gz"

    - role: "{{ playbook_dir }}/.."
      vars:
        server_action: rotate
        rotation_policy: "daily"

    - role: "{{ playbook_dir }}/.."
      vars:
        server_action: verify
        verify_checksums: true
```

*`example/group_vars/all.yaml`* — full variable set
```yaml
app_name: myapp
app_version: "1.0.0"
install_path: "/opt/myapp"
config_template: "myapp.conf.j2"
backup_enabled: true
backup_retention_days: 7
backup_destination: "/backups"
rotation_policy: "weekly"
verify_checksums: false
```

*`example/host_vars/node1.yaml`* — override for specific host
```yaml
app_version: "2.0.0-beta"
backup_retention_days: 3
verify_checksums: true
```

*`example/host_vars/node2.yaml`* — minimal override
```yaml
backup_enabled: false
```

*`example/inventory/hosts.yaml`*
```yaml
all:
  children:
    webservers:
      hosts:
        node1:
          ansible_host: 127.0.0.1
          ansible_connection: local
        node2:
          ansible_host: 127.0.0.1
          ansible_connection: local
```

Rules:
- `example/` MUST be present at role root
- example MUST cover all actions defined in `vars/main.yaml` → `supported_actions`
- example MUST show at least three variable profiles (default/minimal, full, host-specific override)
- example MUST use relative path `{{ playbook_dir }}/..` to reference the role being demonstrated

---

## 6. Override-friendly dictionary variables

Any role variable that is a **dictionary (mapping) structure** and whose individual keys MAY be overridden by the consumer MUST be implemented using the **defaults + override + combine** pattern.

### 6.1 Pattern definition

In `defaults/main.yaml` (or `vars/main.yaml` for internal constants):

```yaml
structure_name_defaults:
  value1: 1
  value2: 2
```

In the same file, immediately after the `_defaults` variable:

```yaml
structure_name: "{{ structure_name_defaults | combine(structure_name_override | default({})) }}"
```

### 6.2 Consumer-side override

In an inventory file (for example `group_vars/ca_servers.yml`), the consumer overrides only the keys that need to change:

```yaml
# group_vars/ca_servers.yml
structure_name_override:
  value2: 20
```

The resulting `structure_name` will be:

```yaml
value1: 1   # taken from structure_name_defaults
value2: 20  # overridden by structure_name_override
```

### 6.3 Rules

- The `_defaults` variable MUST contain the full set of keys with safe default values.
- The resulting variable MUST be named without the `_defaults` suffix and MUST use the `combine` filter with the `_override` variable.
- The `_override` variable MUST NOT be defined in the role defaults; it is reserved for consumer input (inventory, extra vars, etc.).
- This pattern MUST be used for all dictionary parameters that are expected to be customized per-host or per-group.
