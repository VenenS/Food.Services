using System;
using Common.Serilog.Middleware;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class CommonSerilogAppBuilderExtensions
    {
        public static IApplicationBuilder UseCustomRequestLogging(
            this IApplicationBuilder app, Action<CustomRequestLoggingOptions> optionsBuilder = null)
        {
            var options = new CustomRequestLoggingOptions();

            optionsBuilder?.Invoke(options);

            return app.UseMiddleware<CustomRequestLoggingMiddleware>(Options.Create(options));
        }
    }
}
