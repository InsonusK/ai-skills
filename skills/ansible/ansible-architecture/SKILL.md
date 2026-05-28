---
name: ansible-architecture
description: >
  High-level architectural concepts for Ansible projects.
  Defines the three execution layers: playbooks, task lists, and roles.
type: architecture
domain: infrastructure
tags:
  - ansible
  - architecture
  - playbook
  - role
  - task-list
---

# Ansible Storybook Skill (Architecture Concepts)

This skill defines the **high-level architectural concepts** for organizing Ansible code.  
For concrete implementation rules see the linked specialized skills below.

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

## 2. Three layers of execution

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

### 2.1 Playbook — orchestration entrypoint
- **Purpose:** ONLY orchestrate execution
- **Must NOT contain:** business logic
- **See details:** [Ansible Playbook Requirements](../ansible-playbook-requirements/SKILL.md)

### 2.2 Task Lists — workflow engine
- **Purpose:** cross-role workflows, multi-host operations, business processes
- **Is:** composition of capabilities
- **Is NOT:** implementation of capabilities
- **See details:** [Ansible Task List Requirements](../ansible-task-list-requirements/SKILL.md)

### 2.3 Roles — state + host capabilities
- **Purpose:** infrastructure configuration, reusable components, state definitions, host capabilities
- **Defines:** what the host is able to do (backup, restore, verify, etc.)
- **See details:** [Ansible Role Requirements](../ansible-role-requirements/SKILL.md)

---

## 3. Cross-cutting requirements

Requirements that apply to **all** layers (playbooks, roles, task lists):

- Global validation rule
- Default values handling rule
- Directory structure standard
- Domain separation

**See details:** [Ansible General Requirements](../ansible-general-requirements/SKILL.md)

---

## 4. Key principles

| Principle | Description |
|-----------|-------------|
| Playbooks | orchestration entrypoint |
| Roles | state + host capabilities |
| Tasks | cross-role workflows |
| Commands | belong to roles when they are host-scoped capabilities |
| Validation | mandatory entry gate everywhere |
| No business logic | inside playbooks |
| No implicit assumptions | without validation |
