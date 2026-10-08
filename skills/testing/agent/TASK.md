# Task: run the examples — done

The task this file held (W5: run every example in a container with the toolchains, fix what breaks) is finished; `STATUS.md` has the results item by item, `DECISIONS.md` the choices made on the way.

To repeat it after a change to `tools/testing/`, a kind script or a normalizer:

1. `bash skills/testing/agent/check.sh` — copies and examples consistent.
2. `bash skills/testing/agent/run-example.sh {example-dir}` for every example — the Go and .NET plateau examples, and `skills/testing/{python,typescript}/solution-conformance-testing-in-*.skill/example`. It needs the example's toolchain, `jq`, `node` and `python3`.
   - Python: nothing to prepare — the script runs the example's `make init`, which creates `.venv`.
   - `gw009-001`: set `TEST_DATABASE_DSN` to a PostgreSQL the run may write to.
3. Break one scenario → `make test-kind-unit` exits non-zero, `result/scenarios.json` exists, `make test-report` exits 0 and `run.json` shows the kind as `failed`. Revert.
4. Delta mutation on a real change needs a repository whose `HEAD~1` differs in a production file: copy the example out, `git init`, commit twice, `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=HEAD~1`.

Rules while fixing — unchanged:

- Code has one source: a real file under the skill's `assets/` (copied verbatim) or `templates/` (placeholders filled). Change it there **and** in every example copy in the same commit — `check.sh` §6 and §10 fail otherwise.
- Shared by every stack, in `skills/testing/core/solution-conformance-testing.skill/assets/`: `tools/testing/` and `tools/livingdoc/`. Stack-specific: only `tools/testing/kinds/` in the stack skill's `assets/` — and for Go the three normalizers those scripts call.
- A project's `Makefile` carries `include tools/testing/testing.mk` and no testing recipe (`check.sh` §11).
- Do not add a caller-facing variable or target; if the contract itself is wrong, record it in `DECISIONS.md` with ⚠️ and stop for the owner.
