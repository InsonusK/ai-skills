---
name: solution-forms
description: Signal Forms as the default approach for building forms, consistent with the application's Signal-based state management
domain: skill
type: architecture
version: 1.0
tags:
  - skill/architecture/solution
  - angular
  - forms
  - signal-forms
triggers:
  - Building a new form in any feature
  - Reviewing whether a form should use Signal Forms or Reactive Forms
  - Deciding whether a form's field schema should be extracted into its own file
creates: []
extends:
  - "libs/{feature}/feature (form components)"
depends_on:
  - "[[../solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]"
  - "[[../solution-state-management.skill/solution-state-management.skill.md|State management]]"
adr:
  - "[[adr/forms-approach.md|Forms Approach ADR]]"
---

# Goal

- Standardize how new forms are built across the application, using Signal Forms as the default now that it is stable (Angular 22+)
- Keep forms consistent with the rest of the application's Signal-based reactivity established by the "State management" solution, rather than leaving forms as an isolated RxJS-based exception
- Give every form component fine-grained, synchronous access to field-level validity/touched/error state without manual subscription management

# Capabilities

- No manual `valueChanges` subscriptions or manual cleanup for form reactivity
- Fine-grained re-rendering: only the template expressions reading a specific field's Signal update when that field changes
- A single submission pattern (`submitForm()`) that reports success/failure without hand-rolled validity checks before every submit
- Design-system-provided custom controls integrate the same way as built-in controls, via `ControlValueAccessor` compatibility with `formField`

# Core Principles

- New forms are built with Signal Forms (`form()`/`FieldTree`) by default
- Existing Reactive Forms are not force-migrated — Reactive Forms remains supported and is only left in place or touched opportunistically
- Field schema/validators stay inline in the component for simple forms, and are extracted into a `{form-name}.form.ts` file once cross-field logic or validator count makes the component harder to read
- Form submission always goes through `submitForm()`, and any resulting HTTP call goes through the owning feature's data-access facade, never directly through `HttpClient`

# Adr

- [[adr/forms-approach.md|Signal Forms as the default for all new forms instead of Reactive Forms or a complexity-based hybrid]]
  - Selected variant: Signal Forms as the default — chosen because it is now stable (Angular 22), matches the Signal-based reactivity already adopted for state management, and Angular 22 resolves the main historical blocker (ControlValueAccessor compatibility) for control-heavy forms

# Requirements

SOLUTION:
- [[../solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]
  - Form components live inside their owning feature's `libs/{feature}/feature` project, per the existing structure
- [[../solution-state-management.skill/solution-state-management.skill.md|State management]]
  - A form's underlying data Signal follows the same component-local/feature-level tiering already established — a form's data typically lives as component-local state or inside the feature's Signal Store, per that solution's rules

NPM:
- @angular/forms (signals entry point: `@angular/forms/signals`)
  - `form()`, `FieldTree`, built-in validators (`required`, `minDate`, `maxDate`, `validateHttp()`), `when`, `submitForm()`

# Template Skill Mutations

REPOSITORY:
- [[./Implementation/Repository.extend.md|Repository]] - extend - bump minimum Angular version to 22+, add the convention for extracting non-trivial field schemas into `.form.ts` files
PROJECT:
- No project-level (Nx project) changes — this solution operates at the component level, inside existing feature projects

Artifact-level:
- [[./Implementation/FormComponent/{component-name}.component.ts.extend.md|{form-name} (generic pattern)]] - extend - build the form with Signal Forms, applied to any form component in any feature

# Workflow

## Build a new simple form (happy path)

1. A component defines a data Signal for the form's value.
2. `form()` wraps that Signal with inline validators (`required`, etc.), producing a `FieldTree`.
3. Submission calls `submitForm()`, which runs validation and, if valid, invokes the provided async callback (typically calling the feature's data-access facade).
4. On failure, the returned `false` and the field-level `invalid()`/`errors()` signals give the component everything needed to show validation feedback — no manual error-state bookkeeping.

## Build a non-trivial form with cross-field validation (happy path)

1. The field schema and validators are extracted into a `{form-name}.form.ts` file once `when`-based cross-field logic or validator count grows past what's comfortable inline.
2. The component imports the schema-building function and applies it to its own data Signal.
3. The rest of the workflow (submission, error display) is identical to the simple case.

![Build a non-trivial form with cross-field validation (happy path)](./diagrams/build-a-non-trivial-form-with-cross-field-validation-happy-p.mmd)

## Existing Reactive Forms form is left untouched (steady state)

1. A form written before this solution was adopted continues to work as-is; nothing forces its migration.
2. If that form is later substantially reworked for unrelated reasons, it is rebuilt with Signal Forms at that point, following this solution's pattern.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[./Implementation/FormComponent/{component-name}.component.ts.extend.md#MUST|{component-name}.component.ts.extend]]

## SHOULD
- [[./Implementation/FormComponent/{component-name}.component.ts.extend.md#SHOULD|{component-name}.component.ts.extend]]

## SHOULD NOT
- [[./Implementation/Repository.extend.md#SHOULD NOT|Repository.extend]]

# Anti-patterns

- [[./Implementation/Repository.extend.md|See Repository.extend.md]] — starting a new form with Reactive Forms out of habit; mass-migrating existing forms for consistency's sake alone.
- [[./Implementation/FormComponent/{component-name}.component.ts.extend.md|See {component-name}.component.ts.extend.md]] — manually subscribing to field changes instead of using the Signal directly; calling `HttpClient` directly from a form's submit handler.

# Check list

- [ ] The workspace runs Angular >= 22
- [ ] Every new form uses `form()`/`FieldTree` from `@angular/forms/signals`
- [ ] No existing Reactive Forms form has been migrated purely for consistency, without another reason to touch it
- [ ] Non-trivial field schemas are extracted into their own `.form.ts` file rather than left inline
- [ ] Every custom design-system control used inside a form implements `ControlValueAccessor`