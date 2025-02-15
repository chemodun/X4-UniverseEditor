using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SharedWindows
{
  public class AssemblyInfo
  {
    public string Product { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public List<string> Components { get; set; } = new();
    private static readonly string[] ComponentsToExclude =
    {
      "System.",
      "Microsoft.",
      "WindowsBase",
      "PresentationCore",
      "PresentationFramework",
      "Windows",
      "UIAutomation",
      "DirectWriteForwarder",
      "netstandard",
    };

    public static AssemblyInfo GetAssemblyInfo(Assembly assembly)
    {
      var assemblyInfo = new AssemblyInfo();
      var version = assembly.GetName().Version;
      assemblyInfo.Product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";
      assemblyInfo.Version = $"Version {version?.ToString()}";
      assemblyInfo.Components = GetReferencedAssemblies();
      assemblyInfo.Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
      return assemblyInfo;
    }

    private static List<string> GetReferencedAssemblies()
    {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      var componentList = new List<string>();

      foreach (var assembly in assemblies)
      {
        var name = assembly.GetName();
        if (name == null || name.Name == null)
        {
          continue;
        }
        if (ComponentsToExclude.Any(x => name.Name.StartsWith(x)))
        {
          continue;
        }

        componentList.Add(item: $"{name.Name} - Version {name.Version}");
      }
      componentList.Sort();
      return componentList;
    }
  }
}
