---
name: Ansible Playbook Requirements
description: Architecture rules for writing Ansible playbooks — orchestration, scope definition and validation
---

# Ansible Playbook Requirements

This skill defines requirements for **Ansible playbooks** — the storybook entrypoint of the architecture.

---

## 1. Responsibilities

A playbook MUST:

- NOT contain business logic
- ONLY orchestrate execution

Allowed responsibilities:

### 1.1 Orchestration of:
- roles
- task lists (`include_tasks`)

### 1.2 Definition of target scope:
- hosts
- groups

### 1.3 Execution of validation BEFORE any action:
- validation task MUST be first executed step
