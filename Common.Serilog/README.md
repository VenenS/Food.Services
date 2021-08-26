# Common.Serilog

Описание, TODO

## Installation

#### Установка зависимостей

```
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Settings.Configuration
```

#### Конфигурация ASP.NET Core приложения

В Program.cs, в конфигурацию `WebHostBuilder` добавить `UseSerilog`:

```csharp
WebHost
  .CreateDefaultBuilder(args)
  .UseSerilog()
  .UseStartup<Startup>();
```

Для интеграции Serilog'a в стандартный пайплайн для логгинга, в его конфигурации нужно добавить вызов метода расширения `AddSerilog`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ...

    services.AddLogging(builder => {
        builder.ClearProviders();
        builder.AddSerilog();
    });

    // ...
```

#### appsettings.json

Необходимо добавить конфигурацию логгера в appsettings:

```json
"Serilog": {
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "formatter": "Common.Serilog.Formatters.ElasticFormatter, Common.Serilog",
        "path": "Logs/PROJECT_NAME@ASP_ENVIRONMENT/log-.log",
        "rollingInterval": "Day"
      }
    }
  ],
  "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
}
```

Самая важная часть в этом отрывке -- это конфигурация `File` sink с использованием преобразователя `ElasticFormatter`. `ElasticFormatter` конвертирует лог сообщения в формат, подходящий для отправки в ELK.

### Конфигурация filebeat

### Создание шаблона индексов в Elastic Search

## Просмотр логов

Для просмотра логов можно воспользоваться поставляемым скриптом `Scripts/prettify-log.sh` (для работы требуется **bash** и **jq**), например:

```bash
./prettify-log.sh ../../Food.Services/Logs/Food.Services\@dev/log-20201020.log
```

либо воспользоваться сторонними программами для чтения json логов:

* json-log-viewer - https://github.com/gistia/json-log-viewer
* jl - https://github.com/mightyguava/jl