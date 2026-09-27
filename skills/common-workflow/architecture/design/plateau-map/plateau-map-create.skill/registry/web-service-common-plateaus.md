# Web-service common plateau registry

The `{common}` part of every backend web-service plateau code (`{stack}W{common}.{specific}`), per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md#code-every-plateau|plateau-map-create — Code every plateau]]. One row per combination of 📐 common-VP Variants that at least one stack has built; columns follow the 📐 rows of the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common Variability Map]].

- A number is assigned once, to the next combination first built by any stack, and never reused.
- When a common VP reaches 📐, its column is added and filled for every row with the Variant that row's plateaus realize; a row whose plateaus now disagree splits, and the plateaus that moved get the next free number (recoded in the same change).
- Stack cell: ✅ `{codes}` — built here with its example; 🔸 — built only in another stack.
- A row left with no plateau in any stack is removed; its number goes to **Retired numbers**.

| No. | VP-C001 PersistentStore | VP-C002 TransientStore | VP-C003 TaskBox | VP-C004 HttpOutbound | VP-C005 GrpcOutbound | VP-C006 KafkaProducer | VP-C007 KafkaConsumer | VP-C008 RabbitMqProducer | VP-C009 RabbitMqConsumer | VP-C010 Outbox | Go | dotnet |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 001 | None | None | No | No | No | No | No | No | No | No | ✅ GW001.000, GW001.001 | ✅ DW001.000 |
| 005 | None | None | No | No | Yes | No | No | No | No | No | ✅ GW005.001 | 🔸 |
| 006 | None | Redis | No | No | Yes | No | No | No | No | No | ✅ GW006.001 | 🔸 |
| 007 | PostgreSQL | Redis | No | No | Yes | No | No | No | No | No | ✅ GW007.001 | 🔸 |
| 008 | PostgreSQL | None | No | No | Yes | No | No | No | No | No | 🔸 | ✅ DW008.003, DW008.004 |

Retired numbers (never reused): 002, 003, 004 — their plateaus moved to 005–008 when outbound gRPC became common VP-C005.
