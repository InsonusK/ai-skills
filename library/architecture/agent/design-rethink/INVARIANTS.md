# Feature and VP authoring redesign

Review contract, 2026-11-26. Status: awaiting owner review before bulk authoring.

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
8. The responsibility decision does not choose a top-level `src/Client` or `src/Service`
   layout, shared cross-module entity ownership, or a configurable replacement for
   API-specific code. Track those as separate open questions.
9. Human documentation links authoritative skills/ADRs and gives each assertion a
   status: current, owner-decided pending implementation, planned, or unresolved.
   Skills contain executable instructions; documentation contains catalog navigation
   and pipeline sequencing. Do not copy the roadmap into multiple mutable tables.
10. Reusable authoring procedures belong in `library/architecture/design/`, separate
    from application specs and stack realizations; they apply skill-design/content/tags.
    No library file links repository-only skills. Update local layout guidance to
    permit this design folder before adding procedures there.

## Waves and artifacts

| Wave | Artifact | Classification | Acceptance |
|---|---|---|---|
| 0, this PR | This contract and catalog overview sample | New | Owner can review model, scope and client delta |
| 0, this PR | `check.py` | New | Links, current-spec coverage and dependency row types checked |
| 1 | Feature map and VP map under `docs/`, typed dependency view | New | Every current spec and every roadmap VP accounted for; no invented realizations |
| 1 | Decision index and README navigation | New / modify | Existing ADRs linked, open questions distinguished, roadmap has one owner |
| 2 | `architecture-feature-add` skill | New | Capability, baseline classification, evidence, VP reuse, map update and verification |
| 2 | `architecture-vp-add` skill | New | Admission question, independence/alternatives, dependencies, status and realization plan |
| 2 | `architecture-vp-describe` skill and template | New | Purpose, guarantees, attach contract, scenarios, diagrams, settings, errors and stack evidence |
| 2 | Authoring layout ADR and local layout extension | New / modify | Exportable procedures separated from repository-specific placement rules |
| Follow-up | `api-client` boundary ADR and shared client guidance | Modify | New DTO boundary replaces conflicting rules and examples explicitly |
| Follow-up | HTTP/gRPC/retry specs and Go/.NET/Python realizations | Modify | Client returns DTO; service owns mapping; retry remains a client middleware |
| Follow-up | Client backlog card and affected documentation | Modify | Accepted responsibility decision distinguished from remaining layout decisions |

Each wave runs the checker, receives a separate conformance audit, and is committed
separately. Executable examples affected by the follow-up must pass their own checks;
documentation checks alone do not validate changed client behavior.

## Decisions log

- Accepted by owner: service adapts API and maps DTOs; client calls API, owns retry
  execution and returns API DTOs (2026-11-26).
- Derived from current catalog: keep the single base and attachable VP model.
- Proposed: keep feature admission, VP admission and VP description as three skills;
  do not fold implementation into admission or require all stacks to exist at once.
- Proposed: reusable authoring skills in `library/architecture/design/`; keep the old
  design pipeline available to its existing consumers.
- Open: the public DTO for a gRPC client may be a generated provider message or an
  API-shaped client-owned DTO. Decide before changing the generated-message rule;
  neither choice permits returning consumer domain entities from the client.
- Open: cross-module service/model ownership and physical Client/Service directories
  remain separate from this accepted responsibility split.

## Status

Wave 0: sample and review contract prepared. Waves 1–2 have not started. The executable client migration is a separately scoped
follow-up; the requested documentation and authoring skills do not depend on
finishing that migration.
The checker validates the sample's structure and references, not business behavior.

## Separate audit pass

Reviewed the sample against the contract: corrected unit-of-work dependency to
the shared DB abstraction, added the external-id dependency, limited wave-0 check
claims to actual checks, and separated executable migration from this task.
The review gate remains pending.
