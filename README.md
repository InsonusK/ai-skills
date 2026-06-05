# AI skills

Skills for agent working

work with [ai-skills-manager](https://github.com/InsonusK/ai-skills-manager)

# Dotnet
## 1. Контекст архитектурных принципов (backend)
### 1.1 Общая архитектурная модель
- **Pragmatic Modular Monolith**  
    Основная архитектура системы.  
    Монолит с четкими модулями (bounded contexts), без преждевременного разбиения на микросервисы.    
- **Future Microservice Readiness (non-invasive)**  
    Модули проектируются как кандидаты на будущие сервисы, но без:
    - разрыва FK        
    - отказа от JOIN        
    - введения distributed patterns без необходимости   

---

### 1.2 Модульность и границы
- **Controlled Module Boundaries**  
    Модули изолированы логически, но не физически.  
    Разрешено:
    - cross-module reads (JOIN, specifications)
    Ограничено:    
    - cross-module writes (только через владельца агрегата)
- **Shared Relational Model**  
    Единая БД для всех модулей.  
    FK между модулями допустимы и используются как часть модели.

---

### 1.3 CQRS модель
- **Behavioral CQRS (не strict CQRS)**    
    - Commands изменяют состояние и могут возвращать результат        
    - Queries не изменяют бизнес-данные        

---

### 1.4 Транзакции и согласованность
- **Transactional Consistency First**  
    Приоритет ACID транзакций внутри монолита.  
    Eventual consistency не используется без необходимости.    
- **Reservation-Based Process Consistency**  
    Для сложных бизнес-процессов используется:    
    - резервирование ресурсов        
    - промежуточные статусы        
    - финализация операции
        
---

### 1.5 Интеграции и события
- **Transactional Outbox Pattern**  
    Integration events сохраняются в БД в одной транзакции с изменениями.    
- **Event Separation Model**    
    - Domain Events → внутри агрегата/модуля        
    - Application Notifications → внутри приложения        
    - Integration Events → внешние системы (Kafka и т.д.)        

---

### 1.6 Application слой (MediatR)
- **MediatR Pipeline Architecture**  
    Поток обработки:  
    API → Command/Query → Pipeline → Handler    
- Pipeline включает:    
    - validation        
    - transaction boundary        
    - logging / cross-cutting concerns        

---

### 1.7 CQRS реализация
- **Commands**    
    - изменяют состояние        
    - возвращают результат (updated state allowed)        
- **Queries**    
    - только чтение        
    - допускается логирование/метрики, но не бизнес-изменения        

---

### 1.8 Domain Model подход
- **Hybrid Domain Model**  
    Комбинация:    
    - rich domain (инварианты внутри entity)        
    - anemic patterns (для orchestration и сложной логики)        
- **Thin Handler Pattern**  
    Handler = orchestration слой, не содержит бизнес-логики.    
- **Focused Domain Services**  
    Используются только для:    
    - cross-aggregate logic        
    - orchestration        
    - сложных вычислений        

---

### 1.9 Validation модель
- **Layered Validation Strategy**    
    - Input validation (DTO / FluentValidation)        
    - Domain validation (invariants)        
    - Business process validation (workflow rules)        

---

### 1.10 Query / Read model

- **Specification Pattern (Ardalis.Specification)**  
    Используется для:
    
    - reusable queries
        
    - cross-module reads
        
    - complex filtering logic
        

---

### 1.11 API архитектура

- **Controller vs Minimal API split**
    
    - Controllers → entity/CRUD operations
        
    - Minimal API → processes, integrations, utilities
        

---

## 2. Контекст работы со Skill системой

---

### 2.1 Общая модель skills

- **Skill System as Execution Graph**  
    Skills не просто документы, а узлы графа исполнения:
    
    - определяют поведение агента
        
    - формируют pipeline выполнения задачи
        

---

### 2.2 Типы skills

- **Declarative Skills**  
    Определяют:
    
    - архитектурные ограничения
        
    - правила системы
        
    - invariants  
        ❗ не выполняют работу, только задают рамки
        
- **Workflow Skills**  
    Определяют:
    
    - последовательность действий
        
    - разбиение задачи
        
    - выбор downstream skills  
        ❗ не содержат реализации
        
- **Developing Skills**  
    Определяют:
    
    - конкретные способы реализации кода
        
    - шаблоны создания объектов (Controller, Command, Handler и т.д.)
        

---

### 2.3 Execution model

- **Feature-first workflow**  
    Любая задача начинается с:
    
    - анализа (workflow skill)
        
    - затем перехода в chain workflow → developing skills
        
- **Skill chaining**  
    Workflow skill:
    
    - не реализует систему
        
    - выбирает следующие skills
        

---

### 2.4 Skill boundaries (ключевой принцип)

- Workflow skills:
    
    - orchestration only
        
    - no business logic definition
        
    - no implementation rules
        
- Developing skills:
    
    - only implementation rules
        
    - no architectural decisions
        
- Declarative skills:
    
    - only constraints and invariants
        
    - no steps or implementation
        

---

### 2.5 Skill linking model

- **Stable ID-based linking**
    
    - primary identifier = GUID or stable id
        
    - human name = mutable
        
    - reference format: `use skill [name](skill:id)`
        

---

### 2.6 Skill design principles

- Skills должны быть:
    
    - компактными
        
    - детерминированными
        
    - без лишнего текста
        
    - строго scoped
        
- Один skill = одна ответственность
    
- Skills не дублируют друг друга
    

---

### 2.7 Workflow design principle

- Workflow skill = orchestration layer
    
- Workflow skill:
    
    - анализирует задачу
        
    - определяет тип изменений
        
    - выбирает downstream skills
        
    - формирует execution plan
        
    - НЕ пишет код
        

---

### 2.8 Dependency model

- Skills имеют:
    
    - `requires` (dependency graph)
        
    - `triggers` (activation conditions)
        
- Dependency resolution:
    
    - based on stable IDs
        
    - not on names
        

---

Если хочешь, следующий шаг логично сделать так:  
собрать **минимальный набор developing skills (5–7 штук)** и от них построить первый полный execution pipeline.

Architecture
├── [[skills/dotnet/skill-graph/architecture/backend-project-structure.skill|backend-project-structure.skill]]        ← solution layout, dependency rules
├── [[skills/dotnet/skill-graph/architecture/cross-module-interaction.skill|cross-module-interaction.skill]]         ← module communication rules
├── [[skills/dotnet/skill-graph/architecture/solution/domain-event-architecture.skill|domain-event-architecture.skill]]        ← event system ADR (entry point)
└── [[skills/dotnet/skill-graph/architecture/cqrs-architecture.skill|cqrs-architecture.skill]]                ← TODO: CQRS decision record

Domain Layer
├── [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|entity-pattern.skill]]                   ← entity types, identity matrix
│   ├── [[skills/dotnet/skill-graph/Domain Layer/entity/entity-behavior.skill|entity-behavior.skill]]              ← invariant enforcement
│   ├── [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]]   ← RowVersion / xmin
│   └── [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill|external-created-entity.skill]]      ← Guid property, unique index
├── [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|value-object-pattern.skill]]             ← immutable VO, single/multi-property
├── [[skills/dotnet/skill-graph/Domain Layer/domain-rule-pattern.skill|domain-rule-pattern.skill]]              ← static predicates, no throw
├── [[skills/dotnet/skill-graph/Domain Layer/domain-service.skill|domain-service.skill]]                   ← pure logic extraction
├── [[skills/dotnet/skill-graph/Domain Layer/domain-event-pattern.skill|domain-event-pattern.skill]]             ← define + raise events on entity
└── [[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill|domain-configuration-pattern.skill]]    ← EF IEntityTypeConfiguration

Application Layer
├── [[skills/dotnet/skill-graph/Application Layer/module-application.skill|module-application.skill]]               ← TODO: CQRS handler structure
├── [[skills/dotnet/skill-graph/Domain Layer/command-handler-pattern.skill|command-handler-pattern.skill]]          ← TODO: load→domain→save pattern
├── [[skills/dotnet/skill-graph/Application Layer/query-handler-pattern.skill|query-handler-pattern.skill]]            ← TODO: single-module vs cross-module
├── [[skills/dotnet/skill-graph/Application Layer/ardalis-specification-pattern.skill|ardalis-specification-pattern.skill]]    ← TODO: simple (Domain) vs complex (App)
├── [[skills/dotnet/skill-graph/Application Layer/repository-pattern.skill|repository-pattern.skill]]               ← TODO: IRepository, IReadRepository, IUnitOfWork
├── [[skills/dotnet/skill-graph/Domain Layer/domain-event-handler-pattern.skill|domain-event-handler-pattern.skill]]     ← INotificationHandler + idempotency
├── [[skills/dotnet/skill-graph/Application Layer/concurrency-control-pattern.skill|concurrency-control-pattern.skill]]      ← TODO: MediatR pipeline Version check
└── [[skills/dotnet/skill-graph/Application Layer/guid-resolving-pipeline.skill|guid-resolving-pipeline.skill]]          ← TODO: Guid→Id before handler runs

Infrastructure Layer
├── [[skills/dotnet/skill-graph/Infrastructure Layer/outbox-pattern.skill|outbox-pattern.skill]]                   ← OutboxMessage, interceptor, dispatcher
└── [[skills/dotnet/skill-graph/Infrastructure Layer/async-external-creation.skill|async-external-creation.skill]]          ← client Guid, 409, GuidResolvingBehavior

API Layer
└── [[skills/dotnet/skill-graph/API Layer/api-structure.skill|api-structure.skill]]                    ← Controllers, Minimal API, response mapping


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