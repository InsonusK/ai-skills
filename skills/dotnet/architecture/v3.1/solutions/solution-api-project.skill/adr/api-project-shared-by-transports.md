---
name: api-project-shared-by-transports
description: Whether REST and gRPC each get their own module project or share one {Module}.Api
problem: v3.1 makes the inbound sync API optional (VP8 HTTP, VP9 gRPC), independently combinable. Something must create {Module}.Api and wire it into App.Host before either transport solution runs — one project, or one per transport?
decision: One {Module}.Api project, created by solution-api-project, with REST in /Controllers and gRPC in /Grpc + /Protos. One partial ApiRegistration class; each transport solution adds its own partial and its endpoints.
tags:
  - solution/api-project
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-http-api-publication` (VP8) and `solution-grpc-integration` (VP9) both `extend {Module}.Api.csproj` and both wire into `App.Host`. In v3 that project was created unconditionally by `solution-sln-structure`. In v3.1 the API is optional, so a new solution must create the project — and it must decide whether the two transports share it.

# Selected variant

**Selected variant:** [[#One {Module}.Api project, transports as folders, shared partial ApiRegistration]]

# Searched variants

## One project per transport ({Module}.Api.Rest, {Module}.Api.Grpc)

### Description
`solution-http-api-publication` creates `{Module}.Api.Rest`, `solution-grpc-integration` creates `{Module}.Api.Grpc`.

### Benefits
- Each transport solution owns a whole project — no shared file.
- A module with only one transport has only one API project.

### Costs
- Two projects with identical reference rules, identical `ISender` usage, identical `Result` mapping helpers — duplicated.
- No shared prerequisite solution; the "create the project" step is duplicated in each transport solution, and a module with both transports has two near-identical projects.
- The v3 catalog and its structure skills assume one `{Module}.Api` — a split ripples into every plateau.

## One {Module}.Api project, transports as folders, shared partial ApiRegistration (selected)

### Description
`solution-api-project` creates `{Module}.Api` and a `partial ApiRegistration` with `AddModuleApi()`/`UseModuleApi()`. `solution-http-api-publication` adds `/Controllers` + a partial with `AddHttpApi()`; `solution-grpc-integration` adds `/Grpc` + `/Protos` + a partial with `AddGrpcApi()`. `Program.cs` calls only the top-level pair.

### Benefits
- One project, one set of reference rules, one place for shared `Result`-mapping helpers.
- A clean shared prerequisite: VP8 and VP9 both `depends_on solution-api-project`.
- Matches the v3 single-`{Module}.Api` assumption, so plateau structure stays close to v3.
- `partial` class means the two transport solutions never edit the same file.

### Costs
- `solution-api-project` is an extra solution in the graph (but a small one).
- The `partial ApiRegistration` convention has to be understood by both transport solutions.
