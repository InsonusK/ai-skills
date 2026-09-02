# Behavioral component testing

**Behavioral component testing** (поведенческое тестирование компонентов) — это тестирование UI-компонента через отрисованный DOM: вместо проверки внутренних сигналов, полей и `fixture.componentInstance` тест имитирует действия пользователя и проверяет, что на экране появился ожидаемый результат или событие.

## Why it exists

Обычные юнит-тесты Angular часто завязываются на внутреннее устройство компонента — имена полей, сигналов, привязку шаблона. При рефакторинге, который сохраняет пользовательское поведение, такие тесты ломаются «вхолостую». Кроме того, DOM-уровень не требует настоящего бэкенда, роутинга и сложных бизнес-моков, поэтому тесты остаются быстрыми и стабильными. Этот подход также вынуждает авторов использовать доступные роли и метки, что само по себе подталкивает к лучшей доступности.

## How it works

1. Компонент рендерится в изоляции через Angular Testing Library, с фиксированными входами `input()`/`output()`/`model()`.
2. Если компонент напрямую внедряет одного ближайшего сотрудника (Signal Store / Facade), он заменяется простым фейком.
3. Тест находит элементы через `screen.getByRole`, `getByLabelText`, `getByText` — то есть так, как это делал бы пользователь или вспомогательная технология.
4. Взаимодействие происходит через `@testing-library/user-event` (`click`, `type` и т. д.).
5. Проверяется результат: текст, состояние disabled, вызов события или вызов метода замоканного сотрудника.

```mermaid
flowchart LR
  A[Входы: input/output/model] --> B[render + Testing Library]
  B --> C[Запросы DOM: getByRole]
  C --> D[userEvent]
  D --> E[Утверждения по DOM]
```

### Что делает `screen.getByRole('button', { name: /save/i })`

`getByRole` — это query Angular Testing Library, который ищет элемент по его ** accessibility-роли**. Роль — это семантическое назначение элемента: у обычного `<button>` роль `button`, у `<input type="text">` роль `textbox`, у `<nav>` роль `navigation` и т. д. Роль сообщает скринридерам, что это за элемент, поэтому тест, который использует `getByRole`, одновременно проверяет, что компонент правильно раскрывает семантику.

`{ name: /save/i }` — фильтр по **accessible name** (доступное имя). Accessible name — это текст, который вспомогательная технология читает пользователю. Для кнопки обычно это видимый текст внутри кнопки. Регулярное выражение `/save/i` ищет подстроку «save» без учёта регистра (`i`). Если в DOM есть только кнопка с текстом «Save changes», запрос найдёт её; если кнопки нет или у неё нет правильного имени, тест сразу падает с понятной ошибкой.

### Почему `userEvent.click`, а не `element.click()`

`userEvent.click` имитирует реальную цепочку событий указателя: `pointerdown`, `mousedown`, `pointerup`, `mouseup`, `click`. Это позволяет проверить обработку фокуса, `disabled`-состояние, двойные клики и другие поведенческие детали, которые простой `element.click()` может пропустить.

### Почему `expect(pressed).toHaveBeenCalled()`

`pressed` — это шпион (mock-функция), подставленный вместо `(pressed)` output. Такое утверждение проверяет, что компонент действительно эмитнул выходное событие, а не просто изменил внутреннее поле.

## How it is structured

- **Spec file location** — the test file is created at `spec/{component-name}.component.spec.ts`, not next to the implementation.
- **Inputs** — статические входные данные, передаваемые в компонент.
- **Fake collaborator** — минимальный мок для единственного зависимого сервиса, который компонент внедряет напрямую; HTTP, Facade и бэкенд не мокаются.
- **Rendering helper** — `render()` из Angular Testing Library.
- **Queries** — доступные роли/метки, а не `testId` и не `fixture.debugElement`.
- **Interactions** — `userEvent` для кликов, ввода, фокуса.
- **Assertions** — проверка отрисованного DOM и выходных событий.

## Example

```typescript
// File: projects/design-system/src/lib/ds-button/spec/ds-button.component.spec.ts
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { DsButtonComponent } from '../ds-button.component';

describe('DsButtonComponent', () => {
  it('renders its label and reflects the disabled input', async () => {
    await render(DsButtonComponent, { inputs: { label: 'Save', disabled: true } });
    // getByRole('button') ищет кнопку; { name: /save/i } — фильтр по её видимому тексту
    expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
  });

  it('emits (pressed) when clicked', async () => {
    const pressed = vi.fn();
    await render(DsButtonComponent, {
      inputs: { label: 'Save' },
      on: { pressed },
    });

    await userEvent.click(screen.getByRole('button', { name: /save/i }));
    expect(pressed).toHaveBeenCalled();
  });
});
```

## Related concepts

- [Visual regression testing](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/visual-regression-testing.md) — ловит сломанную вёрстку и тёмную тему, которые DOM-тесты не видят.
- [Style-snapshot testing](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/style-snapshot-testing.md) — объясняет, почему визуальный тест сломался.
- [Accessibility testing](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/accessibility-testing.md) — проверяет WCAG-нарушения, которые Testing Library не исчерпывает.

## Sources

- [Angular Testing Library — официальная документация](https://testing-library.com/docs/angular-testing-library/intro/)
- [Which query should I use? — Testing Library](https://testing-library.com/docs/queries/about/#priority)
- [Generic pattern для компонентных тестов в решении](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md)
- [solution-ui-testing.skill.md — общее описание слоёв тестирования](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md)
