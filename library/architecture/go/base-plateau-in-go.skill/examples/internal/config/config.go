// Package config reads the base plateau's own settings from the environment.
// A variation point never adds fields here: it reads its settings in its own package.
package config

import (
	"fmt"
	"log/slog"
	"os"
	"strings"
)

type Config struct {
	LogLevel slog.Level
	// StartupTitles is the input of the startup command: STARTUP_TITLES, comma-separated.
	StartupTitles []string
}

func Load() (Config, error) {
	var cfg Config
	if raw := os.Getenv("LOG_LEVEL"); raw != "" {
		if err := cfg.LogLevel.UnmarshalText([]byte(raw)); err != nil {
			return Config{}, fmt.Errorf("config: LOG_LEVEL: %w", err)
		}
	}
	if raw := os.Getenv("STARTUP_TITLES"); raw != "" {
		cfg.StartupTitles = strings.Split(raw, ",")
	}
	return cfg, nil
}
