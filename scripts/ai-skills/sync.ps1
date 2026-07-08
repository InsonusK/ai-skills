[CmdletBinding()]
param (
    [switch]$v,     # Флаг подробного вывода
    [switch]$help   # Показать справку
)

if ($help) {
    Write-Host "Usage: sync.ps1 [-v] [--help]"
    Write-Host "  -v      Подробный вывод"
    Write-Host "  --help  Показать эту справку"
    exit 0
}

# Активируем виртуальное окружение Python
.venv/Scripts/Activate.ps1

# Формируем аргументы для aism sync
$arguments = @('sync')
if ($v) {
    $arguments += '-v'
}

# Запускаем синхронизацию
aism @arguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
