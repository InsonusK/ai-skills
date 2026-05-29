---
name: ansible-tasks-documentation

description: Guidelines for writing documentation inside Ansible task files. Covers bilingual comments and parameter descriptions.

metadata:
  domain: infrastructure
  tags:
  - ansible
  - tasks
  - documentation
  - comments
  - i18n
  ai_hints:
    category: guide
---

# When to use this skill
- You have change ansible tasks
- You need write documentation for ansible tasks

# How to use it
1. All steps of task must have comments in English and Russian and describe what is happening in this step. English comments could be skip if the step is self-describing, but Russian comments are required for all steps.
2. All parameters which is used in tasks/*.yml must have comments in English and Russian describing what this parameter is for. And they must be documented in the same way. You can use [template](./templates/readme-template.md.j2) for this.
