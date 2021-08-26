param (
    [Parameter(Mandatory=$true)][alias("p")][string]$processor,
    [Parameter(Mandatory=$true)][alias("a")][string]$assembly,
    [Parameter(Mandatory=$true)][alias("c")][string]$connection,
    # Запуск отката без применения изменений к БД. Выводит список
    # команд которые должны выполниться. Если параметр равен $true,
    # в качестве результата из скрипта возвращается булево значение
    # которое означает были ли найдены ли еще не примененные
    # миграции.
    [switch][alias("n")][alias("dry-run")][bool]$dryRun
)

. "$PSScriptRoot/Utils.ps1"

Assert-FluentMigratorToolInstalled

if (!$dryRun) {
    & dotnet fm migrate -V -c $connection -p $processor -a $assembly up
    if (!$?) {
        Throw "Migrator has failed."
    }
} else {
    $output = dotnet fm migrate -c $connection -p $processor -a $assembly --preview up
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
