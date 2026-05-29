# name

testing-strategy

# description

Defines testing conventions.

# content

## Test Types

Use:

* Unit tests
* Integration tests
* Architecture tests

## Unit Tests

Test isolated business logic.

Avoid database dependencies.

## Integration Tests

Test:

* database behavior
* MediatR pipelines
* transactions
* API integration

## Architecture Tests

Validate:

* dependency rules
* naming conventions
* forbidden references

## Rules

Tests must remain deterministic and isolated.
