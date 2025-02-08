using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class StationGroup
  {
    public string Name { get; private set; }
    public List<(ConstructionPlan plan, int weight)> ConstructionPlans { get; private set; } = [];
    public string Source { get; private set; }
    public string FileName { get; private set; }
    public XElement? XML { get; set; }

    public StationGroup()
    {
      Name = "";
      Source = "";
      FileName = "";
      XML = null;
    }

    public void Load(XElement element, string source, string fileName, List<ConstructionPlan> allConstructionPlans)
    {
      Name = XmlHelper.GetAttribute(element, "name") ?? "";
      foreach (XElement selectElement in element.Elements("select"))
      {
        string constructionPlanId = XmlHelper.GetAttribute(selectElement, "constructionplan") ?? "";
        ConstructionPlan? constructionPlan = allConstructionPlans.FirstOrDefault(cp => cp.Id == constructionPlanId);
        int weight = StringHelper.ParseInt(XmlHelper.GetAttribute(selectElement, "weight"), 100);
        if (constructionPlan != null)
        {
          ConstructionPlans.Add((constructionPlan, weight));
        }
      }
      Source = source;
      FileName = fileName;
      XML = element;
    }

    public ConstructionPlan? GetMostWeightedConstructionPlan()
    {
      if (ConstructionPlans.Count == 0)
      {
        return null;
      }
      ConstructionPlan? mostWeightedConstructionPlan = null;
      int maxWeight = 0;
      foreach ((ConstructionPlan plan, int weight) in ConstructionPlans)
      {
        if (weight > maxWeight)
        {
          mostWeightedConstructionPlan = plan;
          maxWeight = weight;
        }
      }
      return mostWeightedConstructionPlan;
    }

    public static void LoadElements(
      IEnumerable<XElement> elements,
      string source,
      string fileName,
      List<StationGroup> allGroups,
      List<ConstructionPlan> allConstructionPlans
    )
    {
      foreach (XElement element in elements)
      {
        StationGroup group = new();
        group.Load(element, source, fileName, allConstructionPlans);
        if (group.Name == "")
        {
          Log.Warn($"StationGroup must have a name");
          continue;
        }
        if (allGroups.Any(x => x.Name == group.Name))
        {
          Log.Warn($"Duplicate StationGroup name {group.Name}");
          continue;
        }
        allGroups.Add(group);
      }
    }
  }
}
