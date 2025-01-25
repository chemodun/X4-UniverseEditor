using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Utilities.Logging;

namespace X4DataLoader
{
    public class Translation
    {
        public Dictionary<string, Dictionary<string, string>> Translations { get; private set; }

        public Translation()
        {
            Translations = new Dictionary<string, Dictionary<string, string>>();
        }

        public void Load(string filePath)
        {
            var doc = XDocument.Load(filePath);
            foreach (var pageElement in doc.Descendants("page"))
            {
                var pageId = pageElement.Attribute("id")?.Value;
                if (pageId != null)
                {
                    foreach (var element in pageElement.Descendants("t"))
                    {
                        var id = element.Attribute("id")?.Value;
                        var text = element.Value;

                        if (id != null)
                        {
                            if (!Translations.ContainsKey(pageId))
                            {
                                Translations[pageId] = new Dictionary<string, string>();
                            }
                            Translations[pageId][id] = RemoveComments(text);
                        }
                    }
                }
            }
        }

        public string Translate(string reference)
        {
            var match = Regex.Match(reference, @"\{(\d+),(\d+)\}");
            if (match.Success)
            {
                var page = match.Groups[1].Value;
                var id = match.Groups[2].Value;

                if (Translations.TryGetValue(page, out var pageTranslations) && pageTranslations.TryGetValue(id, out var text))
                {
                    return ResolveNestedReferences(text);
                }
            }
            return reference;
        }

        private string RemoveComments(string text)
        {
            return Regex.Replace(text, @"\([^)]*\)", "").Trim();
        }

        private string ResolveNestedReferences(string text)
        {
            return Regex.Replace(text, @"\{(\d+),(\d+)\}", match =>
            {
                var page = match.Groups[1].Value;
                var id = match.Groups[2].Value;
                return Translate($"{{{page},{id}}}");
            });
        }
    }
}
