using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using NLog;
using Utilities.Logging;

namespace ChemGateBuilder
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
    }

    public static void ConfigureNLog(LoggingConfig loggingConfig)
    {
      var currentConfig = LogManager.Configuration;
      var logLevel = loggingConfig != null && loggingConfig.LogLevel != null ? LogLevel.FromString(loggingConfig.LogLevel) : LogLevel.Warn;

      // Check if the current configuration matches the new configuration
      bool configChanged = false;

      // Check console target
      var currentConsoleTarget = currentConfig?.FindTargetByName<NLog.Targets.ConsoleTarget>("logConsole");
      if (
        currentConsoleTarget == null
        || currentConsoleTarget.Layout.ToString()
          != "${time} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}"
      )
      {
        configChanged = true;
      }

      // Check file target
      var currentFileTarget = currentConfig?.FindTargetByName<NLog.Targets.FileTarget>("logFile");
      if (loggingConfig != null && loggingConfig.LogToFile)
      {
        var logFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.log";
        if (currentFileTarget == null || currentFileTarget.FileName.ToString() != logFileName)
        {
          configChanged = true;
        }
      }
      else if (currentFileTarget != null)
      {
        configChanged = true;
      }

      // Check log level rules
      var currentConsoleRule = currentConfig?.LoggingRules.FirstOrDefault(r => r.Targets.Contains(currentConsoleTarget));
      var currentFileRule = currentConfig?.LoggingRules.FirstOrDefault(r => r.Targets.Contains(currentFileTarget));
      if (currentConsoleRule == null || currentConsoleRule.Levels.Min() != logLevel || currentConsoleRule.Levels.Max() != LogLevel.Fatal)
      {
        configChanged = true;
      }
      if (loggingConfig != null && loggingConfig.LogToFile)
      {
        if (currentFileRule == null || currentFileRule.Levels.Min() != logLevel || currentFileRule.Levels.Max() != LogLevel.Fatal)
        {
          configChanged = true;
        }
      }
      else if (currentFileRule != null)
      {
        configChanged = true;
      }

      // Update configuration if changes exist
      if (configChanged)
      {
        var config = new NLog.Config.LoggingConfiguration();

        // Targets
        var logConsole = new NLog.Targets.ConsoleTarget("logConsole")
        {
          Layout =
            "${time} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}",
        };
        config.AddRule(logLevel, LogLevel.Fatal, logConsole);

        if (loggingConfig != null && loggingConfig.LogToFile)
        {
          var logFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.log";
          var logFile = new NLog.Targets.FileTarget("logFile")
          {
            FileName = logFileName,
            Layout =
              "${longdate} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}",
            KeepFileOpen = true,
            DeleteOldFileOnStartup = true, // Overwrite the log file on each run
            ArchiveAboveSize = 0,
            ConcurrentWrites = true,
          };
          config.AddRule(logLevel, LogLevel.Fatal, logFile);
        }

        // Apply config
        LogManager.Configuration = config;
        var logger = LogManager.GetCurrentClassLogger();
        Log.Initialize(logger);
      }
    }
  }
}
