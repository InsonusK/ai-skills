# scripts/mutation-test.sh

Runs Stryker.NET — its native `--since` mode covers a `check` run's `DELTA_BASE` directly, so this script does not need to compute the diff itself. See [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] for the target contract.

Copy verbatim to `scripts/mutation-test.sh`: [`assets/scripts/mutation-test.sh`](../assets/scripts/mutation-test.sh)
