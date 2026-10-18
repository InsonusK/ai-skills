# todo

The base plateau in Go with the gRPC API variation point attached: the PlanItems command is
served as `todo.v1.ItemsService/PlanItems`, with the standard health service and, when
switched on, server reflection.

```bash
grpcurl -plaintext -d '{"titles":["buy milk","walk the dog"]}' localhost:9090 todo.v1.ItemsService/PlanItems
grpcurl -plaintext -d '{"service":"readiness"}' localhost:9090 grpc.health.v1.Health/Check
```

```bash
make init       # once: module dependencies
make run        # build and serve on :9090 with reflection on
make proto-gen  # regenerate gen/api/ after editing the proto file
make test       # every scenario
```
