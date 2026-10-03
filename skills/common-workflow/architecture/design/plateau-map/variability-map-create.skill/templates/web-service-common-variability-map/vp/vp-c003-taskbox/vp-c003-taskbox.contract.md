# TaskBox storage contract (VP-C003)

The contract lives in the VP's spec repository: [InsonusK/taskbox-spec — contract/taskbox.contract.md](https://github.com/InsonusK/taskbox-spec/blob/master/contract/taskbox.contract.md), with its conformance feature, the agent skills in `doc/skills/`, and its ADRs in `doc/adr/`.

- **Version:** schema v1, no release tag yet — read `master`. Plateau GW009.001's pre-release copy conforms to `master` @ `a969e6b` (before the Redis decisions of 2026-09-29, which add one idempotency scenario).
- Section numbers (§1–§9) cited by the Outbox and Inbox contracts refer to that file.

Concept and store rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox|VP-C003 TaskBox]].
