package linkcheck

import (
	"fmt"
	"io"
)

// Run checks command arguments and writes human-readable results.
func Run(args []string, out, errOut io.Writer) int {
	if len(args) == 0 {
		fmt.Fprintln(errOut, "usage: linkcheck URL [URL ...]")
		return 2
	}
	code := 0
	for _, input := range args {
		result := Check(input)
		if result.IsValid {
			fmt.Fprintln(out, "ok", result.Normalized)
		} else {
			fmt.Fprintln(out, "invalid", input, result.ErrorCode)
			code = 1
		}
	}
	return code
}
