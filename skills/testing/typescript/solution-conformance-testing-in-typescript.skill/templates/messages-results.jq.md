# scripts/messages-results.jq

Reduces cucumber-js's `message` formatter output (Cucumber Messages, ndjson, read with `jq -s`) to the `[{"uri", "line", "status"}]` list `scripts/normalize-scenarios.sh` takes — one element per executed test case. `line` is the pickle's last AST node: the `Scenario` line, or the `Examples:` row line for a `Scenario Outline` row. `$prefix` turns the message's `uri` into a repository-relative path.

Copy verbatim to `scripts/messages-results.jq`: [`assets/scripts/messages-results.jq`](../../../core/solution-conformance-testing.skill/assets/scripts/messages-results.jq)
