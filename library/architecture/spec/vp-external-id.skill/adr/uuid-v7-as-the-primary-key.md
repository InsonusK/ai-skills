---
name: uuid v7 as the primary key
description: How a row a caller creates is keyed - the caller's UUID v7 as the primary key, a bigint key beside a unique caller's UUID, or the caller's UUID of any version as the key
problem: A caller outside the service makes the id of a row it creates. Is that id the primary key, and of which form, given that a random UUID is inserted out of order and is twice the size of a bigint?
decision: The caller's UUID v7 is the primary key, and every foreign key to the row is a uuid; an id of another version is refused. A table whose rows the service creates keeps a bigint identity.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The owner's concern (2026-11-04): a UUID is made out of order, so it is inserted out of order, and it is larger than an integer; a `bigint` key with the UUID in a unique column was the old catalog's form (`solution-external-created-entity`).

Measured on PostgreSQL 17.5 with its default settings (`shared_buffers` 128 MB): five million rows inserted in chunks of fifty thousand.

| Form | Time | Last chunk | Table and indexes | Indexes |
|---|---|---|---|---|
| `bigint` key only, no caller's id | 13.8 s | 130 ms | 356 MB | key 107 MB |
| `bigint` key + unique random UUID (v4) | 38.4 s | 469 ms | 631 MB | key 107 MB, UUID 198 MB |
| random UUID (v4) as the key | 30.2 s | 401 ms | 480 MB | key 192 MB |
| UUID v7 as the key | 25.5 s | 251 ms | 438 MB | key 150 MB |

PostgreSQL keeps rows in a heap, not ordered by the key, so a random UUID costs its out-of-order inserts in whichever index holds it; a `bigint` key beside it adds an index and leaves those inserts as they were.

With one million parents and five million children naming their parent:

| Form | Parents | Children | One parent's children by its id | Table and indexes |
|---|---|---|---|---|
| `bigint` key + unique UUID v7, `bigint` foreign key | 6.7 s | 97.9 s | 99 µs | 809 MB |
| UUID v7 key, `uuid` foreign key | 5.0 s | 80.2 s | 83 µs | 678 MB |

The index of the foreign key is 83 MB as `bigint` and 90 MB as `uuid`: PostgreSQL stores a key repeated in an index once.

# Selected variant
[[#The caller's UUID v7 as the key]]
- Chosen by the owner (2026-11-04), from the measurements above.

# Searched variants

## The caller's UUID v7 as the key

**Selected.**

### Description
The key column is `uuid` with no default; the entry point refuses an id that is not version 7; every foreign key to the row is `uuid`.

### Benefits
- One id per row, which the caller and the service both use; no lookup from one id to another.
- Inserts at the end of the index, as with a `bigint`: the smallest and fastest of the forms with a caller's id.

### Costs
- A foreign key takes 16 bytes in each child row instead of 8.
- The id tells when the row was created, to the millisecond; the skill warns the user when that time may be secret.
- A caller must make UUID v7; one that makes v4 is refused.

## A bigint key beside a unique caller's UUID

### Description
The service's `bigint` identity is the key and every foreign key; the caller's UUID is a unique column used to find the row.

### Benefits
- Foreign keys of 8 bytes.

### Costs
- Two indexes per table, the slowest inserts and the largest table of the measured forms.
- Every call names the row by its UUID, so every call finds the `bigint` first; a child the caller creates names its parent's UUID, which the service turns into the `bigint`.

## The caller's UUID of any version as the key

### Description
As the selected form, accepting a UUID v4 too.

### Benefits
- No demand on the caller's library.

### Costs
- A caller that sends v4 brings back the out-of-order inserts: 30.2 s and a key of 192 MB against 25.5 s and 150 MB.
