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
      var configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
      AppConfig config = LoadConfiguration(configFileName);
      LoggingConfig loggingConfig = config.Logging;
      ConfigureNLog(loggingConfig);
    }

    private static AppConfig LoadConfiguration(string configFileName)
    {
      if (File.Exists(configFileName))
      {
        var jsonString = File.ReadAllText(configFileName);
        return JsonSerializer.Deserialize<AppConfig>(jsonString) ?? new AppConfig();
      }
      else
      {
        return new AppConfig();
      }
    }

    public static void ConfigureNLog(LoggingConfig loggingConfig)
    {
      var config = new NLog.Config.LoggingConfiguration();
      var logLevel = loggingConfig != null && loggingConfig.LogLevel != null ? LogLevel.FromString(loggingConfig.LogLevel) : LogLevel.Warn;
      // Targets
      var logConsole = new NLog.Targets.ConsoleTarget("logConsole");
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
      logConsole.Layout =
        "${time} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}";

      // Rules
      config.AddRule(logLevel, LogLevel.Fatal, logConsole);

      // Apply config
      LogManager.Configuration = config;
      var logger = LogManager.GetCurrentClassLogger();
      Log.Initialize(logger);
    }
  }
}
