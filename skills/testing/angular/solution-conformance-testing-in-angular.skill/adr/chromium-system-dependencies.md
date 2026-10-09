---
name: chromium-system-dependencies
description: Prepare browser system libraries through the public make init command.
problem: Browser download alone leaves Chromium unable to start on a minimal Linux host.
decision: Include Playwright system dependency installation in initialization.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
  - framework/angular
---

# Problem
`make init` downloaded Chromium but omitted its Linux runtime libraries. Ordinary UI runs failed before assertions with missing libglib; the earlier passing proof depended on a temporary LD_LIBRARY_PATH.

# Selected variant
[Install browser and system dependencies during initialization](#install-browser-and-system-dependencies-during-initialization).

# Searched variants
## Install browser and system dependencies during initialization
**Selected.**

Description: after installing the committed npm lockfile, `make init` calls the pinned local Playwright installer with `install --with-deps chromium`.

Benefits: the public preparation command supplies the browser runtime; ordinary Make testing commands require no private library-path workaround.

Costs: Linux package installation uses root/sudo and the package repositories; unavailable privileges or packages make initialization fail visibly.

## Require preinstalled system libraries or a private library path
Description: download only Chromium and expect another environment preparation step.

Benefits: initialization performs no operating-system package changes.

Costs: a clean host still fails despite successful initialization; consumers need runner-specific knowledge and the reported success cannot be reproduced with the documented commands.

# Consequences
System dependency preparation belongs to initialization, not to individual test kinds. The shared Make testing/report contract and test adapters remain unchanged. The devcontainer already installs these dependencies; this change supplies the same preparation on the host.

The same runtime preparation supplies system fonts. The example’s earlier snapshot came from fallback fonts in the incomplete environment; its typography-only diff was inspected and the baseline refreshed with the installed Liberation Sans font. Screenshot tolerances and normal read-only snapshot behavior remain unchanged.
