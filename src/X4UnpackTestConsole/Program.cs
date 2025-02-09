using System;
using NLog;
using Utilities.Logging;
using X4Unpack;

namespace X4UnpackTestConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      ConfigureNLog();
      var logger = LogManager.GetCurrentClassLogger();
      Log.Initialize(logger);

      if (args.Length < 2)
      {
        Console.WriteLine("Usage: X4UnpackTestConsole <source_folder> <destination_folder> [mask]");
        return;
      }

      string sourceFolder = args[0];
      string destinationFolder = args[1];
      string mask = args.Length > 2 ? args[2] : "*.*";

      try
      {
        var extractor = new ContentExtractor(sourceFolder);
        extractor.ExtractFilesByMask(mask, destinationFolder);
        Console.WriteLine("Extraction completed successfully.");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}");
      }
    }

    public static void ConfigureNLog()
    {
      var logLevel = LogLevel.Debug;
      var config = new NLog.Config.LoggingConfiguration();
      var logConsole = new NLog.Targets.ConsoleTarget("logConsole")
      {
        Layout =
          "${time} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}  ",
      };
      //longdate
      config.AddRule(logLevel, LogLevel.Fatal, logConsole);
      LogManager.Configuration = config;
    }
  }
}
