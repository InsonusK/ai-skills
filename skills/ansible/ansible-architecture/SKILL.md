---
name: Ansible Architecture Rules
description: How to write andisble playbooks, roles and tasks following architecture rules
---
# Ansible Storybook Skill (Architecture Rules)

## 1. Domain separation

- All configuration is divided into **domains**
  - dns
  - certificates
  - crl
  - vpn
  - monitoring
  - etc

- Each domain is an independent bounded context.

---

## 2. Domain Playbook (Storybook entrypoint)

### 2.1 Responsibilities

A playbook MUST:

- NOT contain business logic
- ONLY orchestrate execution

Allowed responsibilities:
- 2.1.1 Orchestration of:
  - roles
  - task lists (include_tasks)

- 2.1.2 Definition of target scope:
  - hosts
  - groups

- 2.1.3 Execution of validation BEFORE any action:
  - validation task MUST be first executed step

---

## 3. Roles (Infrastructure definition)

### 3.1 Purpose

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

### 3.2 Validation requirement

Every role MUST
- include `validation.yaml` IF:
  - not all input variables have safe defaults
  - or external dependencies exist
- validation MUST be executed at role entry
- sepparate:
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

### 3.3 Command execution inside role
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

### 3.4 When NOT to put actions in role

Actions MUST NOT be placed in role if:
- they require cross-host orchestration
- they depend on global workflow state
- they require distributed coordination (cluster-wide backup, migration, etc.)

In these cases:
- use playbook/task list orchestration layer

---

## 4. Task Lists (Procedures / Pipelines)

### 4.1 Purpose

Task lists define:
- cross-role workflows
- orchestration of multiple capabilities
- multi-host operations
- business processes (backup campaigns, migrations, rollouts)

Task list is a workflow engine layer, NOT host capability.

---

### 4.2 Requirements

Task lists MAY:
- call role capabilities (actions)
- coordinate multiple roles
- define execution order across hosts
- aggregate results
- implement pipelines (backup all DB nodes, then sync, then verify)

Task lists MUST NOT:
- define host-level state
- duplicate role configuration logic
- implement low-level system setup

---

### 4.3 Example: correct usage (NEW)

```yaml
- name: Run backup on DB nodes
  hosts: db_nodes
  tasks:
    - include_role:
        name: database
      vars:
        server_action: backup
```

or workflow orchestration:

```yaml
- name: Backup cluster
  hosts: db_nodes
  tasks:
    - include_role:
        name: database
      vars:
        server_action: backup

    - include_tasks: sync_backup.yaml
```

---

### 4.4 Key principle

Task list = composition of capabilities
NOT implementation of capabilities.

---

## 5. Validation rule (global)

- `validation.yaml` MUST be executed:
  - at start of every role
  - at start of every task list
  - at start of every playbook

- Validation is mandatory fail-fast mechanism

---

## 6. Default values handling rule (IMPORTANT)

If a task list defines **default values for input parameters**, then:

### 6.1 validation.yaml MUST contain initialization step

At the beginning of `validation.yaml`, default values MUST be set explicitly:

```yaml
# Set default values
# Установка значений по умолчанию
- name: set default values
  set_fact:
    ...
  when: skip_set_fact is not defined or not skip_set_fact
```

### 6.2 Execution model

There are TWO execution modes:

A. Playbook level execution (default behavior)
- Playbook calls validation WITHOUT skip_set_fact
- Default values ARE applied

```yaml
- name: Validate deploy prerequisites
  include_tasks: validation.yaml
```

B. Task list internal execution (strict validation mode)
- Task list MUST call validation with skip_set_fact: true
- Default values MUST NOT be applied
- Only validation is performed

```yaml
- name: Validate deploy prerequisites
  include_tasks: validation.yaml
  vars:
    skip_set_fact: true
```

6.3 Rule summary
- Playbook = applies defaults + validates
- Task list = validates only (no mutation of state)
- Default assignment MUST be centralized in validation.yaml
- skip_set_fact controls mutation vs validation-only mode

---

## 7. Directory structure standard

```text
ansible/
  modules/
    {{ module_name }}/

      playbooks/
        {{ playbook_name }}.yaml

      tasks/
        {{ task_list_name }}/
          README.md
          templates/
          task/
            # optional subtask decomposition
          main.yaml
          validation.yaml

      roles/
        {{ role_name }}/
          README.md
          tasks/
            main.yaml
            validation.yaml
          # standard Ansible role structure allowed
```

---

## 8. Execution hierarchy

```
Playbook (storybook)
  ├── validation.yaml (global scope)
  ├── Task Lists (workflow layer)
  │     ├── orchestration across roles
  │     ├── multi-host coordination
  │     └── process pipelines
  │
  └── Roles (capability + state layer)
        ├── validation.yaml
        ├── state/
        └── commands/
              ├── backup.yaml
              ├── restore.yaml
              └── etc
```

## 9. Key principles
- Playbooks = orchestration entrypoint
- Roles = state + host capabilities
- Tasks = cross-role workflows
- Commands belong to roles when they are host-scoped capabilities
- Validation = mandatory entry gate everywhere
- No business logic inside playbooks
- No implicit assumptions without validation