using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Utilities.Logging;

namespace X4XMLPatch
{
  class XMLPatch
  {
    private static void ProcessSingleDoc(XDocument originalXml, XDocument diffXML, string SourceId)
    {
      try
      {
        XElement originalRoot = originalXml.Root ?? throw new InvalidOperationException("originalXml.Root is null");
        XElement diffRoot = diffXML.Root ?? throw new InvalidOperationException("diffXML.Root is null");

        foreach (var operation in diffRoot.Elements())
        {
          switch (operation.Name.LocalName)
          {
            case "add":
              ApplyAdd(operation, originalRoot, SourceId);
              break;
            case "replace":
              ApplyReplace(operation, originalRoot, SourceId);
              break;
            case "remove":
              ApplyRemove(operation, originalRoot);
              break;
            default:
              Log.Warn($"Unknown operation: {operation.Name}. Skipping.");
              break;
          }
        }

        Log.Debug($"Patched XML successfully.");
      }
      catch (Exception ex)
      {
        Log.Error($"Error processing XMLs: {ex.Message}");
      }
    }

    private static void ApplyAdd(XElement addElement, XElement originalRoot, string SourceId)
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
        return;
      }
      if (targetElements.Count() > 1)
      {
        Log.Warn($"Multiple nodes found for add selector: {sel}. Skipping.");
        return;
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
            return;
          }
          targetElement.SetAttributeValue(type, addElement.Value);
          Log.Debug($"Added attribute '{type}' with value '{addElement.Value}' to '{targetElement.Name}'.");
        }
      }
    }

    private static void ApplyReplace(XElement replaceElement, XElement originalRoot, string SourceId)
    {
      string? sel = replaceElement.Attribute("sel")?.Value;
      if (sel == null)
      {
        Log.Warn("Replace operation missing 'sel' attribute.");
        return;
      }

      var targetNodes = originalRoot.XPathEvaluate(sel) as IEnumerable<object>;
      if (targetNodes == null || !targetNodes.Any())
      {
        Log.Warn($"No nodes found for replace selector: {sel}");
        return;
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
    }

    private static void ApplyRemove(XElement removeElement, XElement originalRoot)
    {
      string? sel = removeElement.Attribute("sel")?.Value;
      if (sel == null)
      {
        Log.Warn("Remove operation missing 'sel' attribute.");
        return;
      }

      var targetNodes = originalRoot.XPathEvaluate(sel) as IEnumerable<object>;
      if (targetNodes == null || !targetNodes.Any())
      {
        Log.Warn($"No nodes found for remove selector: {sel}");
        return;
      }

      foreach (var targetObj in targetNodes)
      {
        if (targetObj is XElement target)
        {
          XElement? parent = target.Parent;
          if (parent == null)
          {
            Log.Warn($"Element '{target.Name}' has no parent. Cannot remove.");
            continue;
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
            continue;
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
    }
  }
}
