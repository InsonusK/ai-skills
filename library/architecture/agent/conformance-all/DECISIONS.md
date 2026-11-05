# Shared scenarios for every variation point — decisions

One line per choice; ⚠️ marks a fork with real trade-offs, decided by the owner.

- ⚠️ 2026-11-04, owner: every spec skill gets `conformance/`: base-plateau, base-plateau-extension, vp-cli-api, vp-grpc-api, vp-http-api, vp-health, vp-http-client, vp-grpc-client, vp-client-retry; the missing scenarios of vp-postgres and vp-unit-of-work join theirs.
- ⚠️ 2026-11-04, owner: the stacks' texts merge by union, the stricter check wins; a stack that fails a merged scenario gets its code fixed.
- ⚠️ 2026-11-04, owner: vp-cli-api is not applied to dotnet; its scenarios run in Go and Python only.
- ⚠️ 2026-11-04, owner: one pull request into `arch-new-vp`, branch `arch-conformance-all`.
- 2026-11-04, agent: this supersedes error-shape's "no shared `conformance/` folder is added for the base plateau's log".
- 2026-11-04, agent: one folder per unit under `conformance/`, copied by one line of `conformance.copy` each; no `include` in the manifest, because a VP deletes some base units (`startup`) and an include could not leave them out.
- 2026-11-04, agent: a stack's own scenario file is named `{name}-in-{stack}.feature`; the existing ones (`kind_in_go.feature`, `go-payload.feature`, …) are renamed to it.
- 2026-11-05, agent: a scenario value is the output every stack writes (log level `INFO`, kind `not found`, bool `true`); dotnet's steps map their enum names to it.
- 2026-11-05, agent: the base log's "a failure of the service is written with its stack" is the stack's own in Python and dotnet (`json-log-in-{stack}.feature`): a Go error has no stack to write.
- 2026-11-05, agent: Go's "a middleware that passes on a value of another type" and dotnet's "a type that is no open generic behavior" / "no interceptor" are the stack's own: each tests a mechanism only that stack has.
- 2026-11-05, agent: the command logging scenarios log a command with a title (`{"title":"buy milk"}`), as dotnet did; Go and Python test with the same command shape. dotnet's recording logger writes through `JsonLogLine`, so every stack compares the JSON line.
- 2026-11-05, agent: the gRPC call log writes a code by its canonical name (`INVALID_ARGUMENT`) in every stack, as Python did; Go and dotnet wrote their own spelling (`InvalidArgument`), so one log pipeline read two names for one code.
- 2026-11-05, agent: Go's gRPC answers a failure of no kind INTERNAL and logs it as `grpc call failed` with its error, as Python and dotnet did; before, `Call` turned the error into a status itself and the cause never reached the log.
- 2026-11-05, agent: every stack rejects port 0 (`GRPC_LISTEN_PORT=0`), as dotnet did.
- 2026-11-05, agent: `rpcstatus` is owned by vp-grpc-api; vp-http-api's examples name it in their manifest.
- 2026-11-05, agent: vp-grpc-api's endpoint scenarios are two units, `grpc-contract` and `grpc-items`: the proto mode of vp-http-api extends ItemsService and keeps an `items.feature` of its own, and shares the contract.
- 2026-11-05, agent: the generic steps of the gRPC scenarios name gRPC (`the gRPC call should …`), as dotnet's did, so the HTTP steps of the same dotnet test project never collide with them.
