# TestBed

Angular’s test environment for creating real components with controlled dependencies.

## Why it exists

Component tests need Angular’s dependency injection, template compilation and rendering lifecycle.

## How it works

Configure the component imports/providers, create a fixture, set inputs, wait for stabilization and inspect its DOM or emitted output.

## How it is structured

The test module configures dependencies; the component fixture exposes the instance, its host element and lifecycle helpers.

## Example

The form specs submit through the real template and assert the normalized URL.

## Sources

[Official documentation](https://angular.dev/guide/testing/components-basics)
