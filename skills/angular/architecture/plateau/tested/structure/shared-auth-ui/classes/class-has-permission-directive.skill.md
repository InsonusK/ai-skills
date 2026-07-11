---
name: class-has-permission-directive
description: Structural directive that shows/hides UI elements based on a required permission, sharing the same permission model as route guards
domain: skill
type: template
plateau: tested
artifact_type: directive
version: 20260711170000
tags:
  - skill/template/class
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

# Goal

- Let any template conditionally render UI (buttons, menu items, sections) based on the current user's permissions, without each component re-implementing its own permission check

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The directive checks a permission string, never a role name, consistent with the same authorization model used by route guards
- Hiding an element with this directive is not a substitute for a server-side authorization check — it is a UI convenience only; the corresponding action must still be authorized on the backend

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------------- | -------------------- | --------- |
| Structural directive | `HasPermissionDirective` | `HasPermissionDirective` | `has-permission.directive.ts` | `has-permission.directive.ts` |

# Implementation

```typescript
// Skill: class-has-permission-directive
// Plateau: authenticated
// Version: 20260711170000

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

```html
<button *hasPermission="'orders.delete'" (click)="delete()">Delete order</button>
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Rules

## MUST
- The directive MUST check a permission string, never a role name, consistent with the authorization model.
- Hiding an element with this directive MUST NOT be treated as a substitute for a server-side authorization check — it is a UI convenience only; the corresponding action MUST still be authorized on the backend.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Relying on `*hasPermission` alone to protect a destructive action, with no server-side check**
  - Consequence: a user could still trigger the action by calling the API directly, bypassing the UI entirely
  - Instead: treat this directive purely as UI polish; the backend remains the actual authorization boundary

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Check list

- [ ] Every use of `*hasPermission` corresponds to an action that is also authorized server-side

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Unittest TestCases

- [ ] WHEN the current user's permissions include the required permission THEN
  - [ ] the embedded view is rendered
- [ ] WHEN the current user's permissions do not include the required permission THEN
  - [ ] the embedded view is not rendered (and is cleared if previously rendered)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]
