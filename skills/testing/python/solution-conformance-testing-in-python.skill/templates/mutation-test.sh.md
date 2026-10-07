# scripts/mutation-test.sh

`mutmut`'s CLI for CI-friendly result export and for scoping a run to specific changed files has moved between major versions more than Stryker.NET/StrykerJS have — treat every `mutmut` line below as a sketch to verify against the version this project pins, not a copy-paste command. The surrounding contract (env vars, `$TEST_KIND_DIR/result/mutation-test.json` schema, exit-code propagation) is what must hold regardless of which `mutmut` version/flags end up filling it in — see [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).

Fill and copy to `scripts/mutation-test.sh` — `{package}` = the package's source directory: [`templates/scripts/mutation-test.sh`](scripts/mutation-test.sh)
