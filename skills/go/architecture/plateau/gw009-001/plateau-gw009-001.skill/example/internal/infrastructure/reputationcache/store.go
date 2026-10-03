// Package reputationcache caches reputation lookups in Redis.
package reputationcache

import (
	"context"
	"encoding/json"
	"errors"

	"github.com/redis/go-redis/v9"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
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
