// Source: skill vp-cli-api-in-go

package pipeline

import (
	"context"
	"log/slog"
	"time"

	"github.com/spf13/cobra"
	"github.com/spf13/pflag"

	"github.com/example/todo/internal/logging"
)

// logReceived writes the first record of a run once its arguments are parsed
// - "cli call received" with the action and the flags given as the payload.
func logReceived(cmd *cobra.Command, logger *slog.Logger) {
	logger.LogAttrs(cmd.Context(), slog.LevelInfo, "cli call received",
		slog.String("action", cmd.CommandPath()), logging.Payload(map[string]any{"flags": givenFlags(cmd)}))
}

// logCall writes the last record of a run - "cli call answered" with the
// action run, the duration, failed, and as the payload the output as printed;
// a failed run repeats the flags given, so they are in the log even when only
// failed records keep their payload, and even when the arguments could not be
// parsed and no "cli call received" was written.
func logCall(ctx context.Context, logger *slog.Logger, ran *cobra.Command, start time.Time, err error, output string) {
	sent := map[string]any{"output": output}
	if err != nil {
		sent["flags"] = givenFlags(ran)
	}
	logger.LogAttrs(ctx, slog.LevelInfo, "cli call answered",
		slog.String("action", ran.CommandPath()), slog.Duration("duration", time.Since(start)),
		logging.Failed(err != nil), logging.Payload(sent))
}

// givenFlags are the flags given on the command line, by name.
func givenFlags(cmd *cobra.Command) map[string]string {
	flags := map[string]string{}
	cmd.Flags().Visit(func(f *pflag.Flag) { flags[f.Name] = f.Value.String() })
	return flags
}
