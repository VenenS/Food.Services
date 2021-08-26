param (
    [Parameter(Mandatory=$true)][alias("p")][string]$processor,
    [Parameter(Mandatory=$true)][alias("a")][string]$assembly,
    [Parameter(Mandatory=$true)][alias("c")][string]$connection,
    [Parameter(Mandatory=$true)][alias("r")][string]$ref,
    # Запуск отката без применения изменений к БД. Выводит список
    # команд которые должны выполниться. Если параметр установлен,
    # результатом скрипта становится булево значение которое означает
    # существуют ли миграции, которые не были применены.
    [switch][alias("n")][alias("dry-run")][bool]$dryRun
)

. "$PSScriptRoot/Utils.ps1"

Assert-FluentMigratorToolInstalled

$version = (git ls-tree --full-tree --name-only -r $ref) -split '\r?\n' |
    Select-String -Pattern '^Food.Services.Migrations/Scripts/(\d+)_[^/]*\.cs' |
    % {
        return New-Object psobject -Property @{
            Version = $_.Matches[0].Groups[1].Value
            Filename = $_.Matches[0].Groups[0].Value
        }
    } |
    Sort-Object -Property Version |
    Select-Object -Last 1
if (!$version) {
    Throw "Couldn't determine database version for '$ref'"
}

echo "Script : $($version.Filename)"
echo "Version: $($version.Version)"

# Возврат бд к прежней версии.
if (!$dryRun) {
    & dotnet fm rollback -V -c $connection -p $processor -a $assembly to $version.Version
    if (!$?) {
        Throw "Migrator has failed."
    }
} else {
    $output = dotnet fm rollback -c $connection -p $processor -a $assembly --preview to $version.Version
    if (!$?) {
        echo $output | Out-Host
        Throw "Migrator has failed."
    }

    # Проверка произвелись ли какие-либо миграции.
    if (Test-DatabaseModified $output) {
        echo $output | Out-Host
        return $true
    } else {
        return $false
    }
}
