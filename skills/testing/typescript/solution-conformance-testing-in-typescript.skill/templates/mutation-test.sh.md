# scripts/mutation-test.sh

StrykerJS has no native `--since`/delta flag the way Stryker.NET does, so this script emulates a `check` run's `DELTA_BASE` itself by limiting `--mutate` to the files `git diff` reports as changed. See [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract) for the target contract.

Copy verbatim to `scripts/mutation-test.sh`: [`assets/scripts/mutation-test.sh`](../assets/scripts/mutation-test.sh)
