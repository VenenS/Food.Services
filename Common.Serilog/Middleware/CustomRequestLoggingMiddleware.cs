using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Parsing;

namespace Common.Serilog.Middleware
{
    /// <summary>
    /// Мидлвар логирующая информацию о входящих HTTP запросах.
    /// </summary>
    public class CustomRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DiagnosticContext _diagContext;
        private readonly CustomRequestLoggingOptions _options;
        private readonly MessageTemplateParser _parser = new MessageTemplateParser();

        public CustomRequestLoggingMiddleware(
            RequestDelegate next,
            DiagnosticContext diagContext,
            IOptions<CustomRequestLoggingOptions> options)
        {
            _next = next;
            _diagContext = diagContext;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var startTicks = 0L;

            using (var collector = _diagContext.BeginCollection())
            {
                if (_options.OutputRequestExecutingMessage)
                    Log.Write(GenerateRequestIsExecutingLogEvent(httpContext));

                try
                {
                    startTicks = Stopwatch.GetTimestamp();
                    await _next(httpContext);
                }
                finally
                {
                    var diff = Stopwatch.GetTimestamp() - startTicks;
                    var elapsed = diff * (1000d / Stopwatch.Frequency);

                    if (!collector.TryComplete(out var properties))
                        properties = new List<LogEventProperty>();

                    var evt = GenerateRequestExecutedLogEvent(httpContext, elapsed, properties);

                    Log.Write(evt);
                }
            }
        }

        private LogEvent GenerateRequestIsExecutingLogEvent(HttpContext c)
        {
            var template = GetRequestExecutingMessageTemplate(_options.IncludeQueryString);
            var properties = new List<LogEventProperty>();

            AddCommonRequestProperties(c, properties);

            return new LogEvent(DateTimeOffset.UtcNow, _options.Level, null, template, properties);
        }

        private LogEvent GenerateRequestExecutedLogEvent(
            HttpContext c, double elapsed, IEnumerable<LogEventProperty> properties)
        {
            var actionName = properties.FirstOrDefault(x => x.Name == "ActionName");
            var template = GetRequestExecutedMessageTemplate(
                includeActionName: actionName != null,
                includeQueryString: _options.IncludeQueryString);
            var autoProperties = new List<LogEventProperty>
            {
                new LogEventProperty("ResponseElapsed", new ScalarValue(elapsed)),
                new LogEventProperty("ResponseStatusCode", new ScalarValue(c.Response.StatusCode)),
            };

            if (actionName != null)
                autoProperties.Add(actionName);

            AddCommonRequestProperties(c, autoProperties);

            if (c.Request.Headers.TryGetValue("User-Agent", out var ua) && ua.Count > 0)
                autoProperties.Add(new LogEventProperty("RequestUserAgent", new ScalarValue(ua[0])));

            properties = properties.Concat(autoProperties);
            return new LogEvent(DateTimeOffset.UtcNow, _options.Level, null, template, properties);
        }

        private void AddCommonRequestProperties(HttpContext c, List<LogEventProperty> properties)
        {
            properties.Add(new LogEventProperty("RequestMethod", new ScalarValue(GetRequestMethod(c.Request))));
            properties.Add(new LogEventProperty("RequestPath", new ScalarValue(GetRequestPath(c.Request))));
            properties.Add(new LogEventProperty("RequestId", new ScalarValue(GetRequestId(c))));

            if (_options.IncludeQueryString)
            {
                properties.Add(new LogEventProperty(
                    "RequestQuery",
                    new ScalarValue(GetRequestQueryString(c.Request))));
            }

            properties.Add(new LogEventProperty(
                "RequestRemoteIp",
                new ScalarValue(c.Connection.RemoteIpAddress.ToString())));
        }

        private MessageTemplate GetRequestExecutingMessageTemplate(bool includeQueryString)
        {
            var fmt = "{{RequestMethod:l}} {{RequestPath:l}}{0} {{RequestRemoteIp:l}}";
            var template = string.Format(
                fmt,
                includeQueryString ? "{RequestQuery:l}" : "");

            return _parser.Parse(template);
        }

        private MessageTemplate GetRequestExecutedMessageTemplate(bool includeActionName, bool includeQueryString)
        {
            var fmt = "{{RequestMethod:l}} {{RequestPath:l}}{0} {{ResponseStatusCode}} {{ResponseElapsed:0.00}}ms{1}";
            var template = string.Format(
                fmt,
                includeQueryString ? "{RequestQuery:l}" : "",
                includeActionName ? " {ActionName:l}" : "");

            return _parser.Parse(template);
        }

        private static string GetRequestId(HttpContext c) => c.TraceIdentifier;

        private static string GetRequestMethod(HttpRequest r) => r.Method.ToUpperInvariant();

        private static string GetRequestPath(HttpRequest r)
        {
            if (string.IsNullOrEmpty(r.PathBase))
                return r.Path;
            return r.PathBase.ToString().TrimEnd('/') + "/" + r.Path.ToString().TrimStart('/');
        }

        private static string GetRequestQueryString(HttpRequest r) =>
            r.QueryString.ToUriComponent().TrimStart('/');
    }
}
