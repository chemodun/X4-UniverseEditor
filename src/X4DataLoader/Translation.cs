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
      Translations = [];
    }

    private static Regex ReferenceRegex = new(@"\{(\d+),(\d+)\}");
    private static Regex CommentRegex = new(@"\([^)]*\)");

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
              if (!Translations.TryGetValue(pageId, out Dictionary<string, string>? value))
              {
                value = [];
                Translations[pageId] = value;
              }

              value[id] = RemoveComments(text);
            }
          }
        }
      }
    }

    public string Translate(string reference)
    {
      var match = ReferenceRegex.Match(reference);
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

    private static string RemoveComments(string text)
    {
      return CommentRegex.Replace(text, "").Trim();
    }

    private string ResolveNestedReferences(string text)
    {
      return ReferenceRegex.Replace(
        text,
        match =>
        {
          var page = match.Groups[1].Value;
          var id = match.Groups[2].Value;
          return Translate($"{{{page},{id}}}");
        }
      );
    }
  }
}
