# scripts/messages-results.jq

Reduces cucumber-js's `message` formatter output (Cucumber Messages, ndjson, read with `jq -s`) to the `[{"uri", "line", "status"}]` list `scripts/normalize-scenarios.sh` takes — one element per executed test case. `line` is the pickle's last AST node: the `Scenario` line, or the `Examples:` row line for a `Scenario Outline` row. `$prefix` turns the message's `uri` into a repository-relative path.

```jq
# Cucumber Messages (ndjson, slurped with -s) -> [{uri, line, status}], one per test case.
# $prefix is prepended to each pickle uri to make it repo-relative.
(map(select(.gherkinDocument) | .gherkinDocument.feature
     | [.. | objects | select(.id? and .location?) | {key: .id, value: .location.line}]
     | from_entries) | add // {}) as $lines
| (map(select(.pickle) | {key: .pickle.id, value: .pickle}) | from_entries) as $pickles
| (map(select(.testCase) | {key: .testCase.id, value: .testCase.pickleId}) | from_entries) as $cases
| (map(select(.testCaseStarted) | {key: .testCaseStarted.id, value: .testCaseStarted.testCaseId}) | from_entries) as $started
| map(select(.testStepFinished) | .testStepFinished)
| group_by(.testCaseStartedId)
| map(
    ($pickles[$cases[$started[.[0].testCaseStartedId]]]) as $p
    | [.[].testStepResult.status] as $s
    | {
        uri: ($prefix + $p.uri),
        line: $lines[$p.astNodeIds[-1]],
        status: (if all($s[]; . == "PASSED") then "passed"
                 elif any($s[]; . == "FAILED" or . == "UNDEFINED" or . == "AMBIGUOUS" or . == "PENDING") then "failed"
                 else "skipped" end)
      })
```
