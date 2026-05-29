# name

mediatr-behaviors

# description

Defines MediatR pipeline behavior conventions.

# content

## Required Behaviors

The application should support:

* ValidationBehavior
* LoggingBehavior
* TransactionBehavior
* PerformanceBehavior

## Behavior Order

Validation
-> Authorization
-> Logging
-> Transaction
-> Handler

## Rules

Behaviors must remain generic.

Avoid business-specific logic inside behaviors.

## ValidationBehavior

Automatically execute all FluentValidation validators.

Throw validation exceptions consistently.

## TransactionBehavior

Wrap state-changing commands inside database transactions.

Queries must not open transactions.
