Add-Type -AssemblyName System.Web

# Парсит Connection URL
function Parse-ConnectionUrl {
    param (
        # Connection URL
        [Parameter(Mandatory=$true)][Uri]$url
    )

    if (!$url.IsAbsoluteUri) {
        Throw "Invalid URL: '$url'"
    }

    # Формирование строки подключения из URL
    if (!$url.LocalPath -or $url.LocalPath -eq '/') {
        Throw "Empty database name"
    }

    $dbname = $url.LocalPath.Substring(1)
    $user = [System.Web.HttpUtility]::UrlDecode($url.UserInfo.Split(":")[0])
    $pass = [System.Web.HttpUtility]::UrlDecode($url.UserInfo.Split(":")[1])
    $params = [System.Web.HttpUtility]::ParseQueryString($url.Query)
    return New-Object psobject @{
        Database = $dbname
        Provider = $url.Scheme.ToLower()
        User = $user
        Password = $pass
        Host = $url.Host
        Port = $(if (!$url.IsDefaultPort) { $url.Port } else { $null })
        Params = $params
    }
}

# Конвертирует connection URL в connection string, распознаваемый sql провайдерами .net
function Convert-ConnectionUrlToConnectionString {
    param (
        # Connection URL
        [Parameter(Mandatory=$true)][alias("c")][Uri]$url
    )
    $conn = Parse-ConnectionUrl -url $url
    switch ($conn.Provider) {
        "pgsql" {
            if (!$url.IsDefaultPort) {
                $port = "Port=$($conn.Port);"
            }
            return "Database=$($conn.Database);Server=$($conn.Host);$($port)User Id=$($conn.User);Password=$($conn.Password)"
        }
        Default {
            Throw "Unknown database provider '$($url.Scheme)'"
        }
    }
}

# Мерджит 2 словаря, заменяя значения ключей в первом. Возвращает
# новый словарь.
function Merge-Hashtables($first, $second) {
    $result = $first.Clone()
    foreach ($key in $second.Keys) {
        $result[$key] = $second[$key]
    }
    return $result
}

# Проверяет установлен ли dotnet fm и бросает исключение если нет.
function Assert-FluentMigratorToolInstalled {
    if (!(dotnet fm -h)) {
        Throw "FluentMigrator tool is not installed. Run 'dotnet tool install -g fluentmigrator.dotnet.cli' to install it."
    }
}

# Проверяет изменилась ли бд в ходе тестового запуска мигратора.
function Test-DatabaseModified($migratorOutput) {
    return -not (($output -is [string]) -and ($output -match '^Task completed\.$'))
}