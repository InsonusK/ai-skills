---
description: Shape-only consumer loop added to the errgroup
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/kafka-consumer
  - element/cmd-service-main-go
---

> Draft contract — shape only.

# Implementation changes
```go
g.Go(func() error {
	return consumer.Run(ctx) // reacts to messages, calling the domain service per message
})
```

# Check list
- [ ] A real application of this solution replaces `consumer.Run` with the chosen Kafka client library's actual consume loop.
