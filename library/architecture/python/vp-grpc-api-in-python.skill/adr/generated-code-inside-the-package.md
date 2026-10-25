---
name: generated code inside the package
description: Where the Python gRPC variation point puts the code generated from the proto contract, and how its modules import each other
problem: protoc writes Python modules whose imports follow the path of the proto file - todo_pb2_grpc.py imports todo_pb2 by that path - so the contract's place in the project tree (docs/integration/api/{contract-name}/) does not give an importable path; where does the generated code go, and how do its imports resolve?
decision: make proto-gen maps each contract folder to the import path of its generated package (-I{package}/gen/api/{contract-name}=docs/integration/api/{contract-name}/), so protoc writes src/{package}/gen/api/{contract-name}/ with imports of {package}.gen.api.{contract-name}; the modules, their type stubs, and the __init__.py of each folder are committed.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go generates into `gen/api/{contract-name}/` beside the module, imported by its module path. In Python a generated module is imported by the path protoc saw the proto file under: compiled as `todo.proto` from its folder, `todo_pb2_grpc.py` holds `import todo_pb2`, which resolves only when that folder is itself on `sys.path`.

# Selected variant
[[#Mapped to the package's path]]

# Searched variants

## Mapped to the package's path

**Selected.**

### Description
protoc's include flag maps a virtual path to a folder: `-Itodo/gen/api/todo=docs/integration/api/todo/`. The proto file is compiled as `todo/gen/api/todo/todo.proto`, so the generated code lands in `src/todo/gen/api/todo/` and imports `from todo.gen.api.todo import todo_pb2`. `make proto-gen` does this for every contract folder and touches the `__init__.py` of each package.

### Benefits
- The generated code is an ordinary subpackage: installed with the package, type-checked by mypy, imported by its full name like any other module.
- No edit of generated files; regenerating leaves no diff when the contract did not change.

### Costs
- The proto file's name in the descriptor - what reflection reports - is `todo/gen/api/todo/todo.proto`, not the contract's path; the package (`todo.v1`) and the services are unchanged.

## A top-level package of its own

### Description
Generate into `src/todo_api/` and install it beside the application's package.

### Benefits
- The proto file keeps a short name.

### Costs
- A second top-level package per contract, and still an import path chosen by the protoc call.

## Rewrite the imports after generation

### Description
Generate as `todo.proto`, then fix `import todo_pb2` with sed or a tool (`protoletariat`).

### Benefits
- Common in Python projects.

### Costs
- One more tool or script that edits generated files; a new kind of import in a later protoc version breaks it.
