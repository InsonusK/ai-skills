# name

specifications

# description

Defines Ardalis.Specification usage rules.

# content

## Specifications

Specifications encapsulate reusable query logic.

## Rules

Use specifications for:

* reusable filtering
* reusable include graphs
* pagination
* sorting

Avoid:

* trivial one-use specifications
* business workflows inside specifications

## Placement

EF-specific specifications belong to Infrastructure.

Pure business specifications may live in Application or Domain.
