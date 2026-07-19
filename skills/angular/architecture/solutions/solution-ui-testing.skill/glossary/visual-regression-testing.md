# Visual regression testing

**Visual regression testing** (визуальное регрессионное тестирование) — это автоматическая проверка, что UI-компонент выглядит так же, как раньше, путём сравнения текущего скриншота его демо-страницы с сохранённым базовым изображением.

## Why it exists

DOM-тесты умеют проверять текст, роли и события, но не видят фактическую картинку: jsdom/happy-dom не имеет движка рендеринга и не может сказать, что сломалась вёрстка, неправильно применилась тёмная тема, упал контраст или возник конфликт CSS-специфичности. Визуальное регрессионное тестирование закрывает этот пробел, фиксируя внешний вид компонента и фейля CI при любом заметном отклонении.

## How it works

1. Для каждого значимого состояния компонента создаётся стабильная демо-страница (`/ds-button/default`, `/ds-button/disabled` и т. п.).
2. Playwright открывает страницу напрямую по этому URL.
3. При необходимости эмулируется цветовая схема (`light`/`dark`).
4. Тест вызывает `expect(page).toHaveScreenshot(...)`, который сравнивает текущий скриншот с сохранённым baseline.
5. Если пиксельная разница превышает порог, CI падает; baseline обновляется только осознанно через `--update-snapshots` и ревью PR.

```mermaid
flowchart LR
  A[Демо-страница компонента] --> B[Playwright page.goto]
  B --> C[emulateMedia light/dark]
  C --> D[expect.toHaveScreenshot]
  D --> E[Сравнение с baseline PNG]
```

## How it is structured

- **Demo/preview page** — минимальное приложение, которое рендерит компонент в одном состоянии с фиксированными данными: `apps/component-preview` (платформа) или `projects/demo` (design system).
- **Visual spec** — `.visual.spec.ts`, по одному на компонент, со скриншотами каждого состояния и цветовой схемы.
- **Baseline image** — коммитится в репозиторий рядом со спеком как эталон.
- **Diff threshold** — настраивается в Playwright; отсеивает незначительный шум, но остаётся чувствительным к реальным регрессиям.
- **Update workflow** — baseline меняется только как часть намеренного визуального изменения, после просмотра диффа.

## Example

```typescript
import { test, expect } from '@playwright/test';

test.describe('DsButtonComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/ds-button/default');
      await expect(page).toHaveScreenshot(`ds-button-default-${scheme}.png`);
    });
  }
});
```

## Related concepts

- [Behavioral component testing](./behavioral-component-testing.md) — проверяет поведение, но не видит картинку.
- [Style-snapshot testing](./style-snapshot-testing.md) — объясняет, какие именно CSS-свойства изменились, когда визуальный тест падает.
- [Accessibility testing](./accessibility-testing.md) — ловит WCAG-нарушения, которые скриншот не проверяет.

## Sources

- [Playwright — Test Snapshots](https://playwright.dev/docs/test-snapshots)
- [Playwright — Screenshot Assertions](https://playwright.dev/docs/api/class-page#page-screenshot)
- [ADR: visual-regression-approach](../adr/visual-regression-approach.md)
- [Generic pattern для визуальных спеков в решении](../Implementation/Testing/{component-name}.visual.spec.ts.create.md)
- [solution-ui-testing.skill.md — описание визуального слоя](../solution-ui-testing.skill.md)
