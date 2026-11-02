---
name: order and page on the selector
description: Where a query's order and page live - inside the specification, on the selector, or as parameters of a read
problem: A specification with a page, given to a count, an update, and a delete, must mean one set of rows for all three, and First or a page with no order answers rows the engine chose. Where do the order and the page go?
decision: A specification picks rows and holds neither; SelectBy refuses one that does. OrderBy on a selector answers a new ordered selector, the only one with First, Take, and Page; Single takes the case of a unique key.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A delete through a specification paged to one row removed all two rows of the specification in a prototype, because the delete ignored the page while a read honoured it. A selector with no order answered `First` with no failure and no warning.

# Selected variant
[[#OrderBy decorates the selector]]
- Chosen by the owner (2026-11-02): the page as a method of its own, the order as a decorator of the selector.

# Searched variants

## OrderBy decorates the selector

**Selected.**

### Description
`OrderBy` answers a new selector of an ordered type, which adds `First`, `Take`, and `Page`; the first selector is unchanged.

### Benefits
- In a language with types, `First` with no order does not compile.
- One specification serves any order, as an API's sort parameter needs.
- `All`, `Count`, `Update`, and `Delete` work on one set of rows by construction.

### Costs
- The order is written by the service, on the columns of the row.
- A lookup by a unique key needs `Single`, which takes no order, beside `First`.
- A stack's specification library is used only for conditions, joins, and projections.

## The order and the page inside the specification

### Description
The form of Ardalis.Specification: `Query.OrderBy(...).Skip(...).Take(...)`.

### Benefits
- One object says everything about a query; the library needs no wrapping.

### Costs
- A count or a delete must decide whether the page applies, and a wrong decision works on other rows.
- Two sort orders mean two specifications.
- `First` with no order is found only at run time.

## The page as a parameter of All

### Description
`All(page, size)`.

### Benefits
- No extra method.

### Costs
- A call of `All` that does not answer all rows; the order stays unsolved.
