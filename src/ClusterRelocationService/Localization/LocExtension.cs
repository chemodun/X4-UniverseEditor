using System.Windows.Data;
using System.Windows.Markup;
using Localization;

namespace ClusterRelocationService.Localization;

[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension : MarkupExtension
{
  public string Key { get; set; } = string.Empty;
  public string? Fallback { get; set; }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    if (string.IsNullOrWhiteSpace(Key))
    {
      return string.Empty;
    }

    var binding = new Binding($"[{Key}]")
    {
      Source = LocalizationManager.Shared,
      Mode = BindingMode.OneWay,
      FallbackValue = Fallback ?? Key,
    };

    return binding.ProvideValue(serviceProvider);
  }
}
