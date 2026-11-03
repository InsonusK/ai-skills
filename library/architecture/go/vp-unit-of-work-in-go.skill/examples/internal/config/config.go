// Package config reads the base plateau's own settings from the environment.
// A variation point or an entry point never adds fields here: each reads its
// settings in its own package, so removing it removes its settings too.
package config

import (
	"fmt"
	"log/slog"
	"os"
)

type Config struct {
	// LogLevel is LOG_LEVEL: debug, info (default), warn, or error.
	LogLevel slog.Level
}

func Load() (Config, error) {
	var cfg Config
	if raw := os.Getenv("LOG_LEVEL"); raw != "" {
		if err := cfg.LogLevel.UnmarshalText([]byte(raw)); err != nil {
			return Config{}, fmt.Errorf("config: LOG_LEVEL: %w", err)
		}
	}
	return cfg, nil
}
