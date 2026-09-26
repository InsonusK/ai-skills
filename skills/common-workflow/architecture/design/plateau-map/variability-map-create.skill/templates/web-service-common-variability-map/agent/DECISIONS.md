# Decisions log

One line per non-mechanical choice. ⚠️ = a genuine architectural fork, waiting on the owner.

## Owner-decided (2026-09-26, chat)

- TaskBox is the deferred-execution mechanism; Outbox is the policy "outbound calls go through TaskBox". One VP for TaskBox, its store realization depends on where the data lives (Redis data → Redis TaskBox, PostgreSQL data → PostgreSQL TaskBox).
- Redis TaskBox with Redis-resident data is a real outbox (same-store atomic write); no Redis library enqueues inside the caller's `MULTI` generally, so Redis is a shared contract + thin per-stack implementation.
- Common VPs are inherited by every bound stack map under a `VP-C###` ID; the row is always present, even when the stack adds nothing.

## Agent decisions

- Template folder renamed `web-service-variability-map` → `web-service-common-variability-map`, symmetric with `web-service-common-features`. Only 2 inbound links to fix.
- Stack common-VP rows restate nothing the common map owns (question/Variants/Constraint read through the ID link) — duplication is the drift source being removed.
- Stack-local VPs keep their existing `VPn` IDs; only VPs that become common are re-IDed. Minimises churn in dotnet's ~130 referencing files.
- Per-protocol VPs stay separate booleans (C006–C013), per `variability-map-create`'s "combinables split" rule; dotnet already has this shape.
- Earlier chat answer said TaskBox variants were "At least one (Redis/PostgreSQL)". Corrected: that would be a combinable multi-variant VP, which the skill's "combinables split" rule forbids. TaskBox is Yes/No; *which store* is a Realization-depends-on consequence of the storage VPs — matches the owner's own framing ("if Redis → library X, if Persistent → library Y").
- Harness files live in `agent/` beside the template, matching the go/dotnet catalogs' precedent.

## ⚠️ Open forks

- ⚠️ **F1 — DomainLogic as a common VP.** dotnet treats it as a VP (a pass-through module has no domain layer); Go and the common feature template treat it as baseline. *Recommend:* make it common VP-C001; Go records `Fixed: Yes`. Alternative: keep it dotnet-local — then the common feature template's "DomainLogic is common" stands and dotnet contradicts it silently.
- ⚠️ **F2 — Drop the Critical/NonCritical guarantee VPs.** With the owner's rule "the task lives in the store of its data", the guarantee is fully determined by that store (PostgreSQL → durable, Redis → best-effort) — no team can pick it independently, so it fails the "two teams, two answers" test. *Recommend:* drop both VPs and the `MessageSendGuarantee` subtree from the feature template; state the guarantee in C014's Realization depends on. Alternative: keep them as documentation-only VPs (always derivable → noise, and a place to drift).
- ⚠️ **F3 — `Fixed: {Variant}` replaces the agreed `N/A`.** Go's `HttpApi` is common baseline — the answer is always *Yes*, which "N/A" cannot express. `Fixed: No` covers the old N/A. *Recommend:* three states Inherited / Refined / Fixed.
- ⚠️ **F4 — dotnet's domain-modelling VPs stay stack-local for v1.** ValueObjects, SharedRules, EntityConcurrencyControl, ExternalIdentity, AuditTimestamps are conceptually stack-agnostic, but no second stack realizes or even models them. *Recommend:* keep local now, promote when a second stack needs one (promotion = re-ID per INVARIANTS §2). Alternative: promote now; Go gets five `deferred` rows.
- ⚠️ **F5 — Go `ExternalIntegration` → VP-C011 GrpcOutbound.** Go's owner kept it as one transport-agnostic feature, but its only realization is gRPC. *Recommend:* map to C011; C010 HttpOutbound becomes `deferred` in Go. Alternative: a Go-local VP — then Go's outbound calls escape the common map, which is the drift this task exists to stop.
