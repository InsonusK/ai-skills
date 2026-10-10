-- +goose Up
CREATE TABLE IF NOT EXISTS link_checks (
	id SERIAL PRIMARY KEY,
	normalized TEXT NOT NULL,
	flagged BOOLEAN NOT NULL,
	reason TEXT NOT NULL,
	checked_at TIMESTAMPTZ NOT NULL
);

-- +goose Down
DROP TABLE IF EXISTS link_checks;
