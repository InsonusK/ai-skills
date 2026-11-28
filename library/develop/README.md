# Application development contracts

Use these stack-independent contracts when designing or implementing application
code. Stack extensions and architecture variation points supply concrete packages,
projects and runtime mechanisms.

| Contract | Responsibility |
|---|---|
| [app-layers](core/app-layers.skill/app-layers.skill.md) | Layer responsibilities, permitted calls, sibling boundaries and interfaces |
| [app-api-layer](core/app-api-layer.skill/app-api-layer.skill.md) | Entry translation, independent command dispatch and outcome mapping |
| [app-command-layer](core/app-command-layer.skill/app-command-layer.skill.md) | Commands, results, factories, nested dispatch and orchestration |
| [app-domain-layer](core/app-domain-layer.skill.md) | Entities, invariants and reusable domain services |
| [api-client](core/api-client.skill/api-client.skill.md) | External API clients and the integration service adapting their DTOs |

Start with the layer map to establish dependencies, then apply the contract of the
layer being implemented. HTTP/gRPC/CLI API variation points apply app-api-layer
before their transport-specific rules. The mechanism providing an integration
service owns its realization rules; no separate app-service-layer skill is needed.
