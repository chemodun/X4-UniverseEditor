using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace ChemGateBuilder
{
    public partial class AboutWindow : Window
    {
        public string Version { get; set; }
        public string Copyright { get; set; }
        public List<string> Components { get; set; }

        private readonly string[] ComponentsToExclude =
        [
            "System.",
            "Microsoft.",
            "WindowsBase",
            "PresentationCore",
            "PresentationFramework",
            "Windows",
            "UIAutomation",
            "DirectWriteForwarder",
            "netstandard",
        ];
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Get the version information from the assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Version = $"Version {version?.ToString()}";

            // Get the copyright information from the assembly
            Copyright = $"{Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? ""}";

            // Get the list of components and their versions
            Components = GetReferencedAssemblies();
        }

        private List<string> GetReferencedAssemblies()
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

            return componentList;
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