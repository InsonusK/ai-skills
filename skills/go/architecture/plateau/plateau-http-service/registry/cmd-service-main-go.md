---
name: registry-cmd-service-main-go
description: Conflict Detection result for the `cmd-service-main-go` element
tags:
  - concern/architecture
  - stack/go
  - element/cmd-service-main-go
---

# Element
`cmd-service-main-go`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] (`.extend`)
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] (`.extend`)

# Classification
`FMN` — **F**: no Constraint between these three VPs (all common-baseline, none gates another). **M**: each delta changes `run()`'s body. **N**: independent — read side by side, `solution-go-app-logging`'s delta only inserts one call (`logging.Init(cfg.LogLevel)`) at a stated position; `solution-go-http-api`'s delta only adds the HTTP-server construct/serve/shutdown block after that. Neither delta touches a line the other one touches.

# Ordering
`source: ordering-only` — no Feature-Model constraint requires `solution-go-app-logging` before `solution-go-http-api`; the order is required only so the merged `run()` reads correctly (`logging.Init` must run before anything that might log, including the HTTP server's own startup log line). Both deltas state this explicitly in their own `# Rule changes`/`# Rule` sections — `solution-go-app-logging`'s Implementation file says `logging.Init` "must be the first call in `run()` after `config.Load()` returns successfully"; `solution-go-http-api`'s delta is written to come after it.

# Resolution
Canonical — no resolver needed. The three deltas are read together and hand-merged into one `run()` at plateau-assembly time (this plateau's own `example/cmd/linkcheck/main.go`, and the summary in [[../structure/plateau-http-service--file-cmd-service-main.skill.md|the file-tier skill]]); each delta's own instructions are sufficient to produce the correct merge without inventing new logic.

# Architectural signal
N=3 at this plateau already, and every VP-realizing solution this catalog defines further (`solution-grpc-api`, `solution-external-integration`, `solution-cached-db`, `solution-persistent-db`, `solution-kafka-consumer`) also extends this same element (see the catalog-level classification in `skills/go/architecture/agent/DECISIONS.md`, which found N=7 across the whole catalog). This is a real signal that "the composition root" is this family's single widest intersection surface, not a coincidence of this one plateau — worth remembering if a future VP's wiring turns out to need more than "insert one construct+wire block," since every other solution touching this element currently assumes that shape.

