---
name: browser-spec-discovery
description: Discover all existing browser spec suffixes under spec folders.
problem: How does browser discovery include the existing catalog layers without changing their assertion rules?
decision: Discover all existing browser spec suffixes under spec folders.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
How does browser discovery include the existing catalog layers without changing their assertion rules?

# Selected variant
[All browser suffixes in the UI kind](#all-browser-suffixes-in-the-ui-kind)

# Searched variants
## All browser suffixes in the UI kind
**Selected.**

### Description
The inherited Playwright template matches *.ui.spec.ts, *.visual.spec.ts, *.style-snapshot.spec.ts and *.a11y.spec.ts under spec/. The catalog keeps its visual/style/accessibility rules; this testing solution only discovers and runs them. All refinements use the same discovery.

### Benefits
- An otherwise green UI smoke run cannot silently omit existing catalog browser suites.

### Costs
- More applicable browser specs can increase runtime; their dependency setup remains the project responsibility.

## Only UI suffix
### Description
Keep the original *.ui.spec.ts-only matcher.

### Benefits
- Minimal discovery matching the original application example.

### Costs
- Existing catalog suites are silently absent unless every consuming agent notices and patches configuration.
