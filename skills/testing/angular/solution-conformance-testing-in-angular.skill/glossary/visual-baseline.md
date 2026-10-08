# Visual baseline

A reviewed expected screenshot committed beside a browser spec.

## Why it exists

A pixel comparison needs an independent expected appearance to detect unintended rendering changes.

## How it works

A normal run captures the actual region and compares it with the baseline; a difference or missing baseline fails the assertion.

## How it is structured

The expected image is source-controlled; actual images and diffs are transient report artifacts.

## Example

The form appearance spec compares its main region with the reviewed linkcheck-form.png.

## Sources

[Official documentation](https://playwright.dev/docs/test-snapshots)
