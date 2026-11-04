---
name: version compared by the base repository
description: Where the version a row declares is compared - in the base repository of vp-postgres, or by code this variation point attaches beside it (a plugin, a subclass)
problem: Update and Delete of vp-postgres write a row by its key. A row that changes must also be written only while its version is the one the caller read. Which code puts the version into the write?
decision: The base repository of vp-postgres, in every stack, compares a version a row declares; this variation point only declares it on the rows that change.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The base repositories write a row by its key, and a write that finds no row is already the outcome `RowChanged`. The stacks put the version into that write differently:
- dotnet's EF Core compares a concurrency token itself.
- Go's GORM lets a plugin change every statement.
- Python's `Repository.update` builds an `UPDATE` of every column by the key, so a declared `xmin` would be written into `SET`, which PostgreSQL refuses for a system column.

# Selected variant
[[#The base compares a declared version]]
- Chosen by the owner (2026-11-04).

# Searched variants

## The base compares a declared version

**Selected.**

### Description
`Update` and `Delete` of the base repository, and the `Update` of its selector, put a version the row declares into `WHERE`, leave it out of `SET`, and read the new one back onto the row (`RETURNING xmin`). A row that declares none is written by its key as before.

### Benefits
- One check in one place per stack, which every write path passes through, the selector's too.
- The same shape in every stack; a row of a table that never changes is unaffected.

### Costs
- A change of `vp-postgres`'s fixed files in Go and Python (dotnet's EF Core already compares a declared token).
- The base knows the idea of a version, whether or not the application attaches this variation point.

## Code attached beside the base

### Description
The base stays as it is. Go adds a GORM plugin through `db.Use`. Python adds a repository subclass for versioned rows, plus SQLAlchemy's `version_id_col` for the selector's flush. dotnet uses EF Core's token.

### Benefits
- The base knows nothing of versions; the attach check proves the variation point added files only.

### Costs
- Three mechanisms for one rule, one of them per call path in Python (the subclass for `update`, the mapper for the selector).
- The base repository breaks for a versioned row in Python: an agent who builds it over one gets an error from PostgreSQL, not a check.
