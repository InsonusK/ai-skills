# name

cqrs-mediatr

# description

Defines CQRS and MediatR conventions.

# content

## Commands

Commands modify state.

Commands:

* implement IRequest<Result> or IRequest<T>
* must be immutable
* must use validation

## Queries

Queries never modify state.

Queries:

* implement IRequest<T>
* must avoid side effects

## Handlers

Handlers:

* contain application orchestration logic
* must remain thin
* must not contain HTTP concerns

Avoid:

* fat handlers
* business logic duplication
* direct controller logic inside handlers

## Validation

Use FluentValidation.

Every command must have a validator.

## Transactions

Transactions must be handled through pipeline behaviors.

Handlers must not manually manage transactions unless required.
