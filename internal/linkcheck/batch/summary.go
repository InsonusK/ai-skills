// Package batch checks many URLs at once and totals the outcome.
package batch

import "github.com/example/linkcheck/internal/linkcheck"

// Summary counts the valid URLs, the invalid ones, and each error code.
type Summary struct {
	Valid   int
	Invalid int
	Errors  map[string]int
}

// Summarize checks every URL and totals the results.
func Summarize(urls []string) Summary {
	summary := Summary{Errors: map[string]int{}}
	for _, url := range urls {
		result := linkcheck.Check(url)
		if result.IsValid {
			summary.Valid++
			continue
		}
		summary.Invalid++
		summary.Errors[result.ErrorCode]++
	}
	return summary
}

// A change of code: the pull request must run the tests and need no version bump into develop.
