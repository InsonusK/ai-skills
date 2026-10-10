# jsdom

A DOM implementation for Node.js that provides browser-like document APIs without a real browser.

## Why it exists

Component tests can check markup and events quickly while Angular runs in the unit-test process.

## How it works

The runner supplies a document/window environment; Angular renders the fixture into it and tests query the resulting elements.

## How it is structured

A simulated document contains elements and DOM events; it does not reproduce Chromium’s layout, painting or pixel output.

## Example

The form component tests check output text here; a screenshot comparison belongs to the browser kind.

## Sources

[Official documentation](https://github.com/jsdom/jsdom)
