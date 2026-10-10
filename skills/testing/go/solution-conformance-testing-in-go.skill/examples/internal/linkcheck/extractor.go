package linkcheck

import (
	"regexp"
	"strings"
)

var linkPattern = regexp.MustCompile(`(?i)https?://[^\s<>"']+`)

// ExtractLinks returns every http(s) link of the text, in order of first
// appearance, each one once.
func ExtractLinks(text string) []string {
	links := []string{}
	seen := map[string]bool{}
	for _, match := range linkPattern.FindAllString(text, -1) {
		link := strings.TrimRight(match, ".,;:!?)")
		if !seen[link] {
			seen[link] = true
			links = append(links, link)
		}
	}
	return links
}
