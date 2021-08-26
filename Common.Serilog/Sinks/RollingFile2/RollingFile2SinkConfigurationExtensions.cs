using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Serilog.Sinks.RollingFile2
{
    public static class RollingFile2SinkConfigurationExtensions
    {
        /// <summary>
        /// Write log events to the specified file. This is just a thin wrapper around
        /// Serilog.Sinks.File sink that adds an ability to use environment variables
        /// in path.
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="formatter">A formatter, such as <see cref="JsonFormatter"/>, to convert the log events into
        /// text for the file.
        /// </param>
        /// <param name="path">Path to the file.  May include environment variables
        /// in form %VARIABLE%. May also start with ~ and the path will resolve to
        /// the current application directory.
        /// </param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="fileSizeLimitBytes">The approximate maximum size, in bytes, to which a log file will be allowed to grow.
        /// For unrestricted growth, pass null. To avoid writing partial events, the last event within the limit
        /// will be written in full even if it exceeds the limit.</param>
        /// <param name="buffered">Indicates if flushing to the output file can be buffered or not. The default
        /// is false.</param>
        /// <param name="shared">Allow the log file to be shared by multiple processes. The default is false.</param>
        /// <param name="flushToDiskInterval">If provided, a full disk flush will be performed periodically at the specified interval.</param>
        /// <param name="rollingInterval">The interval at which logging will roll over to a new file.</param>
        /// <param name="rollOnFileSizeLimit">If <code>true</code>, a new file will be created when the file size limit is reached. Filenames
        /// will have a number appended in the format <code>_NNN</code>, with the first filename given no number.</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. For unlimited retention, pass null. The default is 31.</param>
        public static LoggerConfiguration RollingFile2(
            this LoggerSinkConfiguration self,
            ITextFormatter formatter,
            string path,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
            long? fileSizeLimitBytes = 256 * 1024 * 1024,
            bool buffered = false,
            bool shared = false,
            TimeSpan? flushToDiskInterval = null,
            RollingInterval rollingInterval = RollingInterval.Infinite,
            bool rollOnFileSizeLimit = false,
            int? retainedFileCountLimit = 5)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(path);
            if (expandedPath.StartsWith($"~/") || expandedPath.StartsWith("~\\") || expandedPath == "~")
            {
                var appRoot = AppDomain.CurrentDomain.BaseDirectory
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                expandedPath = $"{appRoot}{expandedPath.Substring(1)}";
            }
            return self.File(
                formatter: formatter,
                path: expandedPath,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                fileSizeLimitBytes: fileSizeLimitBytes,
                buffered: buffered,
                shared: shared,
                flushToDiskInterval: flushToDiskInterval,
                rollingInterval: rollingInterval,
                rollOnFileSizeLimit: rollOnFileSizeLimit,
                retainedFileCountLimit: retainedFileCountLimit);
        }
    }
}
