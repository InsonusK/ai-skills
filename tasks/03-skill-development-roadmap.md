# Дорожная карта проработки недостающих скиллов

Этот файл содержит поэтапный план задач для ИИ-агента. Каждая задача — это проработка одного или нескольких связанных скиллов из [`02-missing-skills-backlog.md`](./02-missing-skills-backlog.md).

Задачи упорядочены по приоритету: сначала закрываем критические пробелы, которые блокируют end-to-end процесс разработки.

## Принципы выполнения

- Перед созданием нового скилла агент должен проверить, что аналогичный скилл еще не существует.
- Каждый новый скилл должен соответствовать [`skill-design.skill`](../../skills/common-workflow/skill-design.skill/skill-design.skill.md).
- Для .NET-скиллов использовать шаблон [`solution-create.skill`](../../skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md) там, где это уместно.
- Скилл должен содержать: `whenToUse`, Goal, Core Principle, Rules, Anti-patterns, Check list.
- Связанные скиллы одного этапа желательно делать в одной ветке, но каждый — в отдельном файле/папке.

---

## Этап 1. Фундамент: прием требований и анализ

**Цель:** дать агенту инструменты для корректного старта любой задачи, независимо от стека.

### Задача 1.1. Создать скилл приема и классификации требований

**Скилл:** `requirement-intake`

**Что сделать:**
1. Создать файл `skills/common-workflow/requirement-intake/requirement-intake.skill.md` (Human Dir).
2. Описать типы требований: feature, bug, tech debt, spike, refactor.
3. Для каждого типа задать минимальный обязательный набор вопросов.
4. Добавить anti-patterns: "сразу писать код без уточнения типа задачи".
5. Добавить check list.

**Критерий готовности:**
- Скилл проходит проверку по `skill-design`.
- Агент может по описанию пользователя определить тип задачи и список недостающих вопросов.

---

### Задача 1.2. Создать скилл формулировки business problem statement

**Скилл:** `business-problem-statement`

**Что сделать:**
1. Создать файл `skills/common-workflow/business-problem-statement/business-problem-statement.skill.md`.
2. Описать шаблон формулировки: [Actor] имеет проблему [Problem], потому что [Cause], что приводит к [Impact]. Решение должно дать [Outcome].
3. Добавить примеры хороших и плохих формулировок.
4. Добавить anti-patterns: "бизнес-проблема = техническое решение".

**Критерий готовности:**
- Скилл позволяет агенту превратить размытое описание в четкую business problem statement.

---

### Задача 1.3. Создать скилл определения границ задачи

**Скилл:** `scope-boundary-definition`

**Что сделать:**
1. Создать файл `skills/common-workflow/scope-boundary-definition/scope-boundary-definition.skill.md`.
2. Описать формат in-scope / out-of-scope.
3. Добавить правила определения смежных систем.
4. Добавить anti-patterns: "scope creep", "безграничная задача".

**Критерий готовности:**
- Агент может четко зафиксировать, что входит в задачу, а что — нет.

---

### Задача 1.4. Создать скилл формулировки acceptance criteria

**Скилл:** `acceptance-criteria-definition`

**Что сделать:**
1. Создать файл `skills/common-workflow/acceptance-criteria-definition/acceptance-criteria-definition.skill.md`.
2. Описать форматы: Given-When-Then и verification checklist.
3. Добавить требование покрытия happy path и основных ошибочных сценариев.
4. Добавить anti-patterns: "непроверяемые критерии".

**Критерий готовности:**
- Агент может сформулировать проверяемые acceptance criteria по описанию задачи.

---

### Задача 1.5. Создать скилл проверки Definition of Ready

**Скилл:** `definition-of-ready-check`

**Что сделать:**
1. Создать файл `skills/common-workflow/definition-of-ready-check/definition-of-ready-check.skill.md`.
2. Описать чек-лист DoR: цель, scope, acceptance criteria, контекст, зависимости, риски.
3. Добавить правила действий при невыполненном DoR.
4. Добавить anti-patterns: "начинать разработку с неполными требованиями".

**Критерий готовности:**
- Агент перед началом разработки проверяет DoR и сообщает о блокерах.

---

## Этап 2. Проектирование

**Цель:** дать агенту инструменты для качественного доменного и технического проектирования.

### Задача 2.1. Создать скилл доменного моделирования

**Скиллы:** `domain-event-storming-lite`, `aggregate-entity-vo-modeling`

**Что сделать:**
1. Создать `skills/common-workflow/domain-event-storming-lite/domain-event-storming-lite.skill.md`.
2. Создать `skills/common-workflow/aggregate-entity-vo-modeling/aggregate-entity-vo-modeling.skill.md`.
3. В первом — выделение событий, команд, акторов.
4. Во втором — агрегаты, сущности, value objects, инварианты.
5. Оба скилла должны ссылаться на существующие .NET solution-скиллы для таксономии.

**Критерий готовности:**
- Агент может построить доменную модель по business problem и acceptance criteria.

---

### Задача 2.2. Создать скилл проектирования API-контрактов

**Скилл:** `api-contract-design`

**Что сделать:**
1. Создать `skills/common-workflow/api-contract-design/api-contract-design.skill.md`.
2. Описать структуру endpoint: method, route, request, response, errors.
3. Добавить правила версионирования и breaking changes.
4. Добавить примеры для REST и gRPC.

**Критерий готовности:**
- Агент может спроектировать API контракты для новой фичи.

---

### Задача 2.3. Создать скиллы проектирования БД и миграций

**Скиллы:** `database-schema-design`, `ef-core-migrations-design`

**Что сделать:**
1. Создать `skills/common-workflow/database-schema-design/database-schema-design.skill.md`.
2. Создать `skills/dotnet/ef-core-migrations-design/ef-core-migrations-design.skill.md`.
3. В первом — ER-диаграмма, типы, связи, индексы.
4. Во втором — создание безопасных миграций EF Core, seed-данные, production safety.

**Критерий готовности:**
- Агент может спроектировать схему БД и корректные миграции для .NET.

---

### Задача 2.4. Создать скилл проектирования интеграций

**Скилл:** `integration-design`

**Что сделать:**
1. Создать `skills/common-workflow/integration-design/integration-design.skill.md`.
2. Описать выбор sync vs async.
3. Описать idempotency, retry, outbox, dead letter.
4. Добавить anti-patterns: "fire-and-forget без обработки ошибок".

**Критерий готовности:**
- Агент может спроектировать интеграцию с внешней системой.

---

## Этап 3. Планирование реализации

**Цель:** научить агента составлять implementation plan и выбирать solution skills.

### Задача 3.1. Доработать скилл планирования реализации

**Скилл:** `implementation-planning`

**Что сделать:**
1. Создать `skills/common-workflow/implementation-planning/implementation-planning.skill.md`.
2. Описать декомпозицию на шаги, зависимости, артефакты, критерии проверки.
3. Связать с существующим `skills/dotnet/workflows/create-feature/templates/Impementation plan.md`.
4. Добавить anti-patterns: "план без проверяемых промежуточных результатов".

**Критерий готовности:**
- Агент составляет пошаговый implementation plan для задачи любого стека.

---

### Задача 3.2. Создать скилл выбора solution-скиллов

**Скилл:** `solution-selection`

**Что сделать:**
1. Создать `skills/common-workflow/solution-selection/solution-selection.skill.md`.
2. Описать алгоритм: задача → паттерн → solution skill.
3. Добавить ссылки на существующие .NET и Angular solution-скиллы.
4. Добавить anti-patterns: "применять скилл, не проверив контекст".

**Критерий готовности:**
- Агент может выбрать минимальный набор solution-скиллов под задачу.

---

## Этап 4. Git и рабочее пространство

**Цель:** дополнить существующий `work-in-git-tree` правилами именования, окружения и коммитов.

### Задача 4.1. Создать скилл git-стратегии

**Скилл:** `branch-naming-strategy`

**Что сделать:**
1. Создать `skills/common-workflow/branch-naming-strategy/branch-naming-strategy.skill.md`.
2. Описать base branch selection: develop для фич, master для hotfix.
3. Описать naming conventions: `feature/`, `bugfix/`, `hotfix/`, `refactor/`.
4. Добавить commit conventions.

**Критерий готовности:**
- Агент корректно выбирает base branch и имя ветки.

---

### Задача 4.2. Создать скилл проверки окружения

**Скилл:** `development-environment-setup`

**Что сделать:**
1. Создать `skills/common-workflow/development-environment-setup/development-environment-setup.skill.md`.
2. Описать проверку SDK, зависимостей, доступа к БД.
3. Добавить правила: не устанавливать ПО без разрешения, сообщать о проблемах.

**Критерий готовности:**
- Агент проверяет окружение перед разработкой.

---

## Этап 5. Безопасность и observability backend

**Цель:** закрыть критические пробелы в .NET backend.

### Задача 5.1. Создать скилл аутентификации и авторизации .NET

**Скилл:** `dotnet-authentication-authorization`

**Что сделать:**
1. Создать `skills/dotnet/architecture/artifacts/solutions/dotnet-authentication-authorization.skill/dotnet-authentication-authorization.skill.md`.
2. Описать JWT, OAuth2/OIDC, API keys, policies, roles, permissions.
3. Добавить implementation templates для middleware, attributes, handlers.
4. Добавить anti-patterns: "хранить secrets в appsettings.json".

**Критерий готовности:**
- Агент может добавить authN/authZ в .NET API.

---

### Задача 5.2. Создать скилл observability .NET

**Скилл:** `dotnet-observability`

**Что сделать:**
1. Создать `skills/dotnet/architecture/artifacts/solutions/dotnet-observability.skill/dotnet-observability.skill.md`.
2. Описать structured logging (Serilog), health checks, metrics, distributed tracing, correlation id.
3. Добавить implementation templates.

**Критерий готовности:**
- Агент может добавить observability в .NET backend.

---

### Задача 5.3. Создать скилл обработки исключений .NET

**Скилл:** `dotnet-exception-handling-strategy`

**Что сделать:**
1. Создать `skills/dotnet/architecture/artifacts/solutions/dotnet-exception-handling-strategy.skill/dotnet-exception-handling-strategy.skill.md`.
2. Описать бизнес-исключения vs системные, ProblemDetails, middleware, логирование.
3. Связать с `solution-mediator-exception-handler`.

**Критерий готовности:**
- Агент реализует единую стратегию обработки исключений.

---

## Этап 6. Code Review и PR

**Цель:** дать агенту процесс подготовки и прохождения code review.

### Задача 6.1. Создать скилл оформления PR

**Скилл:** `pull-request-authoring`

**Что сделать:**
1. Создать `skills/common-workflow/pull-request-authoring/pull-request-authoring.skill.md`.
2. Описать заголовок, описание, чек-листы, связь с задачами.
3. Добавить anti-patterns: "пустое описание PR".

**Критерий готовности:**
- Агент оформляет понятный PR.

---

### Задача 6.2. Создать скилл процесса code review

**Скилл:** `code-review-process`

**Что сделать:**
1. Создать `skills/common-workflow/code-review-process/code-review-process.skill.md`.
2. Описать, на что смотреть: архитектура, тесты, безопасность, производительность, naming.
3. Добавить классификацию комментариев: blocker / suggestion / nitpick.

**Критерий готовности:**
- Агент может провести code review по чек-листу.

---

### Задача 6.3. Создать скилл обработки feedback

**Скилл:** `handling-review-feedback`

**Что сделать:**
1. Создать `skills/common-workflow/handling-review-feedback/handling-review-feedback.skill.md`.
2. Описать процесс: прочитать, исправить или аргументировать, перепроверить.
3. Добавить anti-patterns: "игнорировать замечания".

**Критерий готовности:**
- Агент корректно обрабатывает замечания review.

---

## Этап 7. CI/CD

**Цель:** дополнить существующий PR validation workflow деплоем и релизами.

### Задача 7.1. Создать скилл deploy workflow

**Скилл:** `github-actions-deploy-workflow`

**Что сделать:**
1. Создать `skills/devops/github-actions-deploy-workflow/github-actions-deploy-workflow.skill.md`.
2. Описать deploy в staging/production, environments, approvals, secrets, smoke tests.
3. Добавить anti-patterns: "хардкодить secrets".

**Критерий готовности:**
- Агент может создать deploy workflow.

---

### Задача 7.2. Создать скилл release management

**Скилл:** `release-management`

**Что сделать:**
1. Создать `skills/common-workflow/release-management/release-management.skill.md`.
2. Описать версионирование, changelog, git tags, release notes.
3. Связать с `devops-github-wf-pr-validation` по проверке версии.

**Критерий готовности:**
- Агент может подготовить релиз.

---

## Этап 8. Тестирование (расширение)

**Цель:** дополнить unit-тесты интеграционными и E2E.

### Задача 8.1. Создать скилл интеграционного тестирования .NET

**Скилл:** `dotnet-integration-testing`

**Что сделать:**
1. Создать `skills/dotnet/testing/dotnet-integration-testing/dotnet-integration-testing.skill.md`.
2. Описать test host, Testcontainers, WireMock, shared fixtures.
3. Добавить implementation templates.

**Критерий готовности:**
- Агент может написать интеграционные тесты для .NET.

---

### Задача 8.2. Создать скилл управления тестовыми данными

**Скилл:** `test-data-management`

**Что сделать:**
1. Создать `skills/common-workflow/test-data-management/test-data-management.skill.md`.
2. Описать factories/builders, seeders, cleanup.
3. Добавить anti-patterns: "shared mutable test state".

**Критерий готовности:**
- Агент управляет тестовыми данными во всех уровнях тестов.

---

## Этап 9. Frontend (Angular) — улучшения

**Цель:** закрыть пробелы в Angular-разработке.

### Задача 9.1. Создать скилл design system Angular

**Скилл:** `angular-design-system`

**Что сделать:**
1. Создать `skills/angular/architecture/artifacts/angular-design-system.skill/angular-design-system.skill.md`.
2. Описать токены, компоненты, применение, расширение.

**Критерий готовности:**
- Агент может применять design system в Angular.

---

## Порядок выполнения задач агентом

Рекомендуемый порядок:

1. **Задача 1.1** — `requirement-intake`
2. **Задача 1.2** — `business-problem-statement`
3. **Задача 1.3** — `scope-boundary-definition`
4. **Задача 1.4** — `acceptance-criteria-definition`
5. **Задача 1.5** — `definition-of-ready-check`
6. **Задача 2.1** — `domain-event-storming-lite` + `aggregate-entity-vo-modeling`
7. **Задача 2.2** — `api-contract-design`
8. **Задача 2.3** — `database-schema-design` + `ef-core-migrations-design`
9. **Задача 3.1** — `implementation-planning`
10. **Задача 3.2** — `solution-selection`
11. **Задача 4.1** — `branch-naming-strategy`
12. **Задача 5.1** — `dotnet-authentication-authorization`
13. **Задача 5.2** — `dotnet-observability`
14. **Задача 6.1** — `pull-request-authoring`
15. **Задача 6.2** — `code-review-process`
16. **Задача 7.1** — `github-actions-deploy-workflow`
17. **Задача 7.2** — `release-management`
18. **Задача 8.1** — `dotnet-integration-testing`
19. **Задача 8.2** — `test-data-management`
20. Остальные задачи по мере необходимости.
