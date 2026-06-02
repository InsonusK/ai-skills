---
uid:
name: "{{ skill-name }}"
description: "{{ Describe what skill define }}"
domain: skill
type: "{ kind of skill declarative | workflow | developing }"
tags:
- "tag-for-skill-classification}}"
triggers:
 - "{{ when skill should called }}"
---

# Goal
{{ Specific, measurable outcome. Not "help user" but "generate X file in Y format" }}

## Skill Scope
{{ define what skill should define and what it shouldn't}}

This skill:
- {{ what skill should define}}

This skill does NOT:
- {{ what skill shouldn't define}}

# Input data
{{ What agent must define from user before start working }}
## Required
{{ Requied input data }}

## Optional
{{ Optional input data }}

# Rules
{{ Some rules and constrains must be followed during work}}

# Anti Goals
{{ what shouln't do by the skill }}

# Work steps
{{ what agent must to by this skill. Must container example of results. }}

# Check list
{{ Check list what agent must done and which artifacts create while using skill. Check list is using to validate that agent follow the skill }}