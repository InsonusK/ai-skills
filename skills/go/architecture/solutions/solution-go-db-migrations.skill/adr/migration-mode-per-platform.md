---
name: migration-mode-per-platform
description: Whether this solution supports one migration call site or two, and which deployment platforms/topologies use which
problem: solution-go-db-migrations first shipped as Job-mode-only (see the superseded adr/job-only-not-startup-run.md). The catalog owner's own real deployment topology makes that stricter than necessary — Docker Stack deployments in that environment are always single-replica (no concurrency risk), and Kubernetes deployments always go through a Helm chart (where a migration Job is "free" to gate on, no extra scripted step needed) — and the owner does not want a two-command deploy sequence anywhere it can be avoided. The owner asked whether reinstating a startup-run call site alongside the job, and having both wired at once as a safety net, creates problems; then asked for the two modes to coexist, selected per platform, with a bilingual ADR recording the full reasoning.
decision: This solution supports two mutually exclusive modes, switched by one config flag (MIGRATE_ON_START, default false) — Job mode (cmd/migrate, the default) and MigrateOnStart mode (cmd/{service}/main.go's own guarded startup call). devops-service-deploy.skill.md states the condition for choosing MigrateOnStart (at most one instance of the migrating process can ever run concurrently for that deployment) rather than a platform-unconditional mapping; for this catalog owner's own topology that resolves to Docker Stack (always single-replica) using MigrateOnStart and Kubernetes (always via Helm) using Job mode with MIGRATE_ON_START explicitly false. Running both modes at once for the same deployment is never correct, even though it would not corrupt data.
tags:
  - solution/go-db-migrations
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# English

## Problem

`solution-go-db-migrations` first shipped supporting only one call site for
`Migrate(ctx, dsn) error`: a standalone `cmd/migrate` binary, run as a deploy-time job ahead of the
app (see the superseded [[./job-only-not-startup-run.md|adr/job-only-not-startup-run.md]]). That
decision was correct in general, but stricter than this catalog owner's own real deployment
topology needs:

- Every Docker Stack deployment the owner actually runs is single-replica. The concurrency race
  Job-mode-only was designed to eliminate (N replicas each attempting the same migration) cannot
  happen there regardless of which mode is used.
- Every Kubernetes deployment the owner actually runs goes through a Helm chart, never plain
  `kubectl` manifests. A Helm `pre-install,pre-upgrade` hook Job blocks the release natively — no
  extra scripted step, no "two commands" the owner explicitly wants to avoid wherever avoidable.

Given that, the owner asked two further questions before settling this: first, whether keeping the
startup-run call site **in addition to** the job — as a defensive fallback, both wired at once —
creates problems; then, having been shown concrete reasons it does, asked for the two modes to
coexist as explicit, mutually exclusive, per-deployment choices instead, with the actual
per-platform assignment decided in `devops-service-deploy.skill.md` and the whole reasoning recorded
in one ADR, in both English and Russian, because of how much nuance it carries.

## Verification performed

Before finalizing, the underlying claims below were checked, not assumed:

- **The Twelve-Factor App methodology**, Factor XI ("Admin processes"), states one-off admin tasks —
  database migrations are named explicitly — must run as a separate process from the app's regular
  long-running process, using the same codebase, environment, and config as that app. This
  independently corroborates the shape both of this solution's modes already have (`cmd/migrate`
  shares the same repo/image/config-loading as `cmd/{service}`), and specifically endorses
  separateness as the default, not startup-coupling.
- **Kubernetes-specific practitioner consensus** (checked directly, not assumed from general
  reputation): migrating at application startup is named as the anti-pattern for exactly the reason
  this catalog's own owner gave originally — "with N replicas you get N concurrent migration
  attempts." Init containers were checked and explicitly rejected in the same sources for the
  identical reason: an init container runs per-pod, so it has the same concurrency problem as
  startup-run, just moved earlier in the pod lifecycle — it does not solve anything a plain
  MigrateOnStart mode doesn't already have to solve via locking. The named best practice matches
  this solution's own Job mode almost verbatim: "one release-gated Job migrates before the
  Deployment rolls."
- **Helm hook blocking behavior**, verified against Helm's own documentation: a `Job` (or `Pod`)
  resource annotated `helm.sh/hook: pre-install,pre-upgrade` is applied and *waited on* by Helm's
  own hook runner before any other resource in the release is touched; if the hook fails, the
  release fails. This is genuinely native blocking (no `kubectl wait` step of the operator's own),
  confirming that Kubernetes-via-Helm can use Job mode with zero deploy-script cost — the concrete
  reason it is designated Job mode below rather than MigrateOnStart, independent of replica count.

## Concrete problems with wiring both modes at once (why "both, as a safety net" was rejected)

Running both call sites for the same deployment (`cmd/migrate` invoked by the pipeline *and*
`MIGRATE_ON_START=true`) would not corrupt data — goose's session lock still serializes any
concurrent attempt. But it was rejected because it reintroduces real operational problems this
solution's whole design exists to avoid:

1. **Every process restart touches the database, not just deploys.** A crash, an OOM-kill, a node
   eviction, an autoscaler scale-out — none of these previously touched the database at all under
   Job mode. With a startup fallback also wired, every one of them now does.
2. **It directly undermines autoscaling**, which matters because "scales better" was the owner's own
   original stated reason for preferring a separate job in the first place: a scale-out event adds
   an unnecessary database round-trip and lock-acquisition attempt on every new replica, at exactly
   the moment extra latency is least wanted.
3. **It can introduce a new pod-readiness failure mode.** If the deploy's Job is still holding the
   advisory lock when an unrelated pod restarts (for a reason that has nothing to do with the
   deploy), that unrelated pod's startup now blocks on a lock it never needed to care about,
   potentially confusing liveness/readiness probes about why the pod is slow to become ready.
4. **It silently masks a broken deploy pipeline.** If the Job step is ever accidentally skipped or
   misconfigured, Job-mode-only fails loudly (`relation does not exist`) — a clear, actionable
   signal that the pipeline needs fixing. A startup fallback would instead quietly self-heal it,
   removing the operator's only signal that the automation has drifted, until a larger or riskier
   migration eventually exposes the gap at a worse moment.
5. **It re-couples migration failure to the app process's own crash-loop** instead of to the deploy
   pipeline's pass/fail signal, for that call path specifically — one of the concrete benefits
   Job mode was chosen for in the first place.

A lighter-weight alternative exists for teams that specifically want a safety net against "the Job
step silently didn't run": a **read-only version check** at startup (compare the schema's recorded
migration version against what the binary expects; refuse to start, loudly, on a mismatch) gives the
same "catch a misconfigured pipeline" benefit without any mutation, without lock contention, and
without masking anything — a mismatch is a startup failure, not a silent fix. This solution does not
implement that check today; it is a reasonable future addition, not a substitute for choosing the
right mode.

## Selected variant

**Selected variant:** [[#Two modes, switched by one config flag, chosen per platform's own topology (selected)]]

## Searched variants

### Two modes, switched by one config flag, chosen per platform's own topology (selected)

#### Description

`solution-go-db-migrations` supports both call sites, mutually exclusive, switched by
`Config.MigrateOnStart` (`MIGRATE_ON_START` env var, default `false`):

- **Job mode** (`MIGRATE_ON_START=false`, the default): `cmd/migrate` is invoked by the deploy
  pipeline as a one-shot job/container gated ahead of the app; `cmd/{service}/main.go` never calls
  `Migrate`.
- **MigrateOnStart mode** (`MIGRATE_ON_START=true`): `cmd/{service}/main.go`'s own `run()` calls
  `Migrate` once, guarded by the flag, before constructing `Store`; `cmd/migrate` exists in the
  repository but is never wired into that deployment's pipeline.

The condition for choosing MigrateOnStart is stated generically, not as a fixed platform mapping:
**it is safe only when at most one instance of the migrating process can ever run concurrently for
that deployment.** `devops-service-deploy.skill.md`'s own "migration step" rule states this
condition — because that skill is shared by every service in this repository, not only ones on this
owner's own known topology, a future service with a multi-replica Docker Stack deployment must still
use Job mode, and the skill's own generic Stack template (`replicas: 2` by default) is left
requiring Job mode unless a specific service's own copy deliberately documents single-replica and
switches it.

For this catalog owner's own stated topology, the condition resolves exactly as requested:

| Platform | This owner's real topology | Mode | Why |
| --- | --- | --- | --- |
| Docker Compose | any | Job mode (always) | Already solved with zero cost via native `depends_on: condition: service_completed_successfully` — no reason to ever choose the other mode here. |
| Docker Stack (Swarm) | always single-replica | MigrateOnStart | The condition ("at most one instance") is true by construction for this owner's services; the two-step wait-and-deploy sequence Job mode would otherwise require is unnecessary overhead for a case with no race to prevent. |
| Kubernetes, plain manifests | not used by this owner | Job mode (default guidance for other teams) | Replica count is commonly > 1; without Helm's hook mechanism, only Job mode + `kubectl wait` is safe. |
| Kubernetes via Helm | always used by this owner | Job mode, `MIGRATE_ON_START` explicitly `false` | A Helm `pre-install,pre-upgrade` hook Job blocks the release natively regardless of replica count — Job mode is "free" here, so it is the default independent of the replica-count condition. |

#### Benefits

- Resolves to exactly the owner's own stated preference (Stack → MigrateOnStart, Kubernetes → Job)
  with zero extra deploy steps anywhere in their own real environment, while the underlying rule
  stays correct and safe for any other team's different topology using the same shared
  `devops-service-deploy.skill.md`.
- `cmd/migrate` is never removed — a team whose Stack topology later grows beyond single-replica (or
  a team that never wants to reason about the condition at all) can switch that one deployment to
  Job mode without this solution changing at all.
- The "running both is unsafe" question the owner asked is answered structurally, not just in prose:
  the flag is a single switch, and every Rule in this solution's own Implementation files states
  which of the two call sites is the one gated by it.

#### Costs

- The safety of MigrateOnStart mode now depends on a fact this solution cannot verify at build
  time (the deployment's actual replica count) — stated as an explicit Boundary and Check-list item
  rather than enforced in code, which relies on the deploying team reading and honoring it.
- Two call sites to keep straight instead of one — mitigated by the flag being the *only* switch
  (no other divergence between the two code paths) and by every Rule cross-referencing it.

### Job mode only, unconditionally (superseded — the solution's previous design)

#### Description

Keep [[./job-only-not-startup-run.md|the earlier decision]]: `cmd/migrate` is the only call site for
every deployment, on every platform, regardless of replica count.

#### Benefits

- Structurally impossible to misconfigure the replica-count condition, because there is no
  condition to get right — one mode, always safe.
- Simpler to reason about and document — no decision matrix, no per-platform assignment.

#### Costs

- Forces a scripted two-step deploy sequence (`kubectl apply` + `kubectl wait`, or the Docker Stack
  two-command sequence) even on this owner's own Kubernetes-via-Helm path, where a Helm hook would
  have made that unnecessary — real, avoidable operational overhead for no safety benefit, since
  Helm's hook already provides the same guarantee for free.
- Forces the same scripted overhead onto this owner's Docker Stack deployments, where the
  concurrency problem Job mode exists to prevent cannot occur at all (single-replica, always) —
  paying a real cost to guard against a risk that is not present.

### Both modes wired simultaneously as a safety net (rejected)

#### Description

Keep `cmd/migrate` invoked by the deploy pipeline *and* leave `MIGRATE_ON_START=true`, so the app
also attempts migration at startup regardless — the literal "what if we do both" question the owner
asked before this ADR was written.

#### Benefits

- If the Job step is ever accidentally skipped, the app "self-heals" the schema instead of failing.

#### Costs

- All five concrete problems in [Concrete problems with wiring both modes at once](#concrete-problems-with-wiring-both-modes-at-once-why-both-as-a-safety-net-was-rejected)
  above: every process restart (not just deploys) touches the database; it undermines autoscaling
  specifically; it can introduce new pod-readiness stalls when an unrelated restart races a
  still-running Job; it silently masks a broken deploy pipeline instead of failing loudly; and it
  re-couples migration failure to the app's own crash-loop for that path.
- The "self-heal" benefit is better achieved, without any of those costs, by a read-only
  version-mismatch check at startup instead of a full migration attempt (see above) — this variant
  was rejected in favor of leaving that lighter check as a documented future option, not building
  the heavier, riskier version now.

### Startup-run only, unconditionally (rejected — carried forward from the superseded ADR)

#### Description

Drop `cmd/migrate` entirely; every deployment on every platform calls `Migrate` from
`cmd/{service}/main.go`'s own startup path.

#### Benefits

- Simplest possible deployment story on any platform — nothing to wire ahead of the app at all.

#### Costs

- Unsafe for any topology where more than one replica may run concurrently — exactly the
  Kubernetes-without-Helm and multi-replica-Stack cases this catalog's own `devops-service-deploy.skill.md`
  must still support for other teams, even though this owner's own topology happens not to need it.
- Ties migration success to the app process's own crash-loop/restart policy instead of to the deploy
  pipeline's own pass/fail signal on every platform, including the ones where a clean Job/Helm-hook
  signal is available for free.

---

# Русский

## Проблема

`solution-go-db-migrations` изначально поддерживал только одну точку вызова
`Migrate(ctx, dsn) error`: отдельный бинарник `cmd/migrate`, запускаемый как job при деплое, до
старта приложения (см. устаревшее решение
[[./job-only-not-startup-run.md|adr/job-only-not-startup-run.md]]). Это решение было верным в общем
случае, но строже, чем требует реальная топология деплоя владельца каталога:

- Все Docker Stack-деплои, которые владелец реально использует, — однорепличные. Гонка при
  параллельных миграциях, ради устранения которой был выбран режим "только job", там в принципе
  невозможна вне зависимости от того, какой режим используется.
- Все Kubernetes-деплои, которые владелец реально использует, идут через Helm chart, а не через
  голые манифесты `kubectl`. Job-хук Helm `pre-install,pre-upgrade` блокирует релиз нативно — без
  дополнительного скриптового шага, без "двух команд", которых владелец явно хочет избежать там, где
  это возможно.

Учитывая это, владелец задал перед принятием решения ещё два вопроса: во-первых, не создаёт ли
проблем сохранение точки вызова при старте сервиса **в дополнение** к job — как защитный
"страховочный" вариант, когда включены оба сразу; затем, увидев конкретные причины, почему это
проблема, попросил, чтобы два режима сосуществовали как явный, взаимоисключающий выбор на один
деплой, а конкретное соответствие "платформа → режим" определялось в `devops-service-deploy.skill.md`,
и чтобы вся аргументация была зафиксирована в одном ADR, на русском и английском, из-за того, что
тема сложная и в ней много нюансов.

## Проведённая проверка

Перед тем как зафиксировать решение, следующие утверждения были проверены, а не приняты на веру:

- **Методология Twelve-Factor App**, фактор XI ("Admin processes"), прямо говорит: разовые
  административные задачи — миграции БД названы явно — должны выполняться как **отдельный процесс**
  от основного долгоживущего процесса приложения, но с тем же кодом, окружением и конфигурацией, что
  и у приложения. Это независимо подтверждает форму, которую уже имеют оба режима этого решения
  (`cmd/migrate` использует тот же репозиторий/образ/загрузку конфига, что и `cmd/{service}`), и
  прямо поддерживает раздельность как поведение по умолчанию, а не связанность со стартом.
- **Консенсус практиков конкретно по Kubernetes** (проверено напрямую, а не принято на веру из общей
  репутации): миграция при старте приложения прямо названа антипаттерном ровно по той причине, что
  изначально называл сам владелец этого каталога — "при N репликах получаем N одновременных попыток
  миграции". Вариант с initContainer был проверен и явно отвергнут в тех же источниках по идентичной
  причине: initContainer выполняется на каждый под отдельно, то есть имеет ту же проблему
  параллелизма, что и миграция при старте, просто сдвинутую раньше в жизненном цикле пода — он не
  решает ничего, что не пришлось бы решать и обычному MigrateOnStart-режиму через блокировку. Явно
  названная лучшая практика почти дословно совпадает с Job-режимом этого решения: "один Job,
  привязанный к релизу, мигрирует перед тем, как раскатывается Deployment".
- **Блокирующее поведение Helm-хуков**, проверено по официальной документации Helm: ресурс `Job`
  (или `Pod`) с аннотацией `helm.sh/hook: pre-install,pre-upgrade` применяется, и сам механизм
  обработки хуков Helm **ждёт** его завершения, прежде чем трогать любой другой ресурс релиза; если
  хук падает — падает весь релиз. Это действительно нативная блокировка (без собственного шага
  `kubectl wait` у оператора), что подтверждает: Kubernetes через Helm может использовать Job-режим
  без какой-либо дополнительной стоимости в деплой-скрипте — конкретная причина, почему ниже для
  этого пути назначен именно Job-режим, независимо от количества реплик.

## Конкретные проблемы при включении обоих режимов одновременно (почему "оба сразу, для подстраховки" отклонено)

Запуск обеих точек вызова для одного и того же деплоя (`cmd/migrate` вызывается пайплайном **и**
одновременно стоит `MIGRATE_ON_START=true`) не приведёт к порче данных — advisory lock из goose всё
равно сериализует любую параллельную попытку. Но это было отклонено, потому что возвращает реальные
эксплуатационные проблемы, ради устранения которых всё это решение и строилось:

1. **Каждый рестарт процесса теперь трогает базу данных, а не только деплой.** Краш, OOM-kill,
   вытеснение с ноды, скейл-аут автоскейлера — раньше ничего из этого вообще не трогало базу данных
   при Job-режиме. При подключённом страховочном старте — теперь трогает всё из перечисленного.
2. **Это напрямую бьёт по автомасштабированию** — а ведь именно "лучше масштабируется" было исходной
   причиной владельца в пользу отдельного job. Событие скейл-аута добавляет лишний round-trip в базу
   и попытку взять lock на каждой новой реплике, именно в тот момент, когда лишняя задержка меньше
   всего нужна.
3. **Может появиться новый сценарий "зависшей" готовности пода.** Если Job деплоя всё ещё держит
   advisory lock в момент, когда перезапускается какой-то не связанный с деплоем под, этот под теперь
   тоже встанет в очередь на lock, который ему вообще не был нужен, — это может запутать
   liveness/readiness-пробы насчёт того, почему под долго не становится готовым.
4. **Это тихо маскирует поломку деплой-пайплайна.** Если шаг с Job случайно пропущен или неправильно
   настроен, режим "только job" падает громко (`relation does not exist`) — чёткий, действенный
   сигнал, что пайплайн нужно чинить. Страховочный запуск при старте вместо этого тихо "самовылечит"
   ситуацию, убирая единственный сигнал оператору о том, что автоматизация разъехалась с реальностью,
   — пока более крупная или рискованная миграция однажды не проявит этот разрыв в худший момент.
5. **Это снова связывает неуспех миграции с crash-loop приложения** вместо чёткого
   успех/неуспех-сигнала от деплой-пайплайна — именно для этого пути, то есть теряется одно из
   конкретных преимуществ, ради которых изначально выбирался Job-режим.

Для команд, которым нужна именно "подстраховка на случай, что шаг с Job тихо не выполнился", есть
более лёгкая альтернатива: **проверка версии в режиме только для чтения** при старте (сравнить
записанную версию миграции в схеме с тем, что ожидает бинарник; при несовпадении — громко отказаться
стартовать) даёт то же самое преимущество "поймать сломанный пайплайн", но без каких-либо мутаций,
без конкуренции за lock и без маскировки чего бы то ни было — несовпадение становится ошибкой старта,
а не тихим исправлением. Это решение такую проверку сегодня не реализует; это разумное будущее
дополнение, а не замена выбору правильного режима.

## Выбранный вариант

**Выбранный вариант:** [[#Два режима, переключаемых одним флагом конфигурации, выбор по топологии платформы (выбрано)]]

## Рассмотренные варианты

### Два режима, переключаемых одним флагом конфигурации, выбор по топологии платформы (выбрано)

#### Описание

`solution-go-db-migrations` поддерживает обе точки вызова, взаимоисключающие, переключаемые через
`Config.MigrateOnStart` (переменная окружения `MIGRATE_ON_START`, по умолчанию `false`):

- **Job-режим** (`MIGRATE_ON_START=false`, по умолчанию): `cmd/migrate` запускается деплой-пайплайном
  как одноразовый job/контейнер перед приложением; `cmd/{service}/main.go` никогда не вызывает
  `Migrate`.
- **MigrateOnStart-режим** (`MIGRATE_ON_START=true`): `run()` в `cmd/{service}/main.go` сам вызывает
  `Migrate` один раз, под защитой флага, перед созданием `Store`; `cmd/migrate` существует в
  репозитории, но никогда не подключается в пайплайн для этого деплоя.

Условие для выбора MigrateOnStart сформулировано обобщённо, а не как жёсткая привязка к платформе:
**он безопасен только тогда, когда для данного деплоя одновременно может работать не более одного
экземпляра мигрирующего процесса.** Правило "шаг миграции" в `devops-service-deploy.skill.md`
формулирует именно это условие — поскольку этот skill общий для всех сервисов в этом репозитории, а
не только для тех, что имеют известную топологию этого владельца, будущий сервис с многорепличным
Docker Stack-деплоем всё равно обязан использовать Job-режим, а собственный общий шаблон Stack в этом
skill (`replicas: 2` по умолчанию) по-прежнему требует Job-режим, пока конкретный сервис сознательно
не задокументирует однорепличность и не переключит режим сам.

Для реальной топологии владельца этого каталога условие разрешается ровно так, как было запрошено:

| Платформа | Реальная топология владельца | Режим | Почему |
| --- | --- | --- | --- |
| Docker Compose | любая | Job-режим (всегда) | Уже решено бесплатно через нативный `depends_on: condition: service_completed_successfully` — здесь нет причины выбирать другой режим. |
| Docker Stack (Swarm) | всегда однореплично | MigrateOnStart | Условие ("не более одного экземпляра") истинно по построению для сервисов этого владельца; двухшаговая последовательность деплоя с ожиданием, которую иначе потребовал бы Job-режим, — лишние накладные расходы там, где нет гонки, от которой нужно защищаться. |
| Kubernetes, голые манифесты | этим владельцем не используется | Job-режим (рекомендация по умолчанию для других команд) | Количество реплик обычно > 1; без механизма хуков Helm безопасен только Job-режим + `kubectl wait`. |
| Kubernetes через Helm | всегда используется этим владельцем | Job-режим, `MIGRATE_ON_START` явно `false` | Job-хук Helm `pre-install,pre-upgrade` блокирует релиз нативно вне зависимости от числа реплик — здесь Job-режим "бесплатен", поэтому он выбран по умолчанию независимо от условия про количество реплик. |

#### Преимущества

- Разрешается ровно в то, что и просил владелец (Stack → MigrateOnStart, Kubernetes → Job), без
  каких-либо дополнительных шагов деплоя в его реальном окружении, при этом базовое правило остаётся
  корректным и безопасным для любой другой команды с другой топологией, использующей тот же общий
  `devops-service-deploy.skill.md`.
- `cmd/migrate` никуда не убирается — команда, чья Stack-топология позже вырастет за пределы одной
  реплики (или команда, которая вообще не хочет думать об этом условии), может переключить именно
  этот деплой на Job-режим без каких-либо изменений в самом решении.
- Вопрос владельца "не опасно ли запускать оба сразу" получает структурный ответ, а не просто
  текстовый: флаг — это единственный переключатель, и каждое правило в Implementation-файлах этого
  решения явно указывает, какая из двух точек вызова им управляется.

#### Недостатки

- Безопасность MigrateOnStart-режима теперь зависит от факта, который это решение не может
  проверить на этапе сборки (реальное количество реплик деплоя), — зафиксировано явно как Boundary и
  пункт чек-листа, а не обеспечено кодом, то есть полагается на то, что деплоящая команда это условие
  прочитает и соблюдёт.
- Две точки вызова вместо одной, за которыми нужно следить, — смягчается тем, что флаг — это
  *единственное* различие (никакого другого расхождения между двумя путями кода нет) и тем, что
  каждое правило на него явно ссылается.

### Только Job-режим, безусловно (устарело — предыдущий дизайн решения)

#### Описание

Сохранить [[./job-only-not-startup-run.md|предыдущее решение]]: `cmd/migrate` — единственная точка
вызова для любого деплоя, на любой платформе, вне зависимости от числа реплик.

#### Преимущества

- Структурно невозможно неправильно оценить условие про количество реплик, потому что оценивать
  нечего — один режим, всегда безопасен.
- Проще рассуждать и документировать — нет матрицы решений, нет привязки к платформе.

#### Недостатки

- Вынуждает использовать скриптовую двухшаговую последовательность деплоя (`kubectl apply` +
  `kubectl wait`, либо аналогичная двухкомандная последовательность для Docker Stack) даже на
  собственном пути владельца — Kubernetes через Helm, — где Helm-хук сделал бы это ненужным:
  реальные, устранимые эксплуатационные накладные расходы без выигрыша в безопасности, поскольку
  Helm-хук уже даёт ту же гарантию бесплатно.
- Вынуждает те же скриптовые накладные расходы на Docker Stack-деплои этого владельца, где проблема
  параллелизма, ради устранения которой существует Job-режим, в принципе не может возникнуть (всегда
  одна реплика), — реальная стоимость платится ради защиты от риска, которого нет.

### Оба режима включены одновременно, для подстраховки (отклонено)

#### Описание

Оставить `cmd/migrate`, вызываемый деплой-пайплайном, **и** одновременно `MIGRATE_ON_START=true`, так
что приложение тоже пытается мигрировать при старте вне зависимости от этого, — буквально тот вопрос
"а что если сделать и то, и другое", который владелец задал до написания этого ADR.

#### Преимущества

- Если шаг с Job случайно пропущен, приложение "самовылечивает" схему вместо падения.

#### Недостатки

- Все пять конкретных проблем из раздела [Конкретные проблемы при включении обоих режимов одновременно](#конкретные-проблемы-при-включении-обоих-режимов-одновременно-почему-оба-сразу-для-подстраховки-отклонено)
  выше: каждый рестарт процесса (а не только деплой) теперь трогает базу; это напрямую бьёт по
  автомасштабированию; может появиться новый сценарий зависшей готовности пода, когда несвязанный
  рестарт пересекается с ещё выполняющимся Job; это тихо маскирует сломанный деплой-пайплайн вместо
  громкого падения; и это снова связывает неуспех миграции с crash-loop приложения для этого пути.
- Преимущество "самовылечения" лучше достигается, без всех этих издержек, проверкой версии в режиме
  только для чтения при старте вместо полноценной попытки миграции (см. выше) — этот вариант отклонён
  в пользу того, чтобы оставить такую более лёгкую проверку как задокументированную опцию на будущее,
  а не строить сейчас более тяжёлую и рискованную версию.

### Только режим при старте, безусловно (отклонено — перенесено из устаревшего ADR)

#### Описание

Полностью убрать `cmd/migrate`; каждый деплой на любой платформе вызывает `Migrate` из стартового
пути `cmd/{service}/main.go`.

#### Преимущества

- Самая простая из возможных схем деплоя на любой платформе — вообще ничего не нужно разворачивать
  перед приложением.

#### Недостатки

- Небезопасно для любой топологии, где одновременно может работать больше одной реплики, — а именно
  такие случаи (Kubernetes без Helm, многорепличный Stack) собственный
  `devops-service-deploy.skill.md` этого каталога обязан по-прежнему поддерживать для других команд,
  даже если топологии самого владельца это не требуется.
- Связывает успех миграции с политикой рестарта/crash-loop процесса приложения вместо
  успех/неуспех-сигнала деплой-пайплайна на любой платформе, включая те, где чистый сигнал от
  Job/Helm-хука доступен бесплатно.
