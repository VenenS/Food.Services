function Backup-Postgres {
    param (
        [Parameter(Mandatory=$true)][Object]$parsedConnectionUrl,
        [Parameter(Mandatory=$true)][string]$outputFile
    )

    $pgDump = "pg_dump.exe"

    if (-not (Get-Command $pgDump)) {
        Throw "pg_dump must be available in PATH!"
    }

    try {
        $env:PGPASSWORD = $parsedConnectionUrl.Password
        if ($parsedConnectionUrl.Port) {
            & $pgDump `
                --file="$outputFile" `
                --format=c `
                --dbname $parsedConnectionUrl.Database `
                --username $parsedConnectionUrl.User `
                --host $parsedConnectionUrl.Host `
                --port $parsedConnectionUrl.Port `
                --verbose
        } else {
            & $pgDump `
                --file="$outputFile" `
                --format=c `
                --dbname $parsedConnectionUrl.Database `
                --username $parsedConnectionUrl.User `
                --host $parsedConnectionUrl.Host `
                --verbose
        }
        if (!$?) {
            Throw "Error while dumping database"
        }
    } finally {
        Remove-Item Env:\PGPASSWORD
    }
}