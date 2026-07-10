---
description: Structural directive that shows/hides UI elements based on a required permission, sharing the same permission model as route guards
project_name: shared-auth-ui
name: has-permission
element_kind: directive
change_kind: create
---

# Goals

- Let any template conditionally render UI (buttons, menu items, sections) based on the current user's permissions, without each component re-implementing its own permission check

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------------- | -------------------- | --------- |
| Structural directive | HasPermissionDirective | HasPermissionDirective | has-permission.directive.ts | has-permission.directive.ts |

# Implementation changes

```typescript
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

```typescript
<button *hasPermission="'orders.delete'" (click)="delete()">Delete order</button>
```

# Rule changes

## MUST
- The directive MUST check a permission string, never a role name, consistent with [[../[[skills/angular/architecture/solutions/solution-authentication.skill/adr/authorization-model]]Hiding an element with this directive MUST NOT be treated as a substitute for a server-side authorization check — it is a UI convenience only; the corresponding action MUST still be authorized on the backend.

# Anti-patterns

- **Relying on `*hasPermission` alone to protect a destructive action, with no server-side check**
  - Consequence: a user could still trigger the action by calling the API directly, bypassing the UI entirely
  - Instead: treat this directive purely as UI polish; the backend remains the actual authorization boundary

# Check list

- [ ] Every use of `*hasPermission` corresponds to an action that is also authorized server-side

# Unittest TestCases

- [ ] WHEN the current user's permissions include the required permission THEN
  - [ ] the embedded view is rendered
- [ ] WHEN the current user's permissions do not include the required permission THEN
  - [ ] the embedded view is not rendered (and is cleared if previously rendered)
