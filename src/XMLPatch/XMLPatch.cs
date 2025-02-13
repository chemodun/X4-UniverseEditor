using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Utilities.Logging;

namespace Utilities.X4XMLPatch
{
  public static class XMLPatch
  {
    public static bool ApplyPatch(XElement originalRoot, XElement diffRoot, string SourceId)
    {
      try
      {
        XElement workingRoot = new XElement("root");
        workingRoot.Add(originalRoot);
        Log.Debug("Applying XML patch...");
        foreach (var operation in diffRoot.Elements())
        {
          switch (operation.Name.LocalName)
          {
            case "add":
              if (!ApplyAdd(operation, workingRoot, SourceId))
              {
                return false;
              }
              break;
            case "replace":
              if (!ApplyReplace(operation, workingRoot, SourceId))
              {
                return false;
              }
              break;
            case "remove":
              if (!ApplyRemove(operation, workingRoot))
              {
                return false;
              }
              break;
            default:
              Log.Warn($"Unknown operation: {operation.Name}. Skipping.");
              return false;
          }
        }

        Log.Debug($"Patched XML successfully.");
        return true;
      }
      catch (Exception ex)
      {
        Log.Error($"Error processing XMLs: {ex.Message}");
        return false;
      }
    }

    private static bool ApplyAdd(XElement addElement, XElement originalRoot, string SourceId)
    {
      string sel = addElement.Attribute("sel")?.Value ?? throw new ArgumentException("The 'sel' attribute is required.");
      string? type = addElement.Attribute("type")?.Value;
      string? pos = addElement.Attribute("pos")?.Value;
      if (pos == null && type == null)
      {
        pos = "append";
      }

      Log.Debug($"Applying add operation: {sel} at {pos!}");

      var targetElements = originalRoot.XPathSelectElements(sel);
      if (targetElements == null || !targetElements.Any())
      {
        Log.Warn($"No nodes found for add selector: {sel}");
        return false;
      }
      if (targetElements.Count() > 1)
      {
        Log.Warn($"Multiple nodes found for add selector: {sel}. Skipping.");
        return false;
      }
      var targetElement = targetElements.First();
      if (pos != null)
      {
        var newElements = addElement.Elements();
        foreach (var newElem in newElements)
        {
          XElement cloned = new XElement(newElem);
          cloned.Add(new XAttribute("_source", SourceId));
          if (pos == "before")
          {
            targetElement.AddBeforeSelf(cloned);
            Log.Debug($"Added new element '{cloned.Name}' before '{targetElement.Name}' in '{targetElement.Parent?.Name}'.");
          }
          else if (pos == "after")
          {
            targetElement.AddAfterSelf(cloned);
            Log.Debug($"Added new element '{cloned.Name}' after '{targetElement.Name}' in '{targetElement.Parent?.Name}'.");
          }
          else if (pos == "prepend")
          {
            targetElement.AddFirst(cloned);
            Log.Debug($"Prepended new element '{cloned.Name}' to '{targetElement.Name}'.");
          }
          else if (pos == "append")
          {
            targetElement.Add(cloned);
            Log.Debug($"Appended new element '{cloned.Name}' to '{targetElement.Name}'.");
          }
          else
          {
            Log.Warn($"Unknown position: {pos}. Skipping insertion.");
            return false;
          }
        }
      }
      else if (type != null)
      {
        if (type.StartsWith('@') && type.Length > 1)
        {
          type = type.Substring(1);
          if (addElement.Value == null)
          {
            Log.Warn("Attribute add operation missing value.");
            return false;
          }
          targetElement.SetAttributeValue(type, addElement.Value);
          Log.Debug($"Added attribute '{type}' with value '{addElement.Value}' to '{targetElement.Name}'.");
        }
      }
      return true;
    }

    private static bool ApplyReplace(XElement replaceElement, XElement originalRoot, string SourceId)
    {
      string? sel = replaceElement.Attribute("sel")?.Value;
      if (sel == null)
      {
        Log.Warn("Replace operation missing 'sel' attribute.");
        return false;
      }

      var targetNodes = originalRoot.XPathEvaluate(sel) as IEnumerable<object>;
      if (targetNodes == null || !targetNodes.Any())
      {
        Log.Warn($"No nodes found for replace selector: {sel}");
        return false;
      }

      foreach (var targetObj in targetNodes)
      {
        if (targetObj is XElement target)
        {
          var newContent = replaceElement.Value;
          if (!string.IsNullOrEmpty(newContent))
          {
            target.Value = newContent;
            Log.Debug($"Replaced text of element '{target.Name}' with '{newContent}'.");
          }

          var newElement = replaceElement.Element("new");
          if (newElement != null)
          {
            XElement replacement = new XElement(newElement);
            replacement.Add(new XAttribute("_source", SourceId));
            target.ReplaceWith(replacement);
            Log.Debug($"Replaced element '{target.Name}' with '{replacement.Name}'.");
          }
        }
        else if (targetObj is XText textNode)
        {
          textNode.Value = replaceElement.Value;
          Log.Debug("Replaced text node.");
        }
        else if (targetObj is XAttribute attr)
        {
          attr.Value = replaceElement.Value;
          Log.Debug($"Replaced attribute '{attr.Name}' with '{replaceElement.Value}'.");
        }
      }
      return true;
    }

    private static bool ApplyRemove(XElement removeElement, XElement originalRoot)
    {
      string? sel = removeElement.Attribute("sel")?.Value;
      if (sel == null)
      {
        Log.Warn("Remove operation missing 'sel' attribute.");
        return false;
      }

      var targetNodes = originalRoot.XPathEvaluate(sel) as IEnumerable<object>;
      if (targetNodes == null || !targetNodes.Any())
      {
        Log.Warn($"No nodes found for remove selector: {sel}");
        return false;
      }

      foreach (var targetObj in targetNodes)
      {
        if (targetObj is XElement target)
        {
          XElement? parent = target.Parent;
          if (parent == null)
          {
            Log.Warn($"Element '{target.Name}' has no parent. Cannot remove.");
            return false;
          }
          target.Remove();
          Log.Debug($"Removed element '{target.Name}' from '{parent.Name}'.");
        }
        else if (targetObj is XAttribute attr)
        {
          XElement? parent = attr.Parent;
          if (parent == null)
          {
            Log.Warn($"Attribute '{attr.Name}' has no parent. Cannot remove.");
            return false;
          }
          attr.Remove();
          Log.Debug($"Removed attribute '{attr.Name}' from '{parent.Name}'.");
        }
        else if (targetObj is XText textNode)
        {
          textNode.Remove();
          Log.Debug("Removed text node.");
        }
      }
      return true;
    }
  }
}
