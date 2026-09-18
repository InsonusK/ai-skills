---
name: plateau-persistent-service--file-infrastructure-reputationcache-store
description: internal/infrastructure/reputationcache/store.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/infrastructure/reputationcache/store.go
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
---

# Goal
Implement `ReputationCache` using Redis, translating `redis.Nil` into a plain miss.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/store.go.create.md|store.go]]

# Core Principles
- Apply ONE plateau template per file.
- Every key is namespaced with a fixed prefix.

# Implementation
```go
// Skill: file-infrastructure-reputationcache-store
// Plateau: plateau-persistent-service
// Version: 20260917040000

package reputationcache

import (
	"context"
	"encoding/json"
	"errors"

	"github.com/redis/go-redis/v9"

	"{module-path}/internal/domain/interfaces"
)

type Store struct {
	client *redis.Client
}

func New(addr, password string, db int) *Store {
	return &Store{client: redis.NewClient(&redis.Options{
		Addr:     addr,
		Password: password,
		DB:       db,
	})}
}

func (s *Store) Close() error {
	return s.client.Close()
}

func (s *Store) Get(ctx context.Context, url string) (interfaces.Reputation, bool, error) {
	raw, err := s.client.Get(ctx, cacheKey(url)).Bytes()
	if errors.Is(err, redis.Nil) {
		return interfaces.Reputation{}, false, nil
	}
	if err != nil {
		return interfaces.Reputation{}, false, err
	}
	var rep interfaces.Reputation
	if err := json.Unmarshal(raw, &rep); err != nil {
		return interfaces.Reputation{}, false, err
	}
	return rep, true, nil
}

func (s *Store) Set(ctx context.Context, url string, rep interfaces.Reputation) error {
	raw, err := json.Marshal(rep)
	if err != nil {
		return err
	}
	return s.client.Set(ctx, cacheKey(url), raw, 0).Err()
}

func cacheKey(url string) string { return "reputation:" + url }
```
Verified against this plateau's own `example/internal/infrastructure/reputationcache/store.go`, against a real Redis instance (not mocked): 3 HTTP requests for the same URL produced exactly one call to the (throwaway, test-only) fake reputation server, confirmed both by that server's own call log and by `redis-cli get` returning the cached JSON value; a gRPC request for the same URL immediately afterward hit the cache too (same domain-service instance, same cache), confirming the cache is shared across transports.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/store.go.create.md|store.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Get` must translate `redis.Nil` to `(zero value, false, nil)` — never propagate it as `err`.
- Namespace every key with a fixed prefix.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/store.go.create.md#MUST|store.go]]

# Check list
- [ ] `Get` never returns `redis.Nil` as its `error` value.
- [ ] Every key goes through `cacheKey`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/store.go.create.md|store.go]]

# Unittest TestCases
- [ ] WHEN the key is absent THEN `Get` returns `(_, false, nil)`
- [ ] WHEN `Set` then `Get` round-trips a value THEN the returned value equals what was set

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/store.go.create.md|store.go]]
