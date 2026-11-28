# Feature and VP authoring redesign

Review contract, 2026-11-26. Status: wave 1 shared contracts prepared; waves 2–4 pending.

## Outcome

Document the existing and planned feature/VP catalog, its dependency graph, and
accepted decisions; provide three independently triggered agent procedures:
adding a feature, admitting a VP, and describing a VP skill.

The first sample is [the catalog overview](../../docs/catalog.md). It distinguishes
existing contracts from the owner's new client/service decision.

## Invariants

1. The target is the existing `library/architecture` model: one base plateau with
   attachable VPs. Do not enumerate a plateau for each combination. Old consumers
   of `common-workflow/architecture/design` remain supported until migrated.
2. A feature is an application capability; a VP records a choice and the guarantees
   of its realization. Neither a Gherkin `.feature` file nor a stack implementation
   is automatically a new catalog feature or VP.
3. The feature map records capability, common/variable classification, application
   types, status, source, and realizing VP(s). The VP map records question,
   alternatives or optional presence, constraints, typed dependencies, spec and
   per-stack realization status. Planned rows do not require fictitious skill links.
4. Use existing `vp-*` names as stable identifiers. Introduce no second numeric ID
   scheme inherited from the old catalogs. Record retirements and renames explicitly.
5. A dependency is a directed graph edge, not necessarily a tree edge: distinguish
   `requires`, `requires-any`, `example-built-on`, and `shares-contract`. A code link,
   an example's chosen API, or two VPs sharing readiness does not establish a
   mandatory application dependency. Planned edges state their evidence and uncertainty.
6. Client calls the provider API, returns API DTOs and technical outcomes, and runs
   transport policies through its pipeline. Service adapts API operations to its
   consumer and maps DTOs into that consumer's entities/models through domain APIs.
   Commands consume the service contract. Retry remains an optional client VP;
   placing responsibility in client does not make retry mandatory.
7. Record the client boundary as a new ADR in the owning `api-client` skill and
   explicitly supersede the conflicting return-value decision. Keep historical ADRs.
   Update HTTP/gRPC specifications, shared scenarios, templates, examples and diagrams
   together. A partial migration must be clearly marked and cannot claim conformance.
8. `domain/`, `infrastructure/`, `client/` and `service/` are sibling logical
   boundaries; client is no longer a child of infrastructure (owner decision).
   Physical stack layouts must express these boundaries using that stack's package,
   module or project conventions; document exact paths before migration.
   Service normally represents one external system and may use several clients of
   that system. Large systems may have several logically separated services.
   Each client has one owning service by default; sharing between decomposed
   services of that same external system is an exceptional, justified dependency
   to avoid where possible, not permission to share clients across unrelated systems.
   Cross-module ownership of consumer models remains explicit.
9. Human documentation links authoritative skills/ADRs and gives each assertion a
   status: current, owner-decided pending implementation, planned, or unresolved.
   Skills contain executable instructions; documentation contains catalog navigation
   and pipeline sequencing. Do not copy the roadmap into multiple mutable tables.
10. Reusable authoring procedures belong in `library/architecture/design/`, separate
    from application specs and stack realizations; they apply skill-design/content/tags.
    No library file links repository-only skills. Update local layout guidance to
    permit this design folder before adding procedures there.

11. Keep app-layers limited to responsibilities, dependency directions, logical
    boundaries and interfaces. API, command/factory and domain implementation rules
    belong to app-api-layer, app-command-layer and app-domain-layer. The providing
    mechanism owns an integration service's implementation; api-client owns its
    API-adapting service. Do not introduce app-service-layer (owner accepted).
    The layer table links existing implementation contracts for navigation; the
    owner accepts that these links load their transitive dependencies.

12. Consumer skills and templates contain application contracts, not catalog
    migration waves or implementation progress. Keep this status in agent/project
    tracking; preserve real capability limits and project configuration inputs.

## Branch and review flow

Implement each wave on `rethink-architecture-design-waves`, based on
`rethink-architecture-design`, and submit its PR into `rethink-architecture-design`.
The owner reviews and merges each wave. The agent never merges branches, enables
auto-merge, or merges PRs. After all waves, the owner merges the existing parent
PR from `rethink-architecture-design` into `develop`.

After each owner merge, update the wave branch from the reviewed parent before
starting the next wave; keep each PR diff limited to its wave. The first wave-branch
PR contains this plan refinement only, so the new scope is reviewable before implementation.

## Waves and artifacts

| Wave | Artifact | Classification | Acceptance |
|---|---|---|---|
| 0 | Contract, sample and `check.py` | New / refine | Reviewed model, responsibilities, scope and checks |
| 1, first implementation | `api-client`, `app-layers`, their owning ADRs and directly linked examples | Modify | Sibling client/service boundaries, API DTO returns, grouping and ownership agree; conflicting old decisions explicitly amended |
| 1 refinement | Three owned layer contracts, relocated ADRs/diagrams and consumer links | New / move / modify | app-layers stays topology-only; API VPs and stack consumers retain implementation rules; no standalone service skill |
| 1 | Dependency inventory for stack layer skills, HTTP/gRPC/retry specs and realizations | New | Every conflicting path, contract and consumer listed with migration wave; interim state marked |
| 2 | Per-language service layout views in `docs/` and base-capability sections | New | One navigable structure per Go/.NET/Python; every element names its owner, feature and VP; alternative paths and planned elements explicit |
| 2 | Stack layer skills, client specs, templates, scenarios and examples | Modify | Layout/DTO contract agrees with wave 1; affected executable checks pass; client backlog reflects settled responsibilities |
| 3 | Feature and VP maps, typed dependency graph, decision index and README navigation | New / modify | All current and planned capabilities accounted for; one roadmap owner; no invented realizations |
| 4 | `architecture-feature-add` | New | Capability, classification, evidence, VP reuse, map and layout updates, verification |
| 4 | `architecture-vp-add` | New | Admission question, independence/alternatives, dependencies, status and realization plan |
| 4 | `architecture-vp-describe` and template | New | Purpose, guarantees, attach contract, scenarios, diagrams, settings, errors, owned paths and stack evidence |
| 4 | Authoring layout ADR and local layout extension | New / modify | Exportable procedures separated from repository-specific placement rules |

Each wave runs its mechanical checks, receives a separate conformance audit,
and is committed separately. Wave 1 updates the shared contracts before architecture
work; its inventory explicitly identifies downstream artifacts awaiting wave 2.
Documentation must not present those examples as already migrated. Executable
examples changed in wave 2 must pass their own checks; a link check is insufficient.

## Client and service contract details

Owner clarifications to encode in wave 1:

- A client may implement all or only part of one Swagger/OpenAPI or proto contract.
  Large contracts may be split into several clients. Group related operations by
  entity responsibility (for example orders), not by contract file size or arbitrary
  controller count; include reads needed to work with that entity, not only writes.
- The normal provider contract is logically cohesive, but the design must cope with
  monolithic contracts. Copy the provider contract unchanged; partial coverage is a
  client operation selection, not deletion of operations from the copied file.
- One client does not become an orchestrator of different external systems. Whether
  one logical entity API spans several provider files needs explicit treatment;
  file boundaries alone must not invent service boundaries.
- A service adapts one external system and may compose several of its clients;
  decompose a large system's service by cohesive responsibility when useful.
- Make one service the normal caller/owner of each client. Record any same-system
  sharing exception and why splitting the clients would be worse. Different services
  still communicate through their application-facing contracts or higher orchestration.
- Preserve technical outcomes, retry safety, cancellation, logging and emulator
  coverage while changing return values and locations. Keep the own-store entities
  form exception explicit rather than forcing all repositories behind API services.

## Complete layout views and base capabilities

Proposed documentation contract for wave 2:

- One layout page per language, with a complete logical tree and exact physical
  package/project paths. Every node lists its owning base capability or VP, status,
  and source skill. Mark fixed/copied code versus application-specific elements.
- Interpret "all VPs" as the full catalog overlay, with alternative branches clearly
  marked, not one supposedly valid executable composition of incompatible choices
  (for example PostgreSQL versus SQLite as replacement own stores).
- Include future VPs as planned paths or explicit unresolved placement; do not pretend
  implementation exists. The authoring procedures must update these views whenever
  an element is added or moved, and mechanically check ownership/path collisions.
- Add base capability sections such as `base-client`, referring to `api-client`, and
  the corresponding service, infrastructure, domain and entry/command foundations.
  These sections explain structural ownership and link source rules rather than
  duplicate them. Concrete HTTP/gRPC VPs refine the common client foundation.
- Counterproposal: use `base-client` as a documentation name, not `base-vp-client`,
  until it has a real selectable variation and an attach contract. A shared structural
  foundation alone is not automatically a new VP skill. This naming is proposed,
  not an owner-approved replacement for the requested `base-vp-client` term.

## Decisions log

- Accepted by owner, 2026-11-28: separate API, command and domain implementation
  contracts from app-layers; keep integration-service rules in the providing
  mechanism, with no app-service-layer skill.


- Accepted by owner: service adapts API and maps DTOs; client calls API, owns retry
  execution and returns API DTOs (2026-11-26).
- Derived from current catalog: keep the single base and attachable VP model.
- Proposed: keep feature admission, VP admission and VP description as three skills;
  do not fold implementation into admission or require all stacks to exist at once.
- Proposed: reusable authoring skills in `library/architecture/design/`; keep the old
  design pipeline available to its existing consumers.
- Resolved by owner requirement, 2026-11-28: protocol replacement for an equivalent
  logical API must not change service; client-owned protocol-neutral API DTOs and
  normalized outcomes define the contract; generated DTOs qualify only when
  shared unchanged by the protocol implementations.
- Accepted by owner: sibling domain/infrastructure/client/service boundaries;
  partial contract clients grouped by entity responsibility; a service may compose
  multiple clients of one external system; one owning service per client by default.
- Accepted by owner: work in the wave branch with PRs into the parent; correct
  `api-client` and `app-layers` before architecture changes.
- Proposed: per-stack catalog overlay layouts with explicit alternatives; base
  capability documentation with no automatic proliferation of VP skills.
- Open: physical .NET project placement, cross-module model ownership and shared
  configuration policy when several clients use one provider endpoint.
- Confirmed by owner, 2026-11-28: retain the prohibition on service-to-service
  calls; cross-system composition stays in commands or domain services.

## Status

The owner merged plan refinement PR #311 into the parent branch. Wave 1 revises
the shared contracts, ADRs and directly linked examples; see [wave-1.md](./wave-1.md)
for its audit and validation record. Waves 2–4 remain pending. The parent PR #310
remains a draft until the waves are complete. Architecture client specs and stack
examples are explicitly marked as awaiting migration; this is not a conforming
end-to-end implementation of the new contract yet.

## Separate audit pass

Reviewed this revised plan against the owner's five proposals: both shared skills
are in the first implementation wave; sibling folders and cardinality exceptions
are explicit; per-language layouts distinguish alternatives and planned elements;
base capability terminology remains a visible proposal. Removed the earlier
claim that client migration is outside this task. Runtime validation is assigned
to the wave that changes examples.
