using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public static XElement? ApplyPatch(XElement originalRoot, XElement diffRoot, string sourceId)
    {
      try
      {
        XElement workingRoot = new("root");
        workingRoot.Add(new XElement(originalRoot));
        Log.Debug("Applying XML patch...");
        foreach (var operation in diffRoot.Elements())
        {
          switch (operation.Name.LocalName)
          {
            case "add":
              if (!ApplyAdd(operation, workingRoot, sourceId))
              {
                continue;
              }
              break;
            case "replace":
              if (!ApplyReplace(operation, workingRoot, sourceId))
              {
                continue;
              }
              break;
            case "remove":
              if (!ApplyRemove(operation, workingRoot))
              {
                continue;
              }
              break;
            default:
              Log.Warn($"Unknown operation: {operation.Name}. Skipping.");
              continue;
          }
        }
        Log.Debug($"Patched XML successfully.");
        return workingRoot.Elements().First();
      }
      catch (Exception ex)
      {
        Log.Error($"Error processing XMLs: {ex.Message}");
        return null;
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

      IReadOnlyList<XElement> targetElements = SelectElements(originalRoot, sel);
      if (targetElements == null || !targetElements.Any())
      {
        Log.Warn(
          $"No nodes found for add selector: '{sel}'! Existing only: '{LastApplicableNode(sel, originalRoot)}'. Skipping operation."
        );
        return false;
      }
      if (targetElements.Count() > 1)
      {
        Log.Warn($"Multiple nodes found for add selector: {sel}. Skipping.");
        return false;
      }
      XElement targetElement = targetElements.First();
      if (pos != null)
      {
        IEnumerable<XElement> newElements = addElement.Elements();
        XElement? lastInserted = null;
        foreach (var newElem in newElements)
        {
          XElement cloned = new(newElem);
          string clonedInfo = GetElementInfo(cloned);
          string targetInfo = GetElementInfo(targetElement);
          string targetParentInfo = GetElementInfo(targetElement.Parent);
          if (cloned.Attribute("_source") == null)
          {
            cloned.Add(new XAttribute("_source", SourceId));
          }
          else
          {
            cloned.SetAttributeValue("_source", SourceId);
          }
          if (lastInserted != null)
          {
            lastInserted.AddAfterSelf(cloned);
            Log.Debug($"Added next element '{clonedInfo}' after '{GetElementInfo(lastInserted)}'.");
            lastInserted = cloned;
            continue;
          }
          switch (pos)
          {
            case "before":
            case "after":
              if (
                targetElement
                  .Parent!.Elements()
                  .Any(e =>
                    e.Name == cloned.Name
                    && e.Attributes().All(a => a.Name == "_source" || cloned.Attribute(a.Name)?.Value == a.Value)
                    && cloned.Attributes().All(a => a.Name == "_source" || e.Attribute(a.Name)?.Value == a.Value)
                  )
              )
              {
                Log.Warn($"Element '{clonedInfo}' already exists in '{targetParentInfo}'. Skipping.");
                continue;
              }
              if (pos == "before")
              {
                targetElement.AddBeforeSelf(cloned);
                Log.Debug($"Added new element '{clonedInfo}' before '{targetInfo}' in '{targetParentInfo}'.");
              }
              else
              {
                targetElement.AddAfterSelf(cloned);
                Log.Debug($"Added new element '{clonedInfo}' after '{targetInfo}' in '{targetParentInfo}'.");
              }
              lastInserted = cloned;
              break;
            case "prepend":
            case "append":
              if (
                targetElement
                  .Elements()
                  .Any(e =>
                    e.Name == cloned.Name
                    && e.Attributes().All(a => a.Name == "_source" || cloned.Attribute(a.Name)?.Value == a.Value)
                    && cloned.Attributes().All(a => a.Name == "_source" || e.Attribute(a.Name)?.Value == a.Value)
                  )
              )
              {
                Log.Warn($"Element '{clonedInfo}' already exists in '{targetInfo}'. Skipping.");
                continue;
              }
              if (pos == "prepend")
              {
                targetElement.AddFirst(cloned);
                Log.Debug($"Prepended new element '{clonedInfo}' to '{targetInfo}'.");
              }
              else
              {
                targetElement.Add(cloned);
                Log.Debug($"Appended new element '{clonedInfo}' to '{targetInfo}'.");
              }
              lastInserted = cloned;
              break;
            default:
              Log.Warn($"Unknown position: {pos}. Skipping insertion.");
              continue;
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

      IReadOnlyList<object> targetNodes = EvaluateNodes(originalRoot, sel);
      if (targetNodes == null || !targetNodes.Any())
      {
        Log.Warn(
          $"No nodes found for replace selector: '{sel}'! Existing only: '{LastApplicableNode(sel, originalRoot)}'. Skipping operation."
        );
        return false;
      }

      foreach (var targetObj in targetNodes)
      {
        if (targetObj is XElement target)
        {
          string targetName = target.Name.LocalName;
          XElement? replaceSubElement = replaceElement.Element(targetName);
          XElement? parent = target.Parent;
          string targetInfo = GetElementInfo(target);
          string parentInfo = GetElementInfo(parent);
          if (replaceSubElement != null)
          {
            if (replaceSubElement.Attribute("_source") == null)
            {
              replaceSubElement.Add(new XAttribute("_source", SourceId));
            }
            else
            {
              replaceSubElement.SetAttributeValue("_source", SourceId);
            }
            string replaceInfo = GetElementInfo(replaceSubElement);
            target.ReplaceWith(replaceSubElement);
            Log.Debug($"Replaced element '{targetInfo}' with '{replaceInfo}' in '{parentInfo}'.");
          }
          else
          {
            Log.Warn($"Can't process replacement for '{targetInfo}' in '{parentInfo}'. Skipping operation.");
          }
        }
        else if (targetObj is XText textNode)
        {
          textNode.Value = replaceElement.Value;
          Log.Debug("Replaced text node.");
        }
        else if (targetObj is XAttribute attr)
        {
          var oldValue = attr.Value;
          attr.Value = replaceElement.Value;
          Log.Debug($"Replaced attribute '{attr.Name}' from '{oldValue}' to '{replaceElement.Value}'.");
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

      IReadOnlyList<object> targetNodes = EvaluateNodes(originalRoot, sel);
      if (targetNodes == null || !targetNodes.Any())
      {
        Log.Warn(
          $"No nodes found for remove selector: '{sel}'! Existing only: '{LastApplicableNode(sel, originalRoot)}'. Skipping operation."
        );
        return false;
      }

      foreach (var targetObj in targetNodes)
      {
        if (targetObj is XElement target)
        {
          XElement? parent = target.Parent;
          string targetInfo = GetElementInfo(target);
          string parentInfo = GetElementInfo(parent);
          if (parent == null)
          {
            Log.Warn($"Element '{targetInfo}' has no parent. Cannot remove.");
            return false;
          }
          target.Remove();
          Log.Debug($"Removed element '{targetInfo}' from '{parentInfo}'.");
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
          Log.Debug($"Removed attribute '{attr.Name}' from '{GetElementInfo(parent)}'.");
        }
        else if (targetObj is XText textNode)
        {
          textNode.Remove();
          Log.Debug("Removed text node.");
        }
      }
      return true;
    }

    private static string GetElementInfo(XElement? element)
    {
      string info = "<";
      if (element != null)
      {
        info += $"{element.Name}";
        if (element.HasAttributes)
        {
          info += $" {element.FirstAttribute?.Name}=\"{element.FirstAttribute?.Value}\"";
          if (element.Attributes().Count() > 1)
          {
            info += " ...";
          }
        }
        info += ">";
      }
      return info;
    }

    private static string GetAttributeInfo(XAttribute? attr)
    {
      if (attr == null)
      {
        return "";
      }
      return $"{attr.Name}=\"{attr.Value}\"";
    }

    private static string LastApplicableNode(string selector, XElement originalRoot)
    {
      string lastApplicableNode = "";
      string[] parts = selector.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
      string xpath = "";
      foreach (var part in parts)
      {
        xpath += "/" + part;
        IReadOnlyList<XElement> nodes = SelectElements(originalRoot, xpath);
        if (nodes.Any())
        {
          lastApplicableNode = xpath;
        }
      }
      return lastApplicableNode;
    }

    private static IReadOnlyList<XElement> SelectElements(XElement root, string selector)
    {
      if (string.IsNullOrWhiteSpace(selector))
      {
        return Array.Empty<XElement>();
      }

      try
      {
        XPathNavigator navigator = root.CreateNavigator();
        XPathExpression expression = navigator.Compile(selector);
        if (navigator is IXmlNamespaceResolver resolver)
        {
          expression.SetContext(resolver);
        }

        object? evaluationResult = navigator.Evaluate(expression);
        if (evaluationResult is XPathNodeIterator iterator)
        {
          List<XElement> elements = [];
          while (iterator.MoveNext())
          {
            if (iterator.Current?.UnderlyingObject is XElement element)
            {
              elements.Add(element);
            }
          }
          return elements;
        }

        if (evaluationResult is IEnumerable<object> enumerable)
        {
          List<XElement> elements = [];
          foreach (object item in enumerable)
          {
            if (item is XElement element)
            {
              elements.Add(element);
            }
          }
          if (elements.Count > 0)
          {
            return elements;
          }
        }

        Log.Warn($"Selector '{selector}' evaluated to '{evaluationResult?.GetType().Name ?? "null"}', which is not a node-set.");
      }
      catch (XPathException ex)
      {
        Log.Warn($"Invalid XPath selector '{selector}': {ex.Message}");
      }

      return Array.Empty<XElement>();
    }

    private static IReadOnlyList<object> EvaluateNodes(XElement root, string selector)
    {
      if (string.IsNullOrWhiteSpace(selector))
      {
        return Array.Empty<object>();
      }

      try
      {
        XPathNavigator navigator = root.CreateNavigator();
        XPathExpression expression = navigator.Compile(selector);
        if (navigator is IXmlNamespaceResolver resolver)
        {
          expression.SetContext(resolver);
        }

        object? evaluationResult = navigator.Evaluate(expression);
        if (evaluationResult is XPathNodeIterator iterator)
        {
          List<object> nodes = [];
          while (iterator.MoveNext())
          {
            if (iterator.Current?.UnderlyingObject is XObject xObj)
            {
              nodes.Add(xObj);
            }
          }
          return nodes;
        }

        if (evaluationResult is IEnumerable<object> enumerable)
        {
          List<object> nodes = enumerable.OfType<XObject>().Cast<object>().ToList();
          if (nodes.Count > 0)
          {
            return nodes;
          }
        }

        Log.Warn($"Selector '{selector}' evaluated to '{evaluationResult?.GetType().Name ?? "null"}', which is not a node-set.");
      }
      catch (XPathException ex)
      {
        Log.Warn($"Invalid XPath selector '{selector}': {ex.Message}");
      }

      return Array.Empty<object>();
    }
  }
}
