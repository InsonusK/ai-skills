# Makefile

Exposes the `test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` targets required by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. Runs against the whole solution, so every test project is covered by one invocation — no per-project target.

Fill and copy to `Makefile` (append the testing block when the repository already has one) — `{solution}` = the solution file name without `.slnx`: [`templates/Makefile`](Makefile)
