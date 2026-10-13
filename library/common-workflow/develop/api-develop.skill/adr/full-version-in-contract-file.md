---
name: full-version-in-contract-file
description: Decide which version string a contract file (openapi.json, .proto) carries inside itself, given that the major version already lives in the package/URL.
problem: Should the version inside the contract file be only minor.patch, or the full major.minor.patch?
decision: Record the full major.minor.patch inside the contract file, with its major equal to the package/URL major.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Every contract carries its major version in the package/URL (`orders.v1`, `/v1/`). The contract file itself also needs a version that changes on every edit, so two copies of the file can be compared. The question is whether that in-file version repeats the major or holds only `minor.patch`.

# Selected variant
**Selected variant:** [[#Full major.minor.patch in the file]]
- A copy of the file detached from its URL/package still says which major it is.
- Matches the OpenAPI convention for `info.version` and what tooling expects.

# Searched variants

## Full major.minor.patch in the file

**Selected.**

### Description
`info.version` / the `.proto` version comment holds `1.4.2`; its major must equal the `v1` in the package/URL.

### Benefits
- The file is self-describing: a copy in a consumer's `docs/integration/client/` identifies its major without the URL.
- Standard semver string, as OpenAPI tooling and readers expect.
- Major mismatch between file and URL is a mechanical check.

### Costs
- The major is stored twice and must be kept equal.

## Only minor.patch in the file

### Description
`info.version` holds `4.2`; the major exists only in the package/URL.

### Benefits
- No duplication of the major.

### Costs
- A copied file cannot be matched to its major without the URL/package context.
- Non-standard: readers and tooling read `4.2` as major 4.
