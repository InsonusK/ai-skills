---
name: plateau-persisted-state-monolith--class-has-permission-directive
description: The *hasPermission structural directive in libs/shared/auth-ui — shows/hides UI by permission string (never a role), a UI convenience only — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing the *hasPermission structural directive (VP7)
plateau: persisted-state-monolith
artifact_type: directive
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> `libs/shared/auth-ui/src/lib/has-permission.directive.ts`. Reads `selectPermissions` from the `auth` slice.

# Goal

- Let any template conditionally render UI (buttons, menu items, sections) by the current user's permissions, without each component re-implementing a permission check

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Structural directive; renders the embedded view iff `permissions().includes(required)`, clears it otherwise
- The check is a permission **string**, never a role name
- Hiding an element is a UI convenience — never a substitute for a server-side authorization check

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/authorization-model.md|authorization-model ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Structural directive | `{X}Directive` | `HasPermissionDirective` | `{x}.directive.ts` | `has-permission.directive.ts` |
| Selector | (readable, no prefix) | `[hasPermission]` | — | — |

# Implementation

```typescript
// Skill: class-has-permission-directive
// Plateau: persisted-state-monolith
// Version: 20260903190000

// The `hasPermission` selector (no lib prefix) is the deliberate, readable
// convention mandated by solution-authentication — allow it past the
// angular-eslint directive-selector rule with one inline disable.
@Directive({ selector: '[hasPermission]', standalone: true })
export class HasPermissionDirective {
  readonly hasPermission = input.required<string>();
  private readonly permissions = inject(Store).selectSignal(selectPermissions);
  private readonly tpl = inject(TemplateRef);
  private readonly vc = inject(ViewContainerRef);
  private rendered = false;
  constructor() {
    effect(() => {
      const allowed = this.permissions().includes(this.hasPermission());
      if (allowed && !this.rendered) { this.vc.createEmbeddedView(this.tpl); this.rendered = true; }
      else if (!allowed && this.rendered) { this.vc.clear(); this.rendered = false; }
    });
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]

# Rules

## MUST
- The check must be a permission string, never a role name.
- Hiding an element with this directive must never be treated as a substitute for a server-side authorization check.
- Never apply several plateau templates per class/artifact.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]

# Check list

- [ ] Every use of `*hasPermission` corresponds to an action also authorized server-side
- [ ] The directive reads `selectPermissions`, never a role

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]

# Unittest TestCases

- [ ] WHEN the current permissions include the required permission THEN the embedded view is rendered
- [ ] WHEN they do not THEN the view is not rendered (and cleared if previously rendered)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]
