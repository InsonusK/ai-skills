---
name: chromium-system-dependencies
description: Who installs the system libraries and fonts Chromium needs for the ui kind
problem: Downloading Chromium alone leaves it unable to start on a minimal Linux host, so make init succeeds and the first UI run fails before any assertion
decision: make init installs the browser together with its system dependencies through the pinned Playwright installer
tags:
  - solution/conformance-testing-in-angular
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
  - framework/angular
---

# Problem
On a minimal Linux host a downloaded Chromium does not start: its runtime libraries are missing, and so are the fonts a screenshot comparison depends on. Decide whether the public preparation command provides them or the environment must.

# Selected variant
**Selected variant:** [[#Install browser and system dependencies during initialization]]

# Searched variants

## Install browser and system dependencies during initialization

**Selected.**

### Description
After installing the committed npm lockfile, `make init` calls the pinned local Playwright installer with `install --with-deps chromium`. The preparation belongs to initialization, not to a test kind; the devcontainer image installs the same packages.

### Benefits
- The public preparation command gives a working browser; a test run needs no private library path.
- The same step installs the fonts, so the screenshot baseline is taken and compared with the same typography.

### Costs
- Installing Linux packages needs root or sudo and the package repositories; without them initialization fails, visibly.

## Require preinstalled system libraries

### Description
Download only Chromium and leave its libraries to another preparation step.

### Benefits
- Initialization changes no operating-system package.

### Costs
- A clean host fails after a successful initialization; the caller needs runner-specific knowledge to repair it.
