# Tasks

The W16 task this file held is done; `STATUS.md` has its results. Two follow-up tasks were opened, independent of each other — each can go to its own agent:

- [`TASK-dotnet-layout.md`](./TASK-dotnet-layout.md) — one folder layout for .NET tests, and the .NET catalog pointing at the testing skill instead of repeating it.
- [`TASK-angular-shapes.md`](./TASK-angular-shapes.md) — Angular testing for a library repository and for an Nx workspace, beside the existing single-application skill. Completed locally on `angular-testing-shapes`; see [verification and decisions](ANGULAR-SHAPES.md).

Each states which decisions are the owner's and which the previous agent took by default; ask the owner about the second kind before building on one.

Common to both — read before starting:

- `AGENTS.md` at the repository root: a worktree of your own, `skills/` as the only source, say so when a decision looks wrong.
- `INVARIANTS.md` here, §5 above all: the contract a caller sees. No new target, no new caller-facing variable.
- Branch `skills-testing` is local, not pushed, no pull request. Branch from it.
- Code has one source: a real file under a skill's `assets/` or `templates/`. Change it there and in every example copy in the same commit; `bash skills/testing/agent/check.sh` fails otherwise.
- `bash skills/testing/agent/run-example.sh {example-dir}` runs one example end to end: a `report` run, a `check` run with caller-chosen directories, delta mutation, the living-doc assertion, a link check. An example that passes it is the proof; a skill text that says so is not.
- The owner reviews by running `make init && make test-and-report` in an example and opening `tmp/testing/report/index.html`.
- Only a person sets `@status/validated`. Remove it from a scenario whose text or steps you change and name that scenario in your report.
- Undo a deliberate breakage from a copy of the file, never with `git checkout` while other changes are uncommitted.
- A full run of every example needs several gigabytes of free disk; the Go build cache alone grew past eight. `go clean -cache` between runs.
- Hand back a message to the owner in Russian that starts with what he can now open and see, then what you decided on your own, then what you could not verify and why.
