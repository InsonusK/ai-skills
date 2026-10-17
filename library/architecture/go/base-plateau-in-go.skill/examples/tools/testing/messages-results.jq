# Cucumber Messages (ndjson, slurped with -s) -> [{uri, line, status}], one per test case.
# $prefix is prepended to each pickle uri to make it repo-relative; "dir/.." segments are
# collapsed, since a feature file linked into a project is reported as "../../src/...".
def normpath: split("/") | reduce .[] as $s ([];
  if $s == ".." and length > 0 and .[-1] != ".." then .[:-1]
  elif $s == "." or $s == "" then .
  else . + [$s] end) | join("/");
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
        uri: ($prefix + $p.uri | normpath),
        line: $lines[$p.astNodeIds[-1]],
        status: (if all($s[]; . == "PASSED") then "passed"
                 elif any($s[]; . == "FAILED" or . == "UNDEFINED" or . == "AMBIGUOUS" or . == "PENDING") then "failed"
                 else "skipped" end)
      })
