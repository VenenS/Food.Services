[CmdletBinding(DefaultParametersetName="Up")]
param (
    # Connection URL для бд
    [Parameter(Mandatory=$true)][alias("u")][Uri]$url,
    # -up: миграция вверх.
    [Parameter(ParameterSetName="Up",Mandatory=$true)][switch][bool]$up,
    # -down: миграция вниз. Требуется параметр -ref.
    [Parameter(ParameterSetName="Down",Mandatory=$true)][switch][bool]$down,
    # -ref <id>: sha коммита/тэг. Откат производится на версию бд
    # используемой на тот момент.
    [Parameter(ParameterSetName="Down",Mandatory=$true)][alias("r")][string]$ref,
    # -n/-dry-run: запуск мигратора без применения изменений к бд с
    # выводом команд которые должны выполниться, если бы миграция
    # запускалась на реальной бд.
    [switch][alias("n")][alias("dry-run")][bool]$dryRun = $false
)

. "$PSScriptRoot/Include/Utils.ps1"
. "$PSScriptRoot/Include/Backup.ps1"

$solutionRoot = "$PSScriptRoot/../.."
$assemblyPath = "$solutionRoot/Food.Services.Migrations/bin/netstandard2.0/Food.Services.Migrations.dll"

# Определение провайдера для мигратора и формирование строки подключения.
$conn = Parse-ConnectionUrl -url $url
$connectionString = Convert-ConnectionUrlToConnectionString -url $url

$backupDir="$solutionRoot/out"
$backupName="db-$(Get-Date -Format 'yyyyMMddhhmm')-$($conn.Provider).dump"

if (-not (Test-Path $assemblyPath)) {
    Throw "Assembly '$assemblyPath' does not exist"
}

switch ($conn.Provider) {
    "pgsql" { 
        $provider = "Postgres"
    }
    Default {
        Throw "Unknown database provider '$($conn.providerName)'."
    }
}

$commonMigratorArgs = @{
    '-assembly' = $assemblyPath
    '-processor' = $provider
    '-connection' = $connectionString
    '-dry-run' = $dryRun
}

# Проверка изменится ли бд
$testArgs = Merge-Hashtables $commonMigratorArgs @{ '-dry-run' = $true }
if ($up) {
    $changed = & "$PSScriptRoot/Include/Upgrade.ps1" @testArgs
} elseif ($down) {
    $testArgs = Merge-Hashtables $testArgs @{ '-ref' = $ref }
    $changed = & "$PSScriptRoot/Include/Rollback.ps1" @testArgs
} else {
    Throw "Migration direction is not set. Pass -up or -down to the script."
}

if ($changed) {
    # Создание бэкапа.
    if (!$dryRun) {
        echo "Creating database backup (provider=$($conn.Provider), host=$($conn.Host), db=$($conn.Database)) ..."
        New-Item -ItemType Directory $backupDir -ea 0 > $null
        $outfile = "$backupDir/$backupName"

        switch ($conn.Provider) {
            "pgsql" {
                Backup-Postgres -parsedConnectionUrl $conn -outputFile $outfile
            }
            Default {
                Throw "Unknown database provider '$($conn.providerName)'."
            }
        }
    } else {
        echo "Will NOT create database backup because -dry-run is enabled"
    }

    # Применение миграций.
    if ($up) {
        echo "Running database upgrade for '$provider' ..."
        $args = Merge-Hashtables $commonMigratorArgs @{}
        & "$PSScriptRoot/Include/Upgrade.ps1" @args
    } else {
        echo "Running database rollback back to '$ref' for '$provider' ..."
        $args = Merge-Hashtables $commonMigratorArgs @{ '-ref' = $ref }
        & "$PSScriptRoot/Include/Rollback.ps1" @args
    }
    echo OK
} else {
    echo "Skipping migration because no changes to the database were detected"
}
