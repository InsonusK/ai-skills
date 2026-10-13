---
description: Add Redis connection settings to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/cached-db
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	RedisHost     string
	RedisPort     string
	RedisPassword string
	RedisDB       int
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		RedisHost:     l.get("REDIS_HOST", "localhost"),
		RedisPort:     l.get("REDIS_PORT", "6379"),
		RedisPassword: l.get("REDIS_PASSWORD", ""),
		RedisDB:       l.getInt("REDIS_DB", 0),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

# Check list
- [ ] `REDIS_HOST`/`REDIS_PORT` default to `localhost`/`6379`; `REDIS_PASSWORD` defaults to empty (no auth).
