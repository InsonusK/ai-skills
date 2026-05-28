---
name: Ansible Task List Requirements
description: Architecture rules for writing Ansible task lists — workflows, pipelines and cross-role orchestration
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
