---
name: authorization-model
description: How the UI models what a user is allowed to see and do
problem: Should UI-level authorization be modeled as coarse-grained roles or granular permissions
decision: Use granular permissions (e.g. "orders.edit"), with roles, if they exist at all, treated as a backend-side grouping mechanism that resolves to a permission set delivered to the client
---

# Problem

Different parts of the UI (route guards, structural directives hiding/showing elements, feature-level logic) need to check whether the current user is allowed to see or do something. We need to decide whether that check is expressed in terms of coarse roles (`'admin'`, `'manager'`) or granular permissions (`'orders.edit'`, `'orders.delete'`), since this shapes the shape of the auth slice's state, the guard/directive APIs built on top of it, and how much backend coordination is needed to add a new fine-grained restriction later.

# Selected variant

**Selected variant:** [[#Granular permissions]]

The client only ever reasons about permission strings (e.g. `'orders.edit'`), delivered as a flat list from the backend as part of the session. Whether the backend internally groups permissions into roles is a backend concern the client does not need to know about — the client's guards, directives, and any other authorization check are all written in terms of permissions, never role names.

# Searched variants

## Granular permissions

### Description

The auth slice stores a flat set of permission strings for the current user (e.g. `['orders.view', 'orders.edit', 'billing.view']`), delivered by the backend as part of login/session response. Route guards, structural directives, and feature-level checks all test for specific permissions.

### Benefits

- Scales cleanly as the application grows — a new restriction is a new permission string, with no need to redefine what an existing role means or risk changing behavior for every user holding that role
- Matches how enterprise-grade authorization is typically modeled, and composes well with the platform-embeddability architecture: an embeddable app built by a separate team can request exactly the permissions it cares about from `@platform/contracts`, without needing to understand the platform's full role taxonomy
- Keeps the client decoupled from how the backend chooses to compose permissions into roles internally — the backend can restructure its role model without any client-side change, as long as the permission strings it emits stay stable
- Testable in isolation: a guard or directive's behavior is a pure function of "does the permission set contain X," easy to unit test with a fabricated permission list

### Costs

- Requires the backend to already have (or to build) a permission-granular authorization model, which is more upfront design than a simple role field
- A very small application with few, rarely-changing roles pays a bit of extra structure for a flexibility it may not immediately need

## Simple roles

### Description

The auth slice stores a small list of role strings (e.g. `['admin', 'manager']`). Route guards, directives, and feature-level checks test for role membership directly.

### Benefits

- Simplest possible mental model and smallest amount of state/plumbing for a small number of stable roles
- No permission-string taxonomy to design or maintain

### Costs

- Coarse-grained: a new fine-grained restriction ("can edit orders but not delete them") either forces a new role to be created (multiplying the role matrix) or forces a compromise (grant/deny more than intended)
- Every embeddable app onboarded via the platform-embeddability solution would need to understand the platform's specific role names to guard its own UI correctly, coupling it to the platform's internal role taxonomy rather than to a stable, purpose-named permission
- Harder to test in isolation — a role-based check often implicitly encodes "and this role happens to also mean X," which is easy to get subtly wrong as the role list grows
