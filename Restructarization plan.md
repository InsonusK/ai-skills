### Complete restructuring plan

#### Final skill tree target

```
/Architecture
  /Solutions
    command-handling.solution.skill
    domain-events.solution.skill
    guid-resolving.solution.skill
    concurrency-control.solution.skill
    cross-module-communication.solution.skill
  backend-project-structure.skill

/Module
  module-layer.skill
  /Api
    module-api.skill
    /Components
      api-controller.skill
      api-minimal-endpoint.skill
  /Application
    module-application.skill
    /Components
      feature-command-handler.skill
      feature-query-handler.skill
      feature-validator.skill
      event-handler.skill
      ardalis-specification.skill
      repository.skill
  /Domain
    module-domain.skill
    /Components
      entity.skill
      entity-behavior.skill
      entity-concurrency.skill
      external-created-entity.skill
      value-object.skill
      domain-rule.skill
      domain-service.skill
      domain-event.skill
      ef-configuration.skill

/App
  /Host
    app-host.skill
  /Queries
    app-queries.skill
  /Infrastructure
    app-infrastructure.skill
    /Components
      outbox.skill
      unit-of-work.skill

/Shared
  shared-layer.skill

/BuildingBlocks
  building-blocks.skill
```

---

#### Phase 1 — Root structure + Module layer

**Input:** `backend-project-structure` **Output:**

- `/Architecture/backend-project-structure.skill` — stripped to solution layout + dependency graph + links to layer skills only
- `/Module/module-layer.skill` — what a module is, its 4 projects, inter-module dependency rules

---

#### Phase 2 — Domain layer

**Input:** `entity-pattern`, `entity-behavior`, `entity-concurrency-pattern`, `external-created-entity`, `value-object-pattern`, `domain-rule-pattern`, `domain-service`, `domain-event-pattern`, `domain-configuration-pattern` **Output:**

- `/Module/Domain/module-domain.skill` — Domain project boundary, structure, rules, what belongs here
- `/Module/Domain/Components/entity.skill`
- `/Module/Domain/Components/entity-behavior.skill`
- `/Module/Domain/Components/entity-concurrency.skill`
- `/Module/Domain/Components/external-created-entity.skill`
- `/Module/Domain/Components/value-object.skill`
- `/Module/Domain/Components/domain-rule.skill`
- `/Module/Domain/Components/domain-service.skill`
- `/Module/Domain/Components/domain-event.skill`
- `/Module/Domain/Components/ef-configuration.skill`

---

#### Phase 3 — Application layer

**Input:** `module-application`, `command-handler-pattern`, `query-handler-pattern`, `ardalis-specification-pattern`, `repository-pattern`, `domain-event-handler-pattern` **Output:**

- `/Module/Application/module-application.skill` — Application project boundary, structure, DI registration rules
- `/Module/Application/Components/feature-command-handler.skill`
- `/Module/Application/Components/feature-query-handler.skill`
- `/Module/Application/Components/feature-validator.skill`
- `/Module/Application/Components/event-handler.skill`
- `/Module/Application/Components/ardalis-specification.skill`
- `/Module/Application/Components/repository.skill`

---

#### Phase 4 — API layer

**Input:** `api-structure` **Output:**

- `/Module/Api/module-api.skill` — Api project boundary, structure, rules
- `/Module/Api/Components/api-controller.skill`
- `/Module/Api/Components/api-minimal-endpoint.skill`

---

#### Phase 5 — App layer

**Input:** `backend-project-structure` (App sections), `outbox-pattern`, `repository-pattern` (UnitOfWork parts) **Output:**

- `/App/Host/app-host.skill` — composition root, DI registration, pipeline order, module wiring
- `/App/Queries/app-queries.skill` — cross-module reads, DbContext access, when to use vs module query
- `/App/Infrastructure/app-infrastructure.skill` — persistence, EF, repository implementations, outbox
- `/App/Infrastructure/Components/outbox.skill`
- `/App/Infrastructure/Components/unit-of-work.skill`

---

#### Phase 6 — Shared and BuildingBlocks

**Input:** `backend-project-structure` (Shared + BuildingBlocks sections) **Output:**

- `/Shared/shared-layer.skill` — Result types, Exceptions, base types, what belongs here vs BuildingBlocks
- `/BuildingBlocks/building-blocks.skill` — pipeline behaviors, spec base, outbox contracts, what belongs here

---

#### Phase 7 — Solution skills

**Input:** `cqrs-architecture`, `domain-event-architecture`, `async-external-creation`, `guid-resolving-pipeline`, `concurrency-control-pattern`, `cross-module-interaction` **Output:**

- `/Architecture/Solutions/command-handling.solution.skill`
- `/Architecture/Solutions/domain-events.solution.skill`
- `/Architecture/Solutions/guid-resolving.solution.skill`
- `/Architecture/Solutions/concurrency-control.solution.skill`
- `/Architecture/Solutions/cross-module-communication.solution.skill`

---

#### Summary table

|Phase|Input skills|New skills created|Skills refactored|
|---|---|---|---|
|1|1|1|1|
|2|9|10|9|
|3|6|7|6|
|4|1|3|1|
|5|2|5|2|
|6|1|2|1|
|7|6|5|6|

**Total: 21 existing skills → 33 focused skills in a navigable tree**