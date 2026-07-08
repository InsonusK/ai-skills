#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

python -m venv .venv
& .\.venv\Scripts\Activate.ps1
pip install -r (Join-Path $scriptDir '..' 'requirements.txt')
aism sync
