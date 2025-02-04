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
            if (_logger == null)
            {
                return;
            }
            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(LogLevel.Info, _logger!.Name, message);
            logEvent.Properties["ClassName"] = className;
            logEvent.Properties["MemberName"] = memberName;
            logEvent.Properties["FilePath"] = Path.GetFileName(filePath);
            _logger.Log(logEvent);
        }

        public static void Debug(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            if (_logger == null)
            {
                return;
            }
            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(LogLevel.Debug, _logger!.Name, message);
            logEvent.Properties["ClassName"] = className;
            logEvent.Properties["MemberName"] = memberName;
            logEvent.Properties["FilePath"] = Path.GetFileName(filePath);
            _logger.Log(logEvent);
        }

        public static void Error(string message, Exception? ex = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            if (_logger == null)
            {
                return;
            }
            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(LogLevel.Error, _logger!.Name, message)
            {
                Exception = ex
            };
            logEvent.Properties["ClassName"] = className;
            logEvent.Properties["MemberName"] = memberName;
            logEvent.Properties["FilePath"] = Path.GetFileName(filePath);
            _logger.Log(logEvent);
        }

        public static void Warn(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            if (_logger == null)
            {
                return;
            }
            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(LogLevel.Warn, _logger!.Name, message);
            logEvent.Properties["ClassName"] = className;
            logEvent.Properties["MemberName"] = memberName;
            logEvent.Properties["FilePath"] = Path.GetFileName(filePath);
            _logger.Log(logEvent);
        }

        public static void Trace(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            if (_logger == null)
            {
                return;
            }
            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(LogLevel.Trace, _logger!.Name, message);
            logEvent.Properties["ClassName"] = className;
            logEvent.Properties["MemberName"] = memberName;
            logEvent.Properties["FilePath"] = Path.GetFileName(filePath);
            _logger.Log(logEvent);
        }

        public static void Fatal(string message, Exception? ex = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            if (_logger == null)
            {
                return;
            }
            var className = GetClassName(filePath);
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger!.Name, message)
            {
                Exception = ex
            };
            logEvent.Properties["ClassName"] = className;
            logEvent.Properties["MemberName"] = memberName;
            logEvent.Properties["FilePath"] = Path.GetFileName(filePath);
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