---
name: ansible-task-list-requirements

description: Architecture rules for Ansible task lists. Defines cross-role workflows, pipelines, and orchestration boundaries.

metadata:
  domain: infrastructure
  tags:
  - ansible
  - task-list
  - workflow
  - pipeline
  - requirements
  ai_hints:
    category: guide
---

# Ansible Task List Requirements

This skill defines requirements for **Ansible task lists** — the workflow and procedure layer of the architecture.

---

## 1. Purpose

Task lists define:
- cross-role workflows
- orchestration of multiple capabilities
- multi-host operations
- business processes (backup campaigns, migrations, rollouts)

Task list is a workflow engine layer, NOT host capability.

---

## 2. Requirements

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

### 2.1 Validation requirement

Task lists MUST perform all validation and assertions that can be done **before** executing any command or role action.

> **HIGHLIGHT:** All validation and assertion which we can do before run command we MUST do in validation. Do NOT defer checks to runtime if they can be validated upfront.

- Validate inputs, variables, and preconditions at the start of the workflow
- Use `assert` or `fail` modules to stop early on invalid configuration
- If validation differs per step or action, split validation into separate task files

Split validation structure (example):
```
task_lists/
  workflow.yaml
  validations/
    step1-validation.yaml
    step2-validation.yaml
```

---

## 3. Example: correct usage

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

## 4. Key principle

Task list = composition of capabilities  
NOT implementation of capabilities.
