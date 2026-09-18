---
description: Store struct — Redis-backed implementation of the domain's cache port
project_name: "internal/infrastructure/{cache}"
name: Store
element_kind: struct
change_kind: create
tags:
  - solution/cached-db
  - element/internal-infrastructure-cache-store-go
---

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------- | --------- |
| the adapter | `Store` | `Store` | `store.go` | `store.go` |

# Implementation changes
```go
// Package {cache} caches {concept} lookups in Redis.
package {cache}

import (
	"context"
	"encoding/json"
	"errors"

	"github.com/redis/go-redis/v9"
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

func (s *Store) Get(ctx context.Context, key string) ({Value}, bool, error) {
	raw, err := s.client.Get(ctx, cacheKey(key)).Bytes()
	if errors.Is(err, redis.Nil) {
		return {Value}{}, false, nil
	}
	if err != nil {
		return {Value}{}, false, err
	}
	var v {Value}
	if err := json.Unmarshal(raw, &v); err != nil {
		return {Value}{}, false, err
	}
	return v, true, nil
}

func (s *Store) Set(ctx context.Context, key string, value {Value}) error {
	raw, err := json.Marshal(value)
	if err != nil {
		return err
	}
	return s.client.Set(ctx, cacheKey(key), raw, 0).Err()
}

func cacheKey(key string) string { return "{cache}:" + key }
```

This catalog's own runnable examples concretize this as `reputationcache.Store` implementing `interfaces.ReputationCache` — see `plateau-cached-service`'s `example/`.

# Rule changes

## MUST
- `Get` must translate `redis.Nil` to `(zero value, false, nil)` — never propagate it as `err`.
  - Risk: the caller in `internal/domain/services` would have to import `github.com/redis/go-redis/v9` to recognize the miss, defeating the port's purpose.
  - Fix: `errors.Is(err, redis.Nil)` check first, exactly as shown.
- Namespace every key with a fixed prefix (`cacheKey`).
  - Risk: an unprefixed key can collide with a key another cache or store writes to the same Redis instance/database.
  - Fix: prefix every key this adapter writes or reads.

# Check list
- [ ] `Get` never returns `redis.Nil` as its `error` value.
- [ ] Every key this adapter touches goes through `cacheKey`.

# Unittest TestCases
- [ ] WHEN the key is absent THEN `Get` returns `(_, false, nil)`
- [ ] WHEN `Set` then `Get` round-trips a value THEN the returned value equals what was set
