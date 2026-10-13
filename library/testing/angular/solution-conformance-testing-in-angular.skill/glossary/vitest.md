# Vitest

A JavaScript/TypeScript test runner used by Angular’s native unit-test builder.

## Why it exists

It runs assertions, isolates tests and produces structured results and code coverage.

## How it works

Angular compiles the application and initializes its test environment; Vitest then executes component specs and reports their outcomes.

## How it is structured

Spec files contain tests and assertions; a runner configuration selects the environment and coverage output.

## Example

The component kind requests JSON results and V8 coverage from the Angular-managed run.

## Sources

[Official documentation](https://angular.dev/guide/testing)
