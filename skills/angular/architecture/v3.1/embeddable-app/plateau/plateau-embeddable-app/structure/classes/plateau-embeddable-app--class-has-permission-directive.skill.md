---
name: plateau-embeddable-app--class-has-permission-directive
description: The remote's *hasPermission structural directive — shows/hides a control by a permission string read from @platform/contracts' SESSION_CONTRACT; same permission-string model as the host, a UX affordance only — embeddable-app plateau
domain: skill
type: template
whenToUse: when creating the remote's src/app/session/has-permission.directive.ts, or gating a control in a remote component
plateau: embeddable-app
artifact_type: component
version: 20260903180000
tags:
  - skill/template/class
  - plateau/embeddable-app
  - stack/typescript
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]]"
---

> `src/app/session/has-permission.directive.ts`. The remote's own copy — deliberately NOT `@org/shared-auth-ui`'s (a monolith lib). It reads the same `SESSION_CONTRACT` singleton the host published.

# Goal

- Gate a control in a remote component on a permission string, using the host's session, with the exact model the host's own `*hasPermission` uses

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Implementation

```typescript
// Skill: class-has-permission-directive
// Plateau: embeddable-app
import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';
import { SESSION_CONTRACT } from '@platform/contracts';

@Directive({ selector: '[hasPermission]' })
export class HasPermissionDirective {
  // eslint-disable-next-line @angular-eslint/directive-selector -- the *hasPermission convention is mandated by solution-session-consumption
  readonly hasPermission = input.required<string>();
  private readonly session = inject(SESSION_CONTRACT);
  private readonly tpl = inject(TemplateRef<unknown>);
  private readonly vcr = inject(ViewContainerRef);

  constructor() {
    effect(() => {
      const allowed =
        this.session.isAuthenticated() && this.session.permissions().includes(this.hasPermission());
      this.vcr.clear();
      if (allowed) this.vcr.createEmbeddedView(this.tpl);
    });
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Rules

## MUST
- Reads the permission set only from `SESSION_CONTRACT` — never a local copy.
- The argument is a permission string, never a role name.
- `*hasPermission` is a UX affordance only — the real enforcement is server-side and, for a whole route, `requirePermission` in `remote.routes.ts`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Check list

- [ ] The directive reads `SESSION_CONTRACT`, not a local session copy
- [ ] The argument is a permission string
- [ ] It is the remote's own directive, not imported from a monolith lib

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Unittest TestCases

- [ ] WHEN the user has the permission THEN `*hasPermission` renders the template; otherwise it clears the view
- [ ] WHEN `isAuthenticated()` is false THEN the gated control is absent regardless of the permission list

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]
