---
name: repository, selector, and specification
description: How a service reaches the rows of a table in every stack - a store with hand-written methods per table, a repository with one method per action on a specification, or a repository whose SelectBy gives a selector over a specification
problem: Every table needs insert, update, delete, and reads, and a condition is wanted again for a count, an update, or a delete. What is the one contract every stack implements?
decision: A generic repository with Insert, Update, Delete, and SelectBy(specification); SelectBy answers a selector with All, Single, Count, Any, Update, Delete, and an ordered form with First, Take, and Page. A specification is a named class. A table gets a method of its own only for what this cannot express.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The first form of this variation point had one store per table with hand-written methods, and a query hidden inside each method. Such a query cannot be reused: a count of the same rows is a second query, and an update of them a third. Every stack must also answer the same scenarios, so the contract cannot depend on what one stack's library offers.

# Selected variant
[[#A repository whose SelectBy gives a selector]]
- Chosen by the owner (2026-11-02); replaces the store with hand-written methods. The selector is the owner's proposal. `First`/`Take` and `Single` are the owner's naming.

# Searched variants

## A repository whose SelectBy gives a selector

**Selected.**

### Description
`repository.SelectBy(new UnclosedTasks(owner))` answers an object bound to that specification, and the service asks it what it needs.

### Benefits
- The repository stays at four methods; every action on a set of rows is in one object.
- One specification serves a count, then an update, with no repeat in the service.
- A new table needs a row and its mapping.
- The contract is plain methods, so a stack with no such library can implement it.

### Costs
- A selector looks like a snapshot and is not: [a-selector-is-no-snapshot](./a-selector-is-no-snapshot.md).
- `Update(change)` loads the picked rows into memory; a mass update needs a method of its own.
- A join follows the relations between rows; a join with no relation needs a method of its own.
- A service sees rows and specifications: they live beside the interface, in the project a service references.

## A repository with one method per action

### Description
`CountBy(specification)`, `AnyBy(specification)`, `UpdateBy(specification, change)`, and so on on the repository, the form of Ardalis.Specification.

### Benefits
- No second object, and no way to read it as a snapshot.

### Costs
- Eight or more methods on one interface, each repeating the specification argument.
- Order and page stay inside the specification, or become parameters of every read.

## A store with hand-written methods per table

### Description
`Insert(listId, row)` and `Select(listId)` per table, the query inside the method.

### Benefits
- The narrowest interface: a service can do only what the store offers.

### Costs
- A repeated condition is copied, or a helper per stack appears.
- Every table writes its reads, its failures, and its updates again.
- Nothing common for the shared scenarios to run on.

## The query object of the stack given to the service

### Description
A service composes `IQueryable` or a query builder itself.

### Benefits
- No code of ours.

### Costs
- The technique reaches the service, a condition is written wherever it is needed, and no contract is shared by the stacks.
