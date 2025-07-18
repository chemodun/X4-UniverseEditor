using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace SharedWindows
{
  public partial class HtmlViewerWindow : Window
  {
    private string _htmlFilePath;

    // Event to notify parent window about errors
    public event EventHandler<HtmlViewerErrorEventArgs>? ErrorOccurred;

    public HtmlViewerWindow(string htmlFilePath, BitmapImage icon)
    {
      InitializeComponent();
      Icon = icon;
      _htmlFilePath = htmlFilePath;

      // Set UserDataFolder before initialization
      SetWebViewUserDataFolder();

      InitializeAsync();
    }

    private async void InitializeAsync()
    {
      try
      {
        HtmlWebView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        await HtmlWebView.EnsureCoreWebView2Async(null);
      }
      catch (System.Exception ex)
      {
        // Notify parent and close window
        OnErrorOccurred(new HtmlViewerErrorEventArgs(ex, "WebView2 initialization failed"));
        Close();
      }
    }

    private void SetWebViewUserDataFolder()
    {
      try
      {
        // Use application-specific temp folder
        string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "X4UniverseEditor";
        string tempFolder = Path.Combine(Path.GetTempPath(), appName, "WebView2Cache");
        Directory.CreateDirectory(tempFolder);

        HtmlWebView.CreationProperties = new CoreWebView2CreationProperties { UserDataFolder = tempFolder };
      }
      catch (System.Exception ex)
      {
        // Notify parent and close window
        OnErrorOccurred(new HtmlViewerErrorEventArgs(ex, "Failed to set WebView2 user data folder"));
        Close();
      }
    }

    private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
      if (e.IsSuccess)
      {
        try
        {
          SetupVirtualHostMapping();
          LoadHtmlFile(_htmlFilePath);
        }
        catch (System.Exception ex)
        {
          OnErrorOccurred(new HtmlViewerErrorEventArgs(ex, "Failed to load HTML file"));
          Close();
        }
      }
      else
      {
        OnErrorOccurred(new HtmlViewerErrorEventArgs(e.InitializationException, "WebView2 initialization completed with error"));
        Close();
      }
    }

    private void SetupVirtualHostMapping()
    {
      // Map local files to a virtual host to avoid CORS issues
      string baseDirectory = Path.GetDirectoryName(_htmlFilePath) ?? Directory.GetCurrentDirectory();

      HtmlWebView.CoreWebView2.SetVirtualHostNameToFolderMapping("local.readme", baseDirectory, CoreWebView2HostResourceAccessKind.Allow);
    }

    private void LoadHtmlFile(string htmlFilePath)
    {
      if (File.Exists(htmlFilePath))
      {
        try
        {
          // Use virtual host mapping instead of file:// protocol
          string fileName = Path.GetFileName(htmlFilePath);
          string virtualUrl = $"https://local.readme/{fileName}";
          HtmlWebView.CoreWebView2.Navigate(virtualUrl);
        }
        catch (System.Exception ex)
        {
          OnErrorOccurred(new HtmlViewerErrorEventArgs(ex, "Failed to navigate to HTML file"));
          Close();
        }
      }
      else
      {
        OnErrorOccurred(
          new HtmlViewerErrorEventArgs(new FileNotFoundException($"HTML file not found: {htmlFilePath}"), "HTML file not found")
        );
        Close();
      }
    }

    private void OnErrorOccurred(HtmlViewerErrorEventArgs e)
    {
      ErrorOccurred?.Invoke(this, e);
    }
  }

  // Event args class for error handling
  public class HtmlViewerErrorEventArgs : EventArgs
  {
    public Exception? Exception { get; }
    public string ErrorMessage { get; }
    public string FilePath { get; }

    public HtmlViewerErrorEventArgs(Exception? exception, string errorMessage, string filePath = "")
    {
      Exception = exception;
      ErrorMessage = errorMessage;
      FilePath = filePath;
    }
  }
}
