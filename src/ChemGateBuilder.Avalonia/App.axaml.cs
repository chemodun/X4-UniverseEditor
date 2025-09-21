using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChemGateBuilder.AvaloniaApp.Services;

namespace ChemGateBuilder.AvaloniaApp
{
  public partial class App : Application
  {
    private SettingsService? _settings;

    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        _settings = new SettingsService();
        _settings.Load();
        desktop.MainWindow = new Views.MainWindow(_settings);
      }

      base.OnFrameworkInitializationCompleted();
    }
  }
}
