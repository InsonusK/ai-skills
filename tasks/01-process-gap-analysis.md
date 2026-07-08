# Анализ покрытия полного цикла разработки скиллами

## 1. Цель анализа

Понять, какие этапы полного процесса разработки — от момента постановки бизнес-задачи до момента попадания кода в мастер-ветку — уже покрыты существующими скиллами в `skills/`, а какие остаются непроработанными. Результат используется для составления бэклога новых скиллов и плана их проработки.

## 2. Методология

1. Просканирована структура `skills/`.
2. Прочитаны все высокоуровневые `*.skill.md` и `SKILL.md`.
3. Скиллы классифицированы по типу: `workflow`, `solution`, `guideline`, `plateau`.
4. Скиллы сопоставлены с этапами жизненного цикла разработки.
5. Выявлены пробелы — этапы, для которых отсутствуют действующие инструкции для агента.

## 3. Карта процесса разработки

Полный путь от требования до мастера разбит на 9 этапов. Для каждого этапа указаны:
- задачи, которые агент решает на этом этапе;
- существующие скиллы, покрывающие этап;
- пробелы.

### Этап 1. Прием и фиксация бизнес-требований

**Задачи агента:**
- Определить тип требования: новая фича, баг, tech debt, исследование, рефакторинг.
- Собрать минимальный набор входных данных от пользователя.
- Сформулировать бизнес-проблему в формате: что не так, для кого, почему важно, ожидаемый эффект.
- Выделить неявные ограничения: законодательство, SLA, объемы данных, совместимость, доступность.
- Определить границы задачи (scope): что входит, что точно не входит, какие системы затрагиваются.
- Сформулировать критерии приемки (acceptance criteria) в проверяемом виде.
- Проверить Definition of Ready (DoR) — достаточно ли информации для старта разработки.

**Существующие скиллы:**
- `skills/dotnet/workflows/create-feature/create-feature.skill.md` — workflow анализа .NET-фичи, но только для dotnet-бэкенда и без явной проверки DoR.

**Пробелы:**
- Нет кросс-доменного workflow приема требований.
- Нет скилла классификации типов задач.
- Нет скилла формулировки business problem statement.
- Нет скилла определения границ задачи.
- Нет скилла формулировки acceptance criteria.
- Нет скилла проверки Definition of Ready.

---

### Этап 2. Доменный и технический анализ

**Задачи агента:**
- Выделить доменные события, команды и акторов бизнес-процесса.
- Определить агрегаты, сущности, value objects, их границы и инварианты.
- Смоделировать бизнес-процесс: последовательность шагов, состояния, переходы, исключительные пути.
- Спроектировать API-контракты: endpoints, запросы, ответы, коды ошибок, версионирование.
- Спроектировать схему БД: таблицы, связи, индексы, ограничения, миграции, seed-данные.
- Спроектировать интеграции: синхронные/асинхронные, outbox, idempotency, retry, обработка ошибок.
- Оценить технические риски и ограничения.
- Принять и зафиксировать архитектурные решения в ADR при необходимости.

**Существующие скиллы:**
- `skills/dotnet/workflows/create-feature/create-feature.skill.md` — анализ модуля, агрегатов, согласованности.
- `skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md` — создание solution-скиллов.
- `skills/dotnet/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md` — создание plateau.
- `skills/dotnet/architecture/artifacts/plateau/default/plateau-default.skill.md` — готовое .NET backend plateau.
- Большинство `solution-*` скиллов в `skills/dotnet/architecture/artifacts/solutions/` покрывают отдельные архитектурные решения.
- `skills/angular/architecture/artifacts/solution-*.skill/` покрывают frontend-архитектуру.

**Пробелы:**
- Нет универсального скилла выделения доменных событий (event storming-lite).
- Нет скилла моделирования бизнес-процесса и состояний.
- Нет скилла проектирования API-контрактов.
- Нет скилла проектирования схемы БД.
- Нет скилла проектирования миграций EF Core.
- Нет скилла проектирования интеграций и outbox.
- Нет скилла оценки рисков.
- Нет скилла написания ADR.

---

### Этап 3. Планирование реализации и тестирования

**Задачи агента:**
- Декомпозировать задачу на конкретные шаги реализации.
- Определить порядок шагов и зависимости между ними.
- Оценить трудоемкость (хотя бы относительно: S/M/L).
- Выбрать подходящие архитектурные паттерны и solution-скиллы.
- Составить план тестирования: какие тесты, какие сценарии, какие edge cases.
- Определить Definition of Done (DoD) для задачи.
- Сформировать implementation plan в согласованном формате.

**Существующие скиллы:**
- `skills/dotnet/workflows/create-feature/create-feature.skill.md` — создание implementation plan.
- `skills/common-workflow/test/workflow-unittest-testplan.skill/workflow-unittest-testplan.skill.md` — планирование unit-тестов.
- `skills/dotnet/testing/testing-strategy.skill.md` — стратегия тестирования .NET.

**Пробелы:**
- Нет универсального скилла декомпозиции и планирования реализации.
- Нет скилла относительной оценки трудоемкости.
- Нет скилла выбора подходящих solution-скиллов под задачу.
- Нет скилла формирования Definition of Done.
- `skills/dotnet/workflows/create-new-operation.md` — пустой шаблон, требует доработки.

---

### Этап 4. Подготовка рабочего пространства

**Задачи агента:**
- Определить базовую ветку (develop/master) и создать task branch.
- Создать git worktree для изоляции работы.
- Проверить, что окружение настроено: SDK, зависимости, доступ к репозиторию.
- Убедиться, что base branch синхронизирован с origin.

**Существующие скиллы:**
- `skills/common-workflow/work-in-git-tree.skill.md` — работа в git worktree, создание PR.

**Пробелы:**
- Нет скилла выбора базовой ветки и стратегии именования веток.
- Нет скилла проверки и настройки development-окружения.
- Нет скилла commit conventions.

---

### Этап 5. Разработка

**Задачи агента:**
- Реализовать доменную модель (entities, value objects, domain rules).
- Реализовать application layer (handlers, validators, behaviors).
- Реализовать инфраструктуру (репозитории, EF конфигурации, миграции).
- Реализовать API/UI слой.
- Добавить логирование в соответствии со стандартами.
- Обеспечить безопасность: аутентификация, авторизация, валидация входных данных, защита секретов.
- Обеспечить observability: structured logging, metrics, tracing, health checks.
- Обработать ошибки и исключения.
- Оптимизировать производительность при необходимости.
- Оставить метаданные о примененных скиллах в коде.

**Существующие скиллы:**
- .NET backend: множество `solution-*` скиллов в `skills/dotnet/architecture/artifacts/solutions/`.
- Angular frontend: `skills/angular/architecture/artifacts/solution-*.skill/`.
- Python: `skills/python/architecture/solutions/solution-default-cli.skill/`.
- `skills/common-workflow/develop/logging-principle.skill.md` — правила логирования.
- `skills/dotnet/workflows/leave-info-of-applied-skill.skill.md` — метаданные скиллов в C#.

**Пробелы:**
- Нет .NET скиллов по аутентификации/авторизации.
- Нет .NET скилла управления секретами.
- Нет .NET скилла безопасной валидации входных данных / OWASP.
- Нет .NET скилла observability (Serilog, OpenTelemetry, metrics, health checks).
- Нет .NET скилла стратегии обработки исключений.
- Нет .NET скиллов производительности: кэширование, пагинация, N+1, оптимизация запросов.
- Нет .NET скиллов интеграций: messaging, outbox, background jobs.
- Angular: нет design system, i18n, accessibility, feature flags, real-time.

---

### Этап 6. Тестирование

**Задачи агента:**
- Написать unit-тесты согласно плану.
- Написать интеграционные тесты.
- Подготовить тестовые данные и фабрики.
- Провести ручное /exploratory тестирование изменений.
- Измерить покрытие кода.
- Написать E2E-тесты для критических сценариев.
- При необходимости провести нагрузочное тестирование.

**Существующие скиллы:**
- `skills/common-workflow/test/workflow-unittest-testplan.skill/` — планирование unit-тестов.
- `skills/common-workflow/test/code-coverage.skill.md` — требования к покрытию.
- `skills/dotnet/testing/dotnet-unittest.skill/` — шаблон .NET unit tests.
- `skills/dotnet/testing/testing-strategy.skill.md` — стратегия тестирования .NET.
- `skills/angular/architecture/artifacts/solution-testing.skill/` — Angular testing.

**Пробелы:**
- Нет .NET скилла интеграционного тестирования (Testcontainers, WireMock).
- Нет .NET скилла E2E-тестирования API/UI.
- Нет скилла управления тестовыми данными (factories, seeders, cleanup).
- Нет скилла нагрузочного тестирования.
- Нет скилла mutation testing.

---

### Этап 7. Самопроверка и качество

**Задачи агента:**
- Запустить линтеры и статический анализ.
- Проверить форматирование кода.
- Убедиться, что проект собирается.
- Проверить типы / compile-time ошибки.
- Провести self-code-review по чек-листу.
- Проверить соответствие примененных solution-скиллов архитектуре.

**Существующие скиллы:**
- `skills/common-workflow/test/architect-validator.skill.md` — валидация применения plateau/solution.

**Пробелы:**
- Нет скилла запуска статического анализа.
- Нет скилла форматирования кода.
- Нет скилла self-review checklist.

---

### Этап 8. Code Review и оформление PR

**Задачи агента:**
- Подготовить PR: заголовок, описание изменений, чек-листы, ссылки на задачу.
- Убедиться, что CI проходит локально / в PR.
- Провести саморевью перед запросом review.
- Ответить на замечания ревьюеров.
- Внести правки и перепроверить.

**Существующие скиллы:**
- `skills/common-workflow/work-in-git-tree.skill.md` — создание PR.

**Пробелы:**
- Нет скилла оформления pull request.
- Нет скилла процесса code review.
- Нет скилла обработки feedback от ревьюеров.
- Нет скилла merge strategy.

---

### Этап 9. CI/CD, merge и публикация

**Задачи агента:**
- Убедиться, что PR-validation workflow прошел.
- Исправить ошибки CI.
- Получить необходимые approve.
- Вмержить изменения в целевую ветку.
- Удалить task branch и worktree.
- При необходимости запустить deploy workflow.
- Управлять версией и changelog при релизе.

**Существующие скиллы:**
- `skills/devops/devops-github-wf-pr-validation.skill/` — PR validation workflow.
- `skills/ansible/ansible-*-requirements/SKILL.md` — общие требования к деплой-автоматизации.

**Пробелы:**
- Нет скилла deploy workflow (GitHub Actions).
- Нет скилла release management (versioning, changelog).
- Нет скилла rollback strategy.
- Нет скилла environment promotion.
- Нет скилла package publishing (NuGet, npm, Docker).

## 4. Сводка пробелов по приоритету

### Критические (блокируют end-to-end процесс)
1. Бизнес-анализ и прием требований.
2. Проектирование БД и миграций.
3. Code review и оформление PR.
4. CI/CD deploy и release.
5. Безопасность backend.
6. Observability backend.
7. Git-стратегия (branch naming, commit conventions, merge strategy).

### Средние (заметно снижают качество процесса)
8. Интеграционное и E2E тестирование .NET.
9. Управление тестовыми данными.
10. Статический анализ и форматирование.
11. API contract design.
12. Integration design (outbox, messaging).

### Низкие / улучшения
13. Angular design system, i18n, accessibility, feature flags.
14. Нагрузочное и mutation testing.
15. Performance optimization skills.
16. Доработка пустого `create-new-operation.md`.
