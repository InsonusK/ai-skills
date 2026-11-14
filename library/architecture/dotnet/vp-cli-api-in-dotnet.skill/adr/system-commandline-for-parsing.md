---
name: System.CommandLine for parsing
description: Which part of the dotnet CLI is provided by a package and which remains the catalog adapter
problem: A copied token parser duplicates command selection and option handling, while the application must keep its own output, logging and error-kind contract.
decision: Use stable System.CommandLine to parse the command tree and options; keep the resource definitions, help rendering, command adapter and run log as copied code.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The CLI needs command selection, required and repeated flags, help, and a
version flag. Its shared contract also fixes JSON/text output, error kinds,
logging and process exit codes, which must not depend on a parser's invocation
pipeline. [System.CommandLine 2.0.0](https://www.nuget.org/packages/System.CommandLine/2.0.0)
is stable; its release status is not a reason to maintain a token parser.

# Selected variant
[[#System.CommandLine with a catalog adapter]]

Chosen by the owner on 2026-11-14.

# Searched variants

## System.CommandLine with a catalog adapter
**Selected.**

### Description
Build a Command/Option tree from the resource definitions and parse the supplied
argument list with System.CommandLine 2.0.14. Required flags and repeated values
use the library's model. Render the catalog help and usage diagnostics without
invoking parser actions; the adapter calls the command, prints the output and
returns its exception to EntryRunner. Disable response files and directives.
Validate unknown raw long-option names so the library cannot consume an unknown
flag as a string value; explicit `--flag=--value` remains valid. Check each
value-option occurrence against the library tokens, since its array arity is
validated across occurrences. Normalize `--flag=` to a separate empty argument
so an explicit empty value is preserved.

### Benefits
- The library owns command selection and option tokenization.
- Shared scenarios continue to define output, logging and exit codes.
- No library invocation handler catches a business failure or ends the process.

### Costs
- The host gains a pinned package dependency.
- Compatibility validation and help rendering remain in the copied adapter.
- Package upgrades require rerunning the parser and shared scenarios.

## Maintain a copied token parser

### Description
Implement command lookup, flag tokenization and repeated values in the copied
pipeline, with no parsing package.

### Benefits
- The host has no additional package dependency.
- Every syntax decision is controlled directly by the catalog.

### Costs
- Every parsing edge case and regression belongs to the catalog.
- Duplicated parsing code expands the fixed pipeline's maintenance scope.
