# Accessibility testing

**Accessibility testing** (тестирование доступности) — это автоматическая проверка UI-компонента на соответствие механически проверяемым правилам WCAG: контраст, корректные ARIA-атрибуты, наличие меток у элементов управления и структура заголовков/ориентиров.

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

## How it is structured

- **Demo/preview page** — та же стабильная страница, что используется для визуальных скриншотов.
- **Accessibility spec** — `.a11y.spec.ts`, по одному рядом с `.visual.spec.ts`.
- **AxeBuilder** — обёртка `@axe-core/playwright`, внедряющая axe-core в страницу.
- **Violations** — массив нарушений с правилом, затронутом элементом и рекомендацией по исправлению.
- **Scoped exceptions** — разрешённые нарушения только конкретного правила, задокументированные в коде.
- **CI gate** — любое неожиданное нарушение ломает сборку.

## Example

```typescript
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

- [Behavioral component testing](./behavioral-component-testing.md) — использует доступные роли/метки, но не исчерпывает WCAG.
- [Visual regression testing](./visual-regression-testing.md) — проверяет внешний вид, но не контраст и ARIA.
- [Style-snapshot testing](./style-snapshot-testing.md) — фиксирует вычисленные стили, но не правила доступности.

## Sources

- [axe-core for Playwright — npm](https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright)
- [Playwright — Accessibility Testing](https://playwright.dev/docs/accessibility-testing)
- [ADR: accessibility-testing-approach](../adr/accessibility-testing-approach.md)
- [Generic pattern для a11y спеков](../Implementation/Testing/{component-name}.a11y.spec.ts.create.md)
- [solution-ui-testing.skill.md — раздел про accessibility](../solution-ui-testing.skill.md)
