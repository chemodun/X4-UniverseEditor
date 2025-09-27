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
  public partial class AboutWindow : Window
  {
    public string ProgramTitle { get; set; } = string.Empty;
    public string AboutTitle
    {
      get => $"About {ProgramTitle}";
    }
    public BitmapImage Image { get; set; }
    public string Version { get; set; }
    public string Copyright { get; set; }
    public List<string> Components { get; set; }
    public Dictionary<string, string> InformationalLinks { get; set; } = new();

    public AboutWindow(BitmapImage icon, Assembly assembly)
    {
      InitializeComponent();
      DataContext = this;
      AssemblyInfo assemblyInfo = AssemblyInfo.GetAssemblyInfo(assembly);
      // Grab all AssemblyMetadata attributes
      var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToDictionary(a => a.Key, a => a.Value ?? string.Empty);
      if (metadata.TryGetValue("NexusModsUrl", out var nexusModsUrl))
        InformationalLinks["Nexus Mods"] = nexusModsUrl;
      if (metadata.TryGetValue("EgosoftForumUrl", out var egosoftForumUrl))
        InformationalLinks["Egosoft Forum"] = egosoftForumUrl;
      if (metadata.TryGetValue("RepositoryUrl", out var gitHubUrl))
        InformationalLinks["GitHub"] = gitHubUrl;
      // Get the version information from the assembly
      var version = assemblyInfo.Version;
      Version = $"Version {version?.ToString()}";
      Icon = icon;
      Image = icon;
      ProgramTitle = assemblyInfo.Product;
      // Get the copyright information from the assembly
      Copyright = assemblyInfo.Copyright;

      // Get the list of components and their versions
      Components = assemblyInfo.Components;
    }

    private BitmapImage? GetWindowIcon()
    {
      // Retrieve the current window icon
      if (this.Icon is BitmapImage bitmap)
      {
        return bitmap;
      }
      return null;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      if (e.Uri != null)
      {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
      }
      e.Handled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
