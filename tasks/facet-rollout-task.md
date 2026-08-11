# Задача: массовая переразметка тегов всех скиллов под facet-словарь

## Контекст
Репозиторий — база из 336 файлов `*.skill.md` под `skills/`. Только что введена контролируемая facet-система тегов для `tags:` во frontmatter, чтобы внешний инструмент синка (`aism`/`ai-skill-manager`, версия 1.7.0) мог резолвить набор скиллов под конкретного агента булевым запросом по тегам (`stack/typescript & concern/testing`) вместо ручного списка путей.

Обязательные документы, прочитать перед началом:
- [skills/common-workflow/skill-design.skill/skill-design.skill.md](skills/common-workflow/skill-design.skill/skill-design.skill.md) — правило про теги (`## MUST`, пункт про facet-разметку) и check list.
- [skills/common-workflow/skill-design.skill/facet-vocabulary.md](skills/common-workflow/skill-design.skill/facet-vocabulary.md) — **источник истины**: список фасетов, значений, правил и self-check из 5 вопросов перед добавлением нового значения.

Эта задача — практическое применение уже утверждённого словаря к существующим файлам. Сам словарь не менять без явного повода (если в процессе найдётся значение, не укладывающееся в словарь — см. раздел "Что делать при неоднозначности" ниже, не изобретать самостоятельно).

## Масштаб
Все 336 файлов `skills/**/*.skill.md`. Текущее состояние тегов:
- 333 файла уже имеют `tags:`, но вперемешку — часть тегов относится к facet-системе (голые слова вроде `dotnet`, `angular`, `testing`, `workflow/test`), часть — к отдельной служебной системе `skill/*`/`plateau/*`, часть — узкоспециализированные ключевые слова (`mediatr`, `xunit`, `guid`...).
- 3 файла вообще без `tags:` (нужно добавить с нуля):
  - `skills/common-workflow/mermaid-diagram.skill.md`
  - `skills/common-workflow/test/code-coverage.skill.md`
  - `skills/common-workflow/test/workflow-unittest-testplan.skill/workflow-unittest-testplan.skill.md`

## Что НЕ трогать (важно)
- Любой тег с префиксом `skill/` или `plateau/` (например `skill/template/class`, `plateau/multiuser-app`) — отдельная система разметки для tooling генерации plateau/solution-скиллов (`plateau-create-by-solutions`). Байт в байт как есть.
- Узкоспециализированные ключевые слова, не совпадающие по смыслу ни с одним значением из словаря (`mediatr`, `cqrs`, `xunit`, `guid`, `idempotency`, `fluent-validation`, `dto`, `entity`, `ddd`, `concurrency`, `query`, `handler`, `github-actions`, `pr-validation`, `quality`, `nx`, `aspnet-core`* и т.д.) — оставить как есть, это свободные теги по новому `## SHOULD`-правилу в skill-design.
- Поля `domain`, `type`, `version`, `creates`, `extends`, `depends_on`, `adr` во frontmatter — не относятся к этой задаче, не трогать.
- Названия и расположение файлов — не переименовывать, не перемещать.

\* Единственное исключение: если встретите готовый ASP.NET Core-специфичный скилл, ему полагается `framework/aspnet-core` — это уже согласованное значение facet `framework/*`, а не long-tail. Но не изобретайте его для скиллов, которые просто "про dotnet backend" в общем — для этого есть `app-type/service`.

## Что делать: правило замены

Для каждого файла с уже существующими "голыми" тегами: если голый тег **по смыслу точно совпадает** со значением из словаря — заменить его на правильно намспейсенный facet-тег (не оставлять дубликатом рядом). Если тег несёт дополнительный смысл, которого facet не покрывает — оставить как есть.

Готовая таблица замен для тегов, которые точно встречаются в репозитории (по частоте):

| Существующий голый тег | Действие |
|---|---|
| `dotnet` | → `stack/dotnet` |
| `angular` | → `stack/typescript` + `framework/angular` |
| `python` | → `stack/python` |
| `testing` | → `concern/testing` |
| `architecture` | → `concern/architecture` |
| `workflow/test` | → `concern/testing` (уводим из `workflow/*` — тот зарезервирован под другое, см. словарь) |
| `unit-testing` | → `concern/testing/unit` **и** `concern/testing` (родитель обязателен, движок не разворачивает иерархию на стороне тега скилла) |
| `bdd` | → `concern/testing/bdd` **и** `concern/testing` |
| `mutation-testing` | → `concern/testing/mutation` **и** `concern/testing` |
| `devops` (на `devops-github-wf-*` скиллах) | → `concern/ci` |
| `ci` | → `concern/ci` |

Всё, чего нет в этой таблице и нет в словаре — не трогать.

## Что делать: правило добавления (если ещё не покрыто заменами)

Для каждого файла проверить, что после замен выполняются требования из `## MUST` в skill-design.skill.md:
1. **Ровно один `stack/<value>`, либо голый `stack`.** Определять по тому, под какой папкой верхнего уровня лежит файл (`skills/python/...` → `stack/python`, `skills/dotnet/...` → `stack/dotnet`, `skills/typescript/...` и `skills/angular/...` → `stack/typescript`, `skills/ansible/...` → `stack/ansible`). Файлы под `skills/common-workflow/...` и `skills/devops/...` — без стека → голый `stack`, если содержимое явно не завязано на один язык (проверять `whenToUse`/description — если там написано "for any language" и т.п., это стопроцентный сигнал для голого `stack`).
2. **Минимум один `concern/*`.** Определять по подпапке (`architecture`, `testing`, `documentation`) и/или по содержанию. **Multi-value — норма, не только для одного скилла.** Пример уже разобранного случая: `skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/` лежит в `architecture`, но по содержанию (Cucumber/coverage/mutation gate) — это ещё и testing → должен получить оба: `concern/architecture` **и** `concern/testing` (плюс `concern/testing/bdd`, `concern/testing/mutation` по содержанию, с обязательным родителем `concern/testing`).
3. **`framework/*` и `app-type/*`** — добавлять только когда контент реально об этом (не подгонять). `app-type/cli` — только для скиллов про консольные приложения; `app-type/service` — только для скиллов именно про backend-сервисы. Если сомневаетесь — не добавлять, это необязательный facet.
4. Для 3 файлов без тегов вообще — определить `stack`/`concern` по папке и содержанию, добавить `tags:` блок с нуля.

## Что делать при неоднозначности
Если встретится тег или ситуация, не укладывающаяся в существующий словарь (нужно новое значение facet'а, которого нет в `facet-vocabulary.md`) — **не изобретать самостоятельно**. Прогнать через self-check из 5 вопросов в конце `facet-vocabulary.md`. Если после self-check значение выглядит легитимным — вынести отдельным списком в конце работы («предлагаю добавить в словарь: ...») вместо тихого добавления в файлы.

## Процесс (рекомендация, не обязательна дословно)
Учитывая объём (333 файла), разумно:
1. Механическая часть — таблица прямых замен выше — можно сделать скриптом (прочитать frontmatter, применить замены построчно), это покроет большую часть файлов почти без судейства.
2. Ручной/LLM-проход по оставшимся случаям: multi-concern (как `dotnet-solution-conformance-testing`), файлы без тегов, файлы, где `stack`/`concern` не считываются однозначно из пути.
3. Работать батчами по папкам верхнего уровня (`python`, `dotnet`, `typescript`+`angular`, `ansible`, `devops`, `common-workflow`) — так легче ревьюить дифф порциями.

## Acceptance criteria
- Каждый файл `skills/**/*.skill.md` имеет минимум один `concern/*` тег.
- Каждый файл имеет ровно один `stack/<value>` тег либо голый `stack` — никогда ни то, ни другое одновременно, никогда ничего из двух.
- Ни в одном файле нет тега, где в одном `/`-пути сцеплены два разных facet'а (например `stack/dotnet/service` или `angular/component`).
- Каждый `concern/testing/<sub>` сопровождается отдельным тегом `concern/testing`.
- Все `skill/*` и `plateau/*` теги во всех 336 файлах — байт в байт как были, без изменений (проверяется диффом — эти строки не должны фигурировать в патче вообще).
- Три ранее нетегированных файла получили `tags:` с как минимум `stack`/`stack/<value>` и `concern/*`.
- Список кандидатов на новые facet-значения (если такие всплыли) приложен отдельно, не влит молча в файлы.

## Пример готового результата (для сверки)
`skills/dotnet/testing/dotnet-unittest.skill/dotnet-unittest.skill.md`, было:
```yaml
tags:
- dotnet
- unit-testing
- xunit
- mocks
- coverage
```
стало:
```yaml
tags:
- stack/dotnet
- concern/testing
- concern/testing/unit
- xunit
- mocks
- coverage
```
(`xunit`/`mocks`/`coverage` — long tail, не трогаем; `dotnet`→`stack/dotnet`; `unit-testing`→`concern/testing/unit` + обязательный родитель `concern/testing`.)
