---
description: Isolated npm install of the pinned living-doc renderers, identical in every stack
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing
  - element/tools-livingdoc-package-json
---

# Goals
- Pin the living-doc renderers once, for every stack, in an npm install that never touches the project's own dependency manifest.

# Implementation changes
`tools/livingdoc/package.json`:
```json
{
  "name": "livingdoc",
  "private": true,
  "type": "module",
  "engines": { "node": ">=22" },
  "dependencies": {
    "@cucumber/html-formatter": "24.2.0",
    "@cucumber/messages": "34.2.1",
    "multiple-cucumber-html-reporter": "4.4.2"
  }
}
```

Run `npm install --prefix tools/livingdoc` once and commit the generated `tools/livingdoc/package-lock.json`; `test-kind-unit` installs with `npm ci`. Add `tools/livingdoc/node_modules/` to `.gitignore`.

# Rule changes

## MUST
- Pin every renderer to the exact version above — no `^`/`~` range — and change a version only here, in this file, for every stack at once.
  - Risk: per-stack or floating versions render the same protocol differently across projects.
  - Fix: bump the version in this Implementation file; stacks copy it verbatim.
- Keep this install under `tools/livingdoc/`, never in the project's own `package.json` — TypeScript projects included.
  - Risk: renderer dependencies leak into the application's dependency tree and audit surface.
  - Fix: `npm ci --prefix tools/livingdoc`.

# Check list
- [ ] `tools/livingdoc/package.json` matches this file; `package-lock.json` is committed; `node_modules/` is ignored.
