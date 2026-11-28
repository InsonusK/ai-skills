---
name: client-and-service-sibling-projects
description: Physical .NET projects for API clients and their adapting services
problem: API clients live beneath Infrastructure and adapting services in Application, coupling handlers to client contracts through the implementation assembly.
decision: Put clients under src/Client and concrete services under src/Service/ApiServiceName/ApiServiceName.Service; retain consumer-owned service interfaces and let Host wire both sides.
status: accepted
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The logical client/service boundaries need physical .NET ownership. Moving service
interfaces away from their consumers would break the existing port ownership,
while leaving adaptation in Application makes its client references transitive
requirements of every handler in that assembly.

# Selected variant
[[#Separate concrete service project]]

Owner selected this physical layout. Project names are refined by
[external-system-service-and-api-client-names](./external-system-service-and-api-client-names.md);
ApiServiceName identifies the external system, independently of the consumer module.

# Searched variants

## Separate concrete service project

**Selected.**

### Description
Client contracts and implementations live under src/Client. Concrete integration
services and their own tests live under src/Service/{ApiServiceName}/{ApiServiceName}.Service and
its adjacent .Tests project. IRemotePlanner stays in Todo.Application/Services,
its consumer's assembly. Service references Application for that interface, Domain
for its models, and the client abstractions it adapts. Application references
neither concrete Service nor client projects. Host references and wires both.
The reference to consumer contracts is dependency inversion, not a permitted call
to handlers. Source namespaces are {service-namespace}.Client and .Service.{ApiServiceName}.

### Benefits
- Client and adapting service have explicit sibling ownership.
- Consumer port placement and service-only Domain Model mapping remain intact.
- Service tests do not belong to the command assembly.

### Costs
- Another project and test project per external-system adaptation boundary.
- Service references the consumer assembly solely for its declared service ports;
  architectural checks must prevent accidental handler use.

## Keep concrete services in Application

### Description
Move clients but retain adaptation and its tests inside the Application project.

### Benefits
- Fewer projects and unchanged service files.

### Costs
- Application keeps client dependencies to host integration implementation.
- The physical tree hides the separate adapting-service boundary.
