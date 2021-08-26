using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Serilog;
using Serilog.Events;

namespace Common.Serilog.Middleware
{
    public class CustomRequestLoggingOptions
    {
        /// <summary>
        /// Выводить в лог сообщение о новом запросе до начала выполнения
        /// запроса.
        /// </summary>
        public bool OutputRequestExecutingMessage { get; set; }

        /// <summary>
        /// Добавить query string в логи?
        /// </summary>
        public bool IncludeQueryString { get; set; } = true;

        /// <summary>
        /// Уровень важности для сообщений производимых мидлварью.
        /// </summary>
        public LogEventLevel Level { get; set; } = LogEventLevel.Information;
    }
}
