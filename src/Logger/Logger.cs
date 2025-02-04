using System.Runtime.CompilerServices;
using NLog;
using System.IO;

namespace Utilities.Logging
{
    public static class Log
    {
        private static Logger? _logger;

        /// <summary>
        /// Initializes the LoggerWrapper with a specific logger.
        /// Call this method once at the start of the application.
        /// </summary>
        /// <param name="logger">An instance of NLog.Logger</param>
        public static void Initialize(Logger logger)
        {
            _logger = logger;
        }

        public static void Info(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            LogMessage(LogLevel.Info, message, ex: null, memberName, filePath);
        }

        public static void Debug(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            LogMessage(LogLevel.Debug, message, ex: null, memberName, filePath);
        }

        public static void Error(string message, Exception? ex = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            LogMessage(LogLevel.Error, message, ex, memberName, filePath);
        }

        public static void Warn(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            LogMessage(LogLevel.Warn, message, ex: null, memberName, filePath);
        }

        public static void Trace(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            LogMessage(LogLevel.Trace, message, ex: null, memberName, filePath);
        }

        public static void Fatal(string message, Exception? ex = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            LogMessage(LogLevel.Fatal, message, ex, memberName, filePath);
        }

        /// <summary>
        /// Logs a message with the specified log level and additional information.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="ex">An optional exception.</param>
        /// <param name="memberName">The name of the member calling the log method.</param>
        /// <param name="filePath">The file path of the source file.</param>
        private static void LogMessage(LogLevel level, string message, Exception? ex, string memberName, string filePath)
        {
            if (_logger == null)
            {
                return;
            }

            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(level, _logger.Name, message)
            {
                Exception = ex
            };
            logEvent.Properties[key: "ClassName"] = className;
            logEvent.Properties[key: "MemberName"] = memberName;
            logEvent.Properties[key: "FilePath"] = Path.GetFileName(filePath);
            _logger.Log(logEvent);
        }

        /// <summary>
        /// Extracts the class name from the file path.
        /// Assumes that the class name matches the file name.
        /// </summary>
        /// <param name="filePath">The full path to the source file.</param>
        /// <returns>The class name.</returns>
        private static string GetClassName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}