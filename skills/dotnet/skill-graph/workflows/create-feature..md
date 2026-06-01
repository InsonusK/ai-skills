---
name: create-feature
description: Analyze and implement a new backend feature using developing skills.
metadata:
    domain: skill
    type: workflow
    tags:
        - dotnet
        - backend
    triggers:
        - user requests new feature
        - user requests business logic implementation
        - user requests new API
        - user requests workflow/process implementation
---

# Goal
Analyze business requirements and orchestrate correct implementation flow for a new backend feature.

# Input data

## Required
- Feature/business requirement description
- Expected business behavior
- Affected entities/modules if known

## Optional
- API contract examples
- UI flow
- Existing related endpoints
- Existing related entities
- Performance requirements
- Security requirements
- Event integration requirements

# Rules
- Always analyze business workflow before implementation.
- Always identify module ownership before creating entities or APIs.
- Always check cross-module interactions against module-boundaries skill.
- Prefer extending existing aggregates/modules over creating new ones.
- Do not immediately generate code without analysis.
- Do not create new modules without justification.

# Work steps
## 1. Analyze feature goal
Determine:
- what business problem is solved;
- what business state changes;
- what business invariants exist;
- whether feature is CRUD, workflow or process orchestration.
Output:
- short business summary.

## 2. Identify affected modules
Determine:
- which module owns feature logic;
- which modules are read dependencies;
- which modules are affected by state changes.
Output:
- module dependency map.

## 3. Identify aggregates and entities
Determine:
- aggregate root;
- related entities;
- required state transitions;
- ownership boundaries.
Output:
- aggregate change plan.

## 4. Analyze consistency requirements
Determine:
- whether operation requires transaction;
- whether reservation/process flow is required;
- whether integration events are required;
- whether outbox publishing is required.
Output:
- consistency strategy.

## 6. Select downstream skills
Determine which implementation skills are required.
1. Does new application operation created?
    Use [create new operation](???)
2. Does need new API
    Use [create new api](???)
3. Always need:
    - []
Examples:

cqrs-command
cqrs-query
validation
domain-model
specification-pattern
integration-events
testing-strategy

Output:

ordered implementation skill list.


# Check list
{{ Check list what agent must done and which artifacts create while using skill. Check list is using to validate that agent follow the skill }}