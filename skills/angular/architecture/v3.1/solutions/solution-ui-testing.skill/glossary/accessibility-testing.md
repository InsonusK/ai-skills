# Accessibility testing

**Accessibility testing** (тестирование доступности) — это автоматическая проверка UI-компонента на соответствие механически проверяемым правилам WCAG: контраст, корректные ARIA-атрибуты, наличие меток у элементов управления и структура заголовков/ориентиров.

## What is WCAG

WCAG (Web Content Accessibility Guidelines) — это международный стандарт, разработанный консорциумом W3C в рамках инициативы WAI. Он описывает, как сделать веб-контент доступным для людей с ограничениями (зрение, слух, двигательные и когнитивные). **Правила определены не нашей командой**, а общемировым сообществом; `@axe-core/playwright` лишь механически проверяет, что отрисованный компонент соответствует этим правилам. Версии WCAG 2.1/2.2 включают критерии вроде минимального контраста 4.5:1, корректных ARIA-ролей и меток для элементов управления.

## Why it exists

Поведенческие DOM-тесты могут случайно проверить наличие `role` или `label`, но только там, где автор теста об этом подумал. Визуальные скриншоты показывают, как выглядит компонент, но не измеряют контраст и не валидируют ARIA. Автоматизированный accessibility-скан даёт надёжную сетку безопасности для тех нарушений, которые машина может найти на каждом PR, без необходимости каждый раз запускать скринридер или ручной аудит.

## How it works

1. Для каждого состояния компонента, уже покрытого визуальным тестом, открывается та же демо-страница через Playwright.
2. `AxeBuilder` из `@axe-core/playwright` внедряет движок axe-core в страницу и запускает анализ.
3. Результат возвращает массив `violations` — найденные нарушения WCAG.
4. Тест утверждает, что `violations` пуст (или содержит только явно задокументированные исключения по конкретным правилам).
5. Исключения не отключают скан целиком, а scoped на одно конкретное правило с пояснением inline.

```mermaid
flowchart LR
  A[Демо-страница] --> B[AxeBuilder]
  B --> C[analyze]
  C --> D[violations]
  D --> E[expect violations.toEqual([])]
```

### Что такое `violations`

`results.violations` — это массив объектов, каждый из которых описывает одно правило WCAG, которое не выполнилось. Основные поля:

- `id` — идентификатор правила, например `color-contrast`, `label`, `aria-required-attr`, `region`.
- `impact` — серьёзность: `minor`, `moderate`, `serious`, `critical`.
- `description` — что именно нарушено.
- `help` — краткое пояснение, например: «Элементы должны иметь достаточный контраст цвета».
- `helpUrl` — ссылка на документацию axe-core по этому правилу.
- `nodes` — массив DOM-элементов, где нарушение найдено. Каждый элемент содержит `target` (селектор), `html` (фрагмент разметки) и `failureSummary` (почему именно этот элемент не проходит).

Пример:

```json
[
  {
    "id": "color-contrast",
    "impact": "serious",
    "description": "Элементы должны иметь достаточный контраст цвета",
    "help": "Элементы должны иметь достаточный контраст цвета",
    "helpUrl": "https://dequeuniversity.com/rules/axe/4.9/color-contrast",
    "nodes": [
      {
        "target": ["button[type=\"button\"]"],
        "html": "<button>Save</button>",
        "failureSummary": "Fix any of the following:\n  Element has insufficient color contrast of 2.1 (foreground color: #ffffff, background color: #aaaaaa, font size: 12.0pt, font weight: normal). Expected contrast ratio of 4.5:1"
      }
    ]
  }
]
```

`expect(results.violations).toEqual([])` — это проверка, что массив пуст. Если axe-core ничего не нашёл, `violations` равен `[]` и тест проходит. Если нашёл хотя бы одно нарушение, тест падает, и в выводе CI видно правило, затронутые элементы и рекомендацию по исправлению.

### Как работает `AxeBuilder`

```typescript
import AxeBuilder from '@axe-core/playwright';

const results = await new AxeBuilder({ page }).analyze();
```

`AxeBuilder({ page })` связывает axe-core с уже открытой Playwright-страницей. `analyze()` внедряет JavaScript движок axe в страницу, сканирует DOM и возвращает объект с `violations`, `passes`, `incomplete`, `inapplicable`. Нас интересует именно `violations`, потому что только там собраны реальные ошибки.

## How it is structured

- **Demo/preview page** — та же стабильная страница, что используется для визуальных скриншотов.
- **Accessibility spec** — `spec/{component-name}.a11y.spec.ts`, по одному рядом с `spec/{component-name}.visual.spec.ts`.
- **AxeBuilder** — обёртка `@axe-core/playwright`, внедряющая axe-core в страницу.
- **Violations** — массив нарушений с правилом, затронутым элементом и рекомендацией по исправлению.
- **Scoped exceptions** — разрешённые нарушения только конкретного правила, задокументированные в коде.
- **CI gate** — любое неожиданное нарушение ломает сборку.

## Example

```typescript
// File: projects/design-system/src/lib/ds-button/spec/ds-button.a11y.spec.ts
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('DsButtonComponent — accessibility', () => {
  test('has no automatically detectable violations (default)', async ({ page }) => {
    await page.goto('/ds-button/default');
    const results = await new AxeBuilder({ page }).analyze();
    expect(results.violations).toEqual([]);
  });
});
```

## Related concepts

- [Behavioral component testing](skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/glossary/behavioral-component-testing.md) — использует доступные роли/метки, но не исчерпывает WCAG.
- [Visual regression testing](skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/glossary/visual-regression-testing.md) — проверяет внешний вид, но не контраст и ARIA.
- [Style-snapshot testing](skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/glossary/style-snapshot-testing.md) — фиксирует вычисленные стили, но не правила доступности.

## Sources

- [axe-core for Playwright — npm](https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright)
- [Playwright — Accessibility Testing](https://playwright.dev/docs/accessibility-testing)
- [Deque — axe-core rules](https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md)
- [ADR: accessibility-testing-approach](skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/adr/accessibility-testing-approach.md)
- [Generic pattern для a11y спеков](skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md)
- [solution-ui-testing.skill.md — раздел про accessibility](skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/solution-ui-testing.skill.md)
