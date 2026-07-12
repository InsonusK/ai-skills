---
name: plateau-multiuser-app--class-has-permission-directive
description: Structural directive that shows/hides UI elements based on a required permission, sharing the same permission model as route guards — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
artifact_type: directive
version: 20260711230000
tags:
  - skill/template/class
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
---

# Goal

- Let any template conditionally render UI based on the current user's permissions, without each component re-implementing its own permission check

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Checks a permission string, never a role name

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------------- | -------------------- | --------- |
| Structural directive | `HasPermissionDirective` | `HasPermissionDirective` | `has-permission.directive.ts` | `has-permission.directive.ts` |

# Implementation

```typescript
// Skill: class-has-permission-directive
// Plateau: multiuser-app
// Version: 20260711230000

@Directive({ selector: '[hasPermission]' })
export class HasPermissionDirective {
  private readonly permissions = inject(Store).selectSignal(selectPermissions);
  private readonly templateRef = inject(TemplateRef);
  private readonly viewContainerRef = inject(ViewContainerRef);

  @Input() set hasPermission(required: string) {
    if (this.permissions().includes(required)) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();
    }
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Rules

## MUST
- The directive MUST check a permission string, never a role name.
- Hiding an element with this directive MUST NOT be treated as a substitute for a server-side authorization check.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Relying on `*hasPermission` alone to protect a destructive action, with no server-side check**
  - Consequence: a user could still trigger the action by calling the API directly
  - Instead: treat this directive purely as UI polish

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Check list

- [ ] Every use of `*hasPermission` corresponds to an action that is also authorized server-side

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Unittest TestCases

- [ ] WHEN the current user's permissions include the required permission THEN
  - [ ] the embedded view is rendered
- [ ] WHEN the current user's permissions do not include the required permission THEN
  - [ ] the embedded view is not rendered

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]
