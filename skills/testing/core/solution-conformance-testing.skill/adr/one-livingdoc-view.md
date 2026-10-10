---
name: one-livingdoc-view
description: One scenario view and one renderer across all four stacks
problem: A separate inventory page duplicates the living doc; Messages pages omit the shared legend and do not prove exclusion reasons are shown
decision: Keep the normalized inventory for checks, convert executed Messages to classic JSON, complete exclusions from the inventory, and render all stacks through the shared classic renderer
tags:
  - solution/conformance-testing
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The owner removed the separate scenario page once the living doc could show every scenario and tag. Classic JSON and Messages still rendered different views, and a single Messages page had no status legend. Excluded pickles do not themselves prove their reasons are visible. This supersedes the rendering choice in [[./livingdoc-renderer-per-protocol.md|the earlier protocol ADR]] and the page output in [[./scenario-report.md|the inventory ADR]]. The caller contract stays unchanged.

# Selected variant
[[#Convert Messages and use one classic renderer]]

# Searched variants

## Convert Messages and use one classic renderer

**Selected.**

### Description
Convert executed Messages test cases to classic JSON with real steps, tags, duration and errors. For both protocols, complete excluded and not-run entries from `result/scenarios.json`. Render all features with `multiple-cucumber-html-reporter`, placing the shared status legend in its footer. `reports/tests/` always opens this living doc. Keep the inventory and tag check, remove the second HTML page.

### Benefits
- The four showcases have the same view and visible status legend.
- Reasons and tags of excluded entries come from the same inventory that checks tags.
- Project manifests and caller targets remain unchanged.

### Costs
- The shared adapter must track the Messages schema and preserve results and step arguments.
- Inventory summaries by category/type disappear with the removed page, as the owner accepted.
- Node is required to render the view; a container without a browser can verify HTML and data, but cannot verify interactions.

## Decorate the official Messages page

### Description
Keep `@cucumber/html-formatter` and add a legend plus missing scenarios around its page.

### Benefits
- The official formatter interprets Messages directly.

### Costs
- Different looks across stacks; wrapping the view does not put exclusions into the same filtering interface.
- Injecting HTML couples the wrapper to a generated bundle's markup.

## Keep the second inventory page

### Description
Keep the old generated table beside both living-doc renderers.

### Benefits
- No Messages adapter; retains category/type summaries.

### Costs
- Repeats the scenario list and contradicts the owner's removal decision.
