# todo

The base plateau in Go with the gRPC API variation point attached: the PlanItems command is
served as `todo.v1.ItemsService/PlanItems`, with server reflection when switched on. Health
endpoints come from the health variation point (`vp-health-in-go`), not from gRPC.

```bash
grpcurl -plaintext -d '{"titles":["buy milk","walk the dog"]}' localhost:9090 todo.v1.ItemsService/PlanItems
```

```bash
make init       # once: module dependencies
make run        # build and serve on :9090 with reflection on
make proto-gen  # regenerate gen/api/ after editing the proto file
make test       # every scenario
```
