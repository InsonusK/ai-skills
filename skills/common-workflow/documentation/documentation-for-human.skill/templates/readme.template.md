# How Apply this template
1. Decide the shape first, following [documentation-for-human.skill.md's "One page or a page group?"](skills/common-workflow/documentation/documentation-for-human.skill/documentation-for-human.skill.md#one-page-or-a-page-group):
   - **Single reference page** — link `## Documentation` to one `docs/api/reference.md`.
   - **Page group** — link `## Documentation` to one row per domain page (for example, `docs/api/auth.md`, `docs/api/billing.md`), each built from [api-reference-group.template.md](skills/common-workflow/documentation/documentation-for-human.skill/templates/api-reference-group.template.md).
2. Fill every section below with real content about the project.
3. Keep installation/setup here (or in an attached `docs/installation.md` linked from here) — never duplicate it inside a domain page.
4. Remove all `hint` and `example` blocks, and this `# How Apply this template` section, before saving as `README.md`.

# {Project Name}
```hint
Project title and a one-sentence description of what it does.
```

## Why
```hint
What problem the project solves and why a reader would use it.
```

## Installation
```hint
Quick, copy-pasteable install command(s). Link to docs/installation.md for platform-specific or detailed setup instead of inlining it here. Follow the rules in installation.md.
```
```example
Requires Python 3.10+.

```bash
pip install mylib==1.2.3
```

See `docs/installation.md` for platform-specific notes.
```

## Quick start
```hint
One complete, runnable usage example a reader can copy and paste to see the project work end to end.
```

## Documentation
```hint
For the single reference page shape: link directly to docs/api/reference.md. Use the worked example at [examples/simple_skill/docs/api/reference.md](../examples/simple_skill/docs/api/reference.md) as a model.
For the page group shape: one row per functional domain, each linking to its docs/api/<domain>.md page, with a short description precise enough that a reader can pick the right page without opening it. Use the worked example at [examples/complex_skill/docs/api/](../examples/complex_skill/docs/api/) as a model.
```
```example
| Topic   | Docs                      | Covers                        |
| ------- | ------------------------- | ------------------------------ |
| Auth    | `docs/api/auth.md`        | Login, token refresh, logout   |
| Billing | `docs/api/billing.md`     | Charges, refunds, invoices |
```
