using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class ConstructionPlan
  {
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Macro { get; private set; }
    public string Type { get; private set; }
    public bool IsClaimCapable { get; private set; } = false;
    public XElement? XML { get; set; }
    public string Source { get; private set; }
    public string FileName { get; private set; }

    public ConstructionPlan()
    {
      Id = "";
      Name = "";
      Description = "";
      Macro = "";
      Type = "";
      Source = "";
      FileName = "";
      XML = null;
    }

    public void Load(
      XElement element,
      string source,
      string fileName,
      Translation translation,
      List<StationModule> allModules,
      List<StationModuleGroup> allStationModuleGroups
    )
    {
      Id = XmlHelper.GetAttribute(element, "id") ?? "";
      Name = translation.Translate(XmlHelper.GetAttribute(element, "name") ?? "");
      Description = translation.Translate(XmlHelper.GetAttribute(element, "description") ?? "");
      Macro = XmlHelper.GetAttribute(element, "macro") ?? "";
      Type = XmlHelper.GetAttribute(element, "type") ?? "";
      foreach (XElement entryElement in element.Elements("entry"))
      {
        string macro = XmlHelper.GetAttribute(entryElement, "macro") ?? "";
        if (String.IsNullOrEmpty(macro))
        {
          Log.Warn($"ConstructionPlan {Id} entry must have a macro");
          continue;
        }
        List<string> moduleGroups = StationModuleGroup.GetModuleGroups(allStationModuleGroups, macro);
        if (moduleGroups.Count == 0)
        {
          Log.Warn($"ConstructionPlan {Id} entry {macro} must have a module group");
          continue;
        }
        foreach (string moduleGroup in moduleGroups)
        {
          if (StationModule.IsModuleWithGroupContainsTag(allModules, moduleGroup, "claim"))
          {
            IsClaimCapable = true;
            break;
          }
        }
        if (IsClaimCapable)
        {
          break;
        }
      }
      XML = element;
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
    }

    public static void LoadFromXML(GameFile file, Galaxy galaxy)
    {
      IEnumerable<XElement> elements = file.XML.XPathSelectElements("/plans/plan");
      foreach (XElement element in elements)
      {
        ConstructionPlan constructionPlan = new();
        constructionPlan.Load(
          element,
          file.Extension.Id,
          file.FileName,
          galaxy.Translation,
          galaxy.StationModules,
          galaxy.StationModuleGroups
        );
        if (constructionPlan.Name == "")
        {
          Log.Warn($"ConstructionPlan {constructionPlan.Id} must have a name");
          continue;
        }
        if (galaxy.ConstructionPlans.Any(cp => cp.Id == constructionPlan.Id))
        {
          Log.Warn($"ConstructionPlan {constructionPlan.Id} has a duplicate id");
          continue;
        }
        galaxy.ConstructionPlans.Add(constructionPlan);
      }
    }
  }
}
