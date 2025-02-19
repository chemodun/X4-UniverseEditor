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

    private static readonly Regex ReferenceRegex = new(@"\{(\d+),(\d+)\}");
    private static readonly Regex CommentRegex = new(@"\([^)]*\)");

    public void Clear()
    {
      Translations.Clear();
    }

    public void LoadFromXML(XElement rootElement)
    {
      foreach (var pageElement in rootElement.Descendants("page"))
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

    public string TranslateByPage(int page, int id)
    {
      if (Translations.TryGetValue(page.ToString(), out var pageTranslations) && pageTranslations.TryGetValue(id.ToString(), out var text))
      {
        return ResolveNestedReferences(text);
      }
      return string.Empty;
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

    public string TranslateString(string text)
    {
      return ResolveNestedReferences(RemoveComments(text));
    }

    public static int[] GetIds(string reference)
    {
      var match = ReferenceRegex.Match(reference);
      if (match.Success)
      {
        return [int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)];
      }
      return [0, 0];
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
