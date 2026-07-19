# How Apply this template
1. Use this once per glossary root — per [documentation-for-concept.skill.md's "Where the page lives"](../documentation-for-concept.skill.md#where-the-page-lives), that is either `solution-{Name}.skill/glossary/README.md` or `docs/glossary/README.md` — never combine both roots into one index.
2. Add one row per concept page, sorted alphabetically by term.
3. Remove the `hint`/`example` blocks and this section before saving.

# Glossary
```hint
One sentence: this page indexes every term/library/technology/pattern explained for this project, so a reader can find an explanation without knowing which file it lives in.
```

| Term | Page | Covers |
| --- | --- | --- |
```hint
One row per entry: the term, a link to docs/glossary/{slug}.md, and a one-line description precise enough that a reader can pick the right page without opening it.
```
```example
| CQRS | [cqrs.md](./cqrs.md) | Separating read and write models for a service |
| Webhook | [webhook.md](./webhook.md) | HTTP callback used to push events instead of polling |
```
