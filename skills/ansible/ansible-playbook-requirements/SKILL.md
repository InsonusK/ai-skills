---
name: ansible-playbook-requirements
description: >
  Architecture rules for Ansible playbooks.
  Defines orchestration responsibilities, target scope, and validation gates.
type: guide
domain: infrastructure
tags:
  - ansible
  - playbook
  - orchestration
  - requirements
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
