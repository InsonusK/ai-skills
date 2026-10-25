package access

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

// DefaultMaxBatch is the batch limit when ACCESS_MAX_BATCH is not set.
const DefaultMaxBatch = 100

// Config is the access variation point's own settings.
type Config struct {
	// Tokens maps a bearer token to its caller: ACCESS_TOKENS, a comma-separated
	// list of token=user or token=user:role+role, e.g.
	// "t-alice=alice:planner,t-bob=bob". Empty by default: no caller is known.
	Tokens map[string]Identity
	// MaxBatch is the most items a command marked batchlimit.Batch may hold:
	// ACCESS_MAX_BATCH, default DefaultMaxBatch.
	MaxBatch int
}

// LoadConfig reads the settings from the environment; an unusable value is an
// error naming its variable.
func LoadConfig() (Config, error) {
	cfg := Config{Tokens: map[string]Identity{}, MaxBatch: DefaultMaxBatch}
	if raw := os.Getenv("ACCESS_TOKENS"); raw != "" {
		for _, item := range strings.Split(raw, ",") {
			token, caller, found := strings.Cut(strings.TrimSpace(item), "=")
			user, roles, _ := strings.Cut(caller, ":")
			if !found || token == "" || user == "" {
				return Config{}, fmt.Errorf("access config: ACCESS_TOKENS item %q is not token=user[:role+role]", item)
			}
			identity := Identity{User: user}
			if roles != "" {
				identity.Roles = strings.Split(roles, "+")
			}
			cfg.Tokens[token] = identity
		}
	}
	if raw := os.Getenv("ACCESS_MAX_BATCH"); raw != "" {
		limit, err := strconv.Atoi(raw)
		if err != nil || limit < 1 {
			return Config{}, fmt.Errorf("access config: ACCESS_MAX_BATCH %q is not a positive number", raw)
		}
		cfg.MaxBatch = limit
	}
	return cfg, nil
}
