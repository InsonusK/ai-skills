#!/usr/bin/env bash
# Re-checks whether Stryker.NET mutation-tests an xunit.v3 / Microsoft.Testing.Platform
# (MTP) solution correctly, by running the same fixture twice:
#   v2 - the fixture as-is (xUnit v2 + Reqnroll.xUnit on VSTest: this solution's stack)
#   v3 - the same fixture converted to xunit.v3 + Reqnroll.xunit.v3 on MTP
# and comparing the mutation results. Same code + same tests => the scores must match.
#
# Verdict (exit code):
#   0  SUPPORTED     - v3 kills the same mutants as v2; moving to xunit.v3 is unblocked
#   1  NOT SUPPORTED - v3 result differs (e.g. every mutant "survives", score 0%)
#   2  INCONCLUSIVE  - a build/run failed, or v2 itself killed nothing
#
# Params (env vars, all optional):
#   STRYKER_VERSION    dotnet-stryker version to test      (default: latest stable on NuGet)
#   XUNIT_V3_VERSION   xunit.v3 package version             (default: latest stable on NuGet)
#   XUNIT_RUNNER_V3    xunit.runner.visualstudio for v3     (default: latest stable on NuGet)
#   FIXTURE            a .NET example on xUnit v2/VSTest    (default: the plateau-core example)
#   WORK_DIR           where both copies are built          (default: a new mktemp dir)
#
# Run from the repository root:
#   skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/recheck/stryker-xunit-v3.sh
set -euo pipefail

latest() { # latest stable version of a NuGet package id
  curl -fsS "https://api.nuget.org/v3-flatcontainer/$1/index.json" \
    | jq -r '.versions | map(select(test("-") | not)) | last'
}

FIXTURE="${FIXTURE:-skills/dotnet/architecture/plateau/plateau-core/plateau-core.skill/example}"
STRYKER_VERSION="${STRYKER_VERSION:-$(latest dotnet-stryker)}"
XUNIT_V3_VERSION="${XUNIT_V3_VERSION:-$(latest xunit.v3)}"
XUNIT_RUNNER_V3="${XUNIT_RUNNER_V3:-$(latest xunit.runner.visualstudio)}"
WORK_DIR="${WORK_DIR:-$(mktemp -d)}"
SOLUTION=$(find "$FIXTURE" -maxdepth 1 \( -name '*.slnx' -o -name '*.sln' \) -printf '%f\n' | head -1)
[ -n "$SOLUTION" ] || { echo "no .slnx/.sln in $FIXTURE" >&2; exit 2; }

echo "fixture=$FIXTURE solution=$SOLUTION stryker=$STRYKER_VERSION xunit.v3=$XUNIT_V3_VERSION runner=$XUNIT_RUNNER_V3"
echo "work dir: $WORK_DIR"

to_v3() { # convert a copy of the fixture from xUnit v2/VSTest to xunit.v3/MTP
  local dir="$1"
  sed -i -E \
    -e "s#<PackageVersion Include=\"xunit\" Version=\"[^\"]+\" />#<PackageVersion Include=\"xunit.v3\" Version=\"$XUNIT_V3_VERSION\" />#" \
    -e "s#<PackageVersion Include=\"xunit.runner.visualstudio\" Version=\"[^\"]+\" />#<PackageVersion Include=\"xunit.runner.visualstudio\" Version=\"$XUNIT_RUNNER_V3\" />#" \
    -e 's#<PackageVersion Include="Reqnroll.xUnit" #<PackageVersion Include="Reqnroll.xunit.v3" #' \
    "$dir/Directory.Packages.props"
  find "$dir" -name '*.csproj' -exec sed -i -E \
    -e 's#<PackageReference Include="xunit" />#<PackageReference Include="xunit.v3" />#' \
    -e 's#<PackageReference Include="Reqnroll.xUnit" />#<PackageReference Include="Reqnroll.xunit.v3" />#' {} +
  printf '{\n  "test": { "runner": "Microsoft.Testing.Platform" }\n}\n' > "$dir/global.json"
  # opt `dotnet test` into MTP (required for xunit.v3 on the .NET 10 SDK)
  sed -i 's#</Project>#  <PropertyGroup>\n    <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>\n    <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>\n  </PropertyGroup>\n</Project>#' \
    "$dir/Directory.Build.props"
}

run_stryker() { # build a copy and mutation-test it; prints "killed survived timeout nocoverage"
  local dir="$1"
  (
    cd "$dir"
    printf '{ "stryker-config": { "solution": "%s", "test-case-filter": "Category!=todo" } }\n' "$SOLUTION" > stryker-config.json
    dotnet tool restore >/dev/null
    dotnet tool update dotnet-stryker --local --version "$STRYKER_VERSION" >/dev/null
    dotnet build "$SOLUTION" -c Release >build.log 2>&1 || { echo "build failed: $dir/build.log" >&2; exit 2; }
    dotnet tool run dotnet-stryker -r json -O stryker-out >stryker.log 2>&1 || { echo "stryker failed: $dir/stryker.log" >&2; exit 2; }
    jq -r '[.files[].mutants[].status] as $s
      | [($s | map(select(. == "Killed")) | length), ($s | map(select(. == "Survived")) | length),
         ($s | map(select(. == "Timeout")) | length), ($s | map(select(. == "NoCoverage")) | length)] | @tsv' \
      stryker-out/reports/mutation-report.json
  )
}

# any failure while preparing the copies is INCONCLUSIVE (2), never NOT SUPPORTED (1)
trap 'echo "INCONCLUSIVE: preparing the fixture copies failed" >&2; exit 2' ERR
mkdir -p "$WORK_DIR"
rm -rf "$WORK_DIR/v2" "$WORK_DIR/v3"
cp -r "$FIXTURE" "$WORK_DIR/v2"
cp -r "$FIXTURE" "$WORK_DIR/v3"
rm -rf "$WORK_DIR"/v?/{tmp,public,TestResults}
to_v3 "$WORK_DIR/v3"
trap - ERR

V2=$(run_stryker "$WORK_DIR/v2") || exit 2
V3=$(run_stryker "$WORK_DIR/v3") || exit 2
read -r V2_KILLED V2_SURVIVED V2_TIMEOUT V2_NOCOV <<<"$V2"
read -r V3_KILLED V3_SURVIVED V3_TIMEOUT V3_NOCOV <<<"$V3"

printf '%-4s killed=%s survived=%s timeout=%s noCoverage=%s\n' \
  v2 "$V2_KILLED" "$V2_SURVIVED" "$V2_TIMEOUT" "$V2_NOCOV" \
  v3 "$V3_KILLED" "$V3_SURVIVED" "$V3_TIMEOUT" "$V3_NOCOV"

if [ "$V2_KILLED" -eq 0 ]; then
  echo "INCONCLUSIVE: v2 killed no mutant - the fixture proves nothing"; exit 2
fi
if [ "$V3_KILLED" -eq "$V2_KILLED" ] && [ "$V3_SURVIVED" -eq "$V2_SURVIVED" ]; then
  echo "SUPPORTED: Stryker.NET $STRYKER_VERSION mutation-tests xunit.v3/MTP like xUnit v2"; exit 0
fi
echo "NOT SUPPORTED: Stryker.NET $STRYKER_VERSION reports a different result on xunit.v3/MTP"; exit 1
