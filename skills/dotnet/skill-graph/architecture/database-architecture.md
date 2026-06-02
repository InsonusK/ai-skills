---
uid: {{ skill uid }}
status: draft
name: {{ skill-name }}
description: {{ Describe what skill define}}
domain: skill
type: {{ kind of skill declarative | workflow | developing }}
tags:
- {{ tag for skill classification }}
triggers:
- {{ when skill should called }}
---
# Goal
Define rules how work with Database
- DBContext boundary
- Migration strategy
- ownership rules
- cross-module read/write rules

# Input data
{{ What agent must define from user before start working }}
## Required
{{ Requied input data }}

## Optional
{{ Optional input data }}

# Rules
{{ Some rules and constrains or anti goals which must be followed during work}}

# Work steps
{{ what agent must to by this skill. Must container example of results. }}

# Check list
{{ Check list what agent must done and which artifacts create while using skill. Check list is using to validate that agent follow the skill }}
