using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace ChemGateBuilder
{
    public partial class AboutWindow : Window
    {
        public string Version { get; set; }
        public string Copyright { get; set; }

        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Get the version information from the assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            Version = $"Version {version?.ToString()}";
            Copyright = $"{Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? ""}";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}