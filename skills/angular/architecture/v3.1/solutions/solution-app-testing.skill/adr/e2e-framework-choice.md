---
name: e2e-framework-choice
description: Choice of end-to-end testing framework
problem: Which framework to use for full end-to-end tests that exercise the running application through a real browser
decision: Use Playwright
tags:
  - solution/app-testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem

End-to-end tests need to drive a real browser against the running application (including, eventually, the platform shell with embeddable apps mounted via federation, per `solution-platform-embeddability`). We need a framework that handles asynchronous UI reliably, supports multiple browser engines, and fits a workspace that will grow to include federated, independently-deployed pieces.

# Selected variant

**Selected variant:** [[#Playwright]]

Playwright is adopted for all end-to-end tests.

# Searched variants

## Playwright

### Description

A multi-browser (Chromium, Firefox, WebKit) end-to-end framework with built-in auto-waiting, network interception, and trace/video capture for debugging failures.

### Benefits

- True multi-browser coverage including WebKit, without needing a separate paid tier — relevant since the platform (per the embeddability solution) will be used across a broad user base whose browser mix is not fully known in advance
- Built-in auto-waiting for elements/network significantly reduces the flaky, manually-tuned waits that plague many e2e suites
- First-class trace viewer and video capture make failures in CI reproducible and debuggable after the fact, rather than requiring the failure to be reproduced locally
- Runs meaningfully faster than comparable alternatives for the same suite, in most published comparisons, due to its architecture (out-of-process browser automation over a fast protocol rather than running inside the browser's own JS context)

### Costs

- Smaller legacy body of tutorials/Stack Overflow answers than the older, more established alternative, though this gap has narrowed considerably
- Team needs to learn Playwright's own API and conventions if previously only familiar with another framework

## Cypress

### Description

A widely-used e2e framework that runs tests inside the browser alongside the application.

### Benefits

- Large, mature ecosystem with extensive existing tutorials and plugins
- Familiar to many engineers already, given its long-standing popularity

### Costs

- The open-source version does not support WebKit-based browsers, limiting real-world browser coverage compared to Playwright
- Running inside the browser's own context (rather than driving it externally) makes certain scenarios — multiple tabs, cross-origin navigation, which the platform-embeddability solution's federated remotes could plausibly involve — more awkward to test than with Playwright's external automation model
- Generally slower per-test than Playwright in most published comparisons
