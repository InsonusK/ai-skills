#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

python -m venv .venv

$venvPython = [System.IO.Path]::Combine($scriptDir, '.venv', 'Scripts', 'python.exe')
if (-not (Test-Path $venvPython)) {
    Write-Error "Python executable not found in virtual environment: $venvPython"
}

$requirementsPath = [System.IO.Path]::Combine($scriptDir, '..', 'requirements.txt')
& $venvPython -m pip install -r $requirementsPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$venvAism = [System.IO.Path]::Combine($scriptDir, '.venv', 'Scripts', 'aism.exe')
if (-not (Test-Path $venvAism)) {
    Write-Error "aism executable not found in virtual environment: $venvAism"
}

& $venvAism sync
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
