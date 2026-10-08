# Playwright

A browser automation and test runner that drives real web pages.

## Why it exists

UI contracts need the actual browser’s navigation, DOM interactions, rendering and screenshots.

## How it works

Start the application server, open a browser context, navigate, interact with accessible locators and await assertions.

## How it is structured

The configuration owns the server and browser settings; spec files declare flows; reporters keep failures, traces and screenshot comparisons.

## Example

The counter browser test clicks Increment in the served Angular application and checks the visible count.

## Sources

[Official documentation](https://playwright.dev/docs/intro)
