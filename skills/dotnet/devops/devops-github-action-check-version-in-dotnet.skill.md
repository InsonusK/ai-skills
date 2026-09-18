---
name: devops-github-action-check-version-in-dotnet
description: .NET-specific implementation of the check-version reusable composite action — reads Directory.Build.props' <Version>, compares it semantically against the base ref using System.Version, and detects whether the project is publishable to NuGet
whenToUse: when creating or updating `.github/actions/check-version/action.yml` in a .NET project
updated: 20260915
tags:
  - stack/dotnet
  - concern/ci
  - github-actions

---

# Scope
This skill adds .NET-specific mechanics on top of the `check-version` composite action consumed by [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]], [[skills/devops/workflows/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]], [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]], and [[skills/dotnet/devops/devops-github-wf-stack-lib-release-publish-in-dotnet.skill.md|devops-github-wf-stack-lib-release-publish-in-dotnet]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- `Directory.Build.props`' `<Version>` is the single source of truth for the whole solution — never a per-project `<Version>` in an individual `.csproj`, which would let projects drift out of sync.
- Version comparison is semantic (`System.Version`, or `NuGet.Versioning.NuGetVersion` when prerelease suffixes are in play), never plain string comparison.
- "Publishable" means at least one production `.csproj` (a project not itself a `*.Tests` project) sets `<IsPackable>true</IsPackable>`.

# Rule

## MUST

### Implement action.yml reading Directory.Build.props
Create `.github/actions/check-version/action.yml` as a composite action that reads `<Version>` from the current commit's `Directory.Build.props` and from the base ref, comparing them with `System.Version`.
```yaml
name: check-version
description: Read and compare the project's version
outputs:
  current:
    value: ${{ steps.version.outputs.current }}
  bumped:
    value: ${{ steps.version.outputs.bumped }}
  publishable:
    value: ${{ steps.version.outputs.publishable }}
runs:
  using: composite
  steps:
    - id: version
      shell: pwsh
      run: |
        function Read-Version($ref) {
          if ($ref) {
            $xml = [xml](git show "${ref}:Directory.Build.props")
          } else {
            $xml = [xml](Get-Content "Directory.Build.props")
          }
          return [Version]($xml.Project.PropertyGroup.Version | Select-Object -First 1)
        }

        $current = Read-Version $null
        $publishable = (Select-String -Path "**/*.csproj" -Pattern "<IsPackable>true</IsPackable>" -ErrorAction SilentlyContinue) -ne $null

        $baseRef = $env:BASE_REF
        $bumped = "true"
        if ($baseRef) {
          try {
            $base = Read-Version $baseRef
            $bumped = if ($current -gt $base) { "true" } else { "false" }
          } catch {
            $bumped = "true"  # no prior Directory.Build.props to compare against
          }
        }

        "current=$current" >> $env:GITHUB_OUTPUT
        "bumped=$bumped" >> $env:GITHUB_OUTPUT
        "publishable=$(if ($publishable) { 'true' } else { 'false' })" >> $env:GITHUB_OUTPUT
      env:
        BASE_REF: >-
          ${{ github.event_name == 'pull_request' && format('origin/{0}', github.base_ref)
              || (github.event_name == 'push' && github.event.before) || '' }}
```
- Violation: reading `<Version>` from an individual `.csproj` instead of the shared `Directory.Build.props`, or comparing versions as plain strings.
- Risk: `"10.0.0" < "2.0.0"` under string comparison gives a false failure; per-project versions let one project bump while another is forgotten, defeating the "one version for the solution" intent.
- Fix: read `Directory.Build.props`' `<Version>` only, compared with `System.Version`.

### publishable reflects IsPackable
Set `publishable` to `true` only when at least one production `.csproj` sets `<IsPackable>true</IsPackable>`.
- Risk: hardcoding `publishable: true` makes [[skills/dotnet/devops/devops-github-wf-stack-lib-release-publish-in-dotnet.skill.md|devops-github-wf-stack-lib-release-publish-in-dotnet]]'s `publish` job run (and fail, missing NuGet credentials) for a solution that only ships services, never a NuGet package.
- Fix: scan for `<IsPackable>true</IsPackable>` as shown above.

# Check list
- [ ] `.github/actions/check-version/action.yml` exists, reads `Directory.Build.props`' `<Version>` from the current and base ref.
- [ ] Comparison uses `System.Version`, never plain string comparison.
- [ ] `publishable` is `true` only when a production `.csproj` sets `<IsPackable>true</IsPackable>`.
- [ ] Outputs are named `current`, `bumped`, `publishable`, matching every consumer workflow's `needs.check-version.outputs.*` references.
