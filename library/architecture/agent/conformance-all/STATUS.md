# Shared scenarios for every variation point — status

The plan by waves; each wave passes `check.sh`, gets a fresh-eyes audit, and is one commit. Tools: `propagate.py` (base example changes into VP examples), `manifest.py` (inherited manifest lines), `template-sync.sh`, `clean-generated.sh`, `show.sh` (compare a unit across stacks).

| Wave | What | State |
|---|---|---|
| 1 | base-plateau (9 units), vp-grpc-api (rpcstatus, grpc-pipeline, grpc-contract, grpc-items) | done; tests green per stack |
| 2 | vp-http-api (both modes), vp-cli-api, vp-health | todo |
| 3 | vp-http-client, vp-grpc-client, vp-client-retry | todo |
| 4 | base-plateau-extension, vp-logging leftovers, vp-postgres / vp-unit-of-work leftovers | todo |
| 5 | spec skills state the rule + checklist; stack skills; README; full ground run | todo |
