#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

if (-not (Get-Command aism -ErrorAction SilentlyContinue)) {
    Write-Error "aism executable not found in PATH. Install it: irm https://raw.githubusercontent.com/InsonusK/go-ai-skill-manager/master/scripts/install.ps1 | iex"
}

& python (Join-Path $scriptDir "check-architecture-tags.py")
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& aism --version
& aism sync
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
