using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using System;

namespace Common.Serilog.Sinks.LogstashHttp
{
    public static class LogstashSinkConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that sends log events over to a logstash http harvester.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="endpoint">Harvester URL</param>
        /// <param name="username">
        /// A username to use when authenticating with the logstash server, or null
        /// if authentication is not desired.
        /// </param>
        /// <param name="password">
        /// A password to use when authenticating with the logstash server, or null
        /// if authentication is not desired.
        /// </param>
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. Default value is 1000.
        /// </param>
        /// <param name="queueLimit">
        /// The maximum number of events stored in the queue in memory, waiting to be posted over
        /// the network. Default value is infinitely.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. Default value is 2 seconds.
        /// </param>
        /// <param name="textFormatter">
        /// The formatter rendering individual log events into text, for example JSON. Default
        /// value is <see cref="NormalRenderedTextFormatter"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Default value is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        public static LoggerConfiguration LogstashHttp(
            this LoggerSinkConfiguration self,
            string endpoint,
            string username = null,
            string password = null,
            int batchPostingLimit = 1000,
            int? queueLimit = null,
            TimeSpan? period = null,
            ITextFormatter textFormatter = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            var httpClient = new LogstashHttp.Internal.LogstashHttpClient(username, password);
            return self.Http(
                requestUri: endpoint,
                httpClient: httpClient,
                batchPostingLimit: batchPostingLimit,
                queueLimit: queueLimit,
                period: period,
                textFormatter: textFormatter,
                restrictedToMinimumLevel: restrictedToMinimumLevel
            );
        }
    }
}
