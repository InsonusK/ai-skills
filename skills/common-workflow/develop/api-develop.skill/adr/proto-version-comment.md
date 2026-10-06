---
name: proto-version-comment
description: Decide how a .proto file carries its major.minor.patch version, since protobuf has no standard version field.
problem: Where and in what form does a .proto file record its contract version?
decision: A `// version: {major}.{minor}.{patch}` comment as the first line of the .proto file.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`openapi.json` has `info.version`; protobuf has no built-in equivalent. Without one fixed form, every service marks the version differently (or not at all), and the version cannot be found or checked mechanically.

# Selected variant
**Selected variant:** [[#First-line version comment]]
- Zero build impact and trivially greppable.

# Searched variants

## First-line version comment (selected)

### Description
The first line of every `.proto` file is `// version: 1.4.2`.

### Benefits
- No code generation or imports involved; works with every protoc toolchain.
- Fixed position and prefix make it greppable and checkable by a script.
- Shows up in every diff of the file.

### Costs
- Not available at runtime through reflection.
- Nothing in the compiler enforces it.

## Custom file option

### Description
Define `extend google.protobuf.FileOptions { string api_version = 50000; }` and set `option (api_version) = "1.4.2";`.

### Benefits
- Machine-readable via descriptors at runtime.

### Costs
- Every contract must import a shared extension definition, which consumers must also obtain to compile it.
- An extension field number has to be allocated and coordinated.

## Separate version file

### Description
A `VERSION` file next to the `.proto` in `docs/integration/api/{contract-name}/`.

### Benefits
- One version for a multi-file contract.

### Costs
- A copied `.proto` loses its version; the file alone is no longer a diff signal.
