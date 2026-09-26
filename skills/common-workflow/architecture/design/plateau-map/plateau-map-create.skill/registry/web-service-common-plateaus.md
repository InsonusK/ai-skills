# Web-service common plateau registry

The `{common}` part of every backend web-service plateau code (`{stack}W{common}.{specific}`), per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md#code-every-plateau|plateau-map-create — Code every plateau]]. One row per combination of 📐 common-VP Variants that at least one stack has built; columns follow the 📐 rows of the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common Variability Map]].

- A number is assigned once, to the next combination first built by any stack, and never reused.
- When a common VP reaches 📐, its column is added and filled for every row with the Variant that row's plateaus realize; a row whose plateaus now disagree splits, and the plateaus that moved get the next free number (recoded in the same change).
- Stack cell: ✅ `{codes}` — built here with its example; 🔸 — built only in another stack.

| No. | VP-C001 PersistentStore | VP-C002 TransientStore | Go | dotnet |
| --- | --- | --- | --- | --- |
