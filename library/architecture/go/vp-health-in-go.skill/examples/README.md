# todo

The base plateau in Go with the health variation point attached: beside the startup command,
the management entry point answers `/health/live`, `/health/ready`, and `/health` on its own
port (`MANAGEMENT_LISTEN_PORT`, default 8081). The startup command is one-shot, so the process
still ends when the command is done, and the management server stops with it; in a service an
API entry point keeps the process - and the health endpoints - running.

```bash
curl localhost:8081/health/ready   # while a long-running entry point keeps the process up
```

```bash
make init    # once: module dependencies
make run     # build and run the startup command beside the health endpoints
make test    # every scenario
```
