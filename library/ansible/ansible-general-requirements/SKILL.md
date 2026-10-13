---
name: ansible-general-requirements

description: Cross-cutting rules for all Ansible artifacts. Covers validation, default values handling, and directory structure standards.

metadata:
  domain: infrastructure
  tags:
  - ansible
  - validation
  - defaults
  - directory-structure
  - requirements
  ai_hints:
    category: guide
---

# Ansible General Requirements

This skill defines cross-cutting requirements applicable to **all** Ansible artifacts: playbooks, roles, and task lists.

---

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

## 2. Global validation rule

- `validation.yaml` MUST be executed:
  - at start of every role
  - at start of every task list
  - at start of every playbook

- Validation is mandatory fail-fast mechanism

---

## 3. Default values handling rule

If an artifact defines **default values for input parameters**, then:

### 3.1 validation.yaml MUST contain initialization step

At the beginning of `validation.yaml`, default values MUST be set explicitly:

```yaml
# Set default values
# Установка значений по умолчанию
- name: set default values
  set_fact:
    ...
  when: skip_set_fact is not defined or not skip_set_fact
```

### 3.2 Execution model

There are TWO execution modes:

**A. Playbook level execution (default behavior)**
- Playbook calls validation WITHOUT `skip_set_fact`
- Default values ARE applied

```yaml
- name: Validate deploy prerequisites
  include_tasks: validation.yaml
```

**B. Task list / Role internal execution (strict validation mode)**
- Task list / Role MUST call validation with `skip_set_fact: true`
- Default values MUST NOT be applied
- Only validation is performed

```yaml
- name: Validate deploy prerequisites
  include_tasks: validation.yaml
  vars:
    skip_set_fact: true
```

### 3.3 Rule summary
- Playbook = applies defaults + validates
- Task list = validates only (no mutation of state)
- Default assignment MUST be centralized in `validation.yaml`
- `skip_set_fact` controls mutation vs validation-only mode

---

## 4. Directory structure standard

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

## 5. Execution hierarchy

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
