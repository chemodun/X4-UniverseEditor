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
    public Dictionary<string, string> InformationalLinks { get; set; }

    public AboutWindow(BitmapImage icon, AssemblyInfo assemblyInfo, Dictionary<string, string> informationalLinks)
    {
      InitializeComponent();
      DataContext = this;
      InformationalLinks = informationalLinks;
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
