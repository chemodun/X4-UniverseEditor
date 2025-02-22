using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using X4DataLoader;

namespace GalaxyEditor
{
  public enum AttributeState
  {
    None,
    Set,
    Unset,
    Modified,
  }

  public enum AttributeType
  {
    String,
    Int,
    Double,
    Bool,
    Attribute,
    Item,
    ListAttributes,
    ListItems,
  }

  public class GalaxyUnifyItemAttribute(string name, AttributeType type, bool isMandatory = false, string group = "")
  {
    public AttributeType Type { get; set; } = type;
    public AttributeState State { get; set; } = AttributeState.None;
    public int Index { get; set; } = -1;
    public bool IsMandatory { get; set; } = isMandatory;
    public string Group { get; set; } = group;
    public string Name { get; set; } = name;
    private string? _valueString = null;
    private int? _valueInt = null;
    private double? _valueDouble = null;
    private bool? _valueBool = null;
    public string? ValueString { get; set; } = null;
    public int? ValueInt { get; set; } = null;
    public double? ValueDouble { get; set; } = null;
    public bool? ValueBool { get; set; } = null;
    public GalaxyUnifyItemAttribute? ValueAttribute { get; set; } = null;
    public GalaxyUnifyItem? ValueItem { get; set; } = null;
    public List<GalaxyUnifyItemAttribute> ListAttributes { get; set; } = [];
    public List<GalaxyUnifyItem> ListItems { get; set; } = [];

    public void PostInit()
    {
      switch (Type)
      {
        case AttributeType.Int:
          _valueInt = ValueInt;
          break;
        case AttributeType.String:
          _valueString = ValueString;
          break;
        case AttributeType.Double:
          _valueDouble = ValueDouble;
          break;
        case AttributeType.Bool:
          _valueBool = ValueBool;
          break;
        case AttributeType.Attribute:
          ValueAttribute?.PostInit();
          break;
        case AttributeType.Item:
          ValueItem?.PostInit();
          break;
        case AttributeType.ListAttributes:
          foreach (GalaxyUnifyItemAttribute item in ListAttributes)
          {
            item.PostInit();
          }
          break;
        case AttributeType.ListItems:
          foreach (GalaxyUnifyItem item in ListItems)
          {
            item.PostInit();
          }
          break;
      }
      State = AttributeState.Set;
    }

    public bool IsHasValue()
    {
      switch (Type)
      {
        case AttributeType.String:
          return !string.IsNullOrEmpty(ValueString);
        case AttributeType.Int:
          return ValueInt != null;
        case AttributeType.Double:
          return ValueDouble != null;
        case AttributeType.Bool:
          return ValueBool != null;
        case AttributeType.Attribute:
          return ValueAttribute != null;
        case AttributeType.Item:
          return ValueItem != null;
        case AttributeType.ListAttributes:
          return ListAttributes.Count > 0;
        case AttributeType.ListItems:
          return ListItems.Count > 0;
      }
      return false;
    }

    public void UpdateFrom(GalaxyUnifyItemAttribute other)
    {
      State = other.State;
      Index = other.Index;
      switch (Type)
      {
        case AttributeType.String:
          if (ValueString != other.ValueString)
          {
            ValueString = other.ValueString;
          }
          break;
        case AttributeType.Int:
          if (ValueInt != other.ValueInt)
          {
            ValueInt = other.ValueInt;
          }
          break;
        case AttributeType.Double:
          if (ValueDouble != other.ValueDouble)
          {
            ValueDouble = other.ValueDouble;
          }
          break;
        case AttributeType.Bool:
          if (ValueBool != other.ValueBool)
          {
            ValueBool = other.ValueBool;
          }
          break;
        case AttributeType.Attribute:
          if (ValueAttribute != null && other.ValueAttribute != null)
          {
            ValueAttribute.UpdateFrom(other.ValueAttribute);
          }
          break;
        case AttributeType.Item:
          if (ValueItem != null && other.ValueItem != null)
          {
            ValueItem?.UpdateFrom(other.ValueItem);
          }
          break;
        case AttributeType.ListAttributes:
          for (int i = 0; i < ListAttributes.Count; i++)
          {
            GalaxyUnifyItemAttribute attribute = ListAttributes[i];
            GalaxyUnifyItemAttribute? otherAttribute = other.ListAttributes.Find(a => a.Index == attribute.Index);
            if (otherAttribute != null)
            {
              attribute.UpdateFrom(otherAttribute);
            }
          }
          for (int i = ListAttributes.Count; i < other.ListAttributes.Count; i++)
          {
            GalaxyUnifyItemAttribute otherAttribute = other.ListAttributes[i];
            GalaxyUnifyItemAttribute attribute = new GalaxyUnifyItemAttribute(
              otherAttribute.Name,
              otherAttribute.Type,
              otherAttribute.IsMandatory,
              otherAttribute.Group
            );
            attribute.UpdateFrom(other.ListAttributes[i]);
            ListAttributes.Add(attribute);
          }
          break;
        case AttributeType.ListItems:
          for (int i = 0; i < ListItems.Count; i++)
          {
            GalaxyUnifyItem item = ListItems[i];
            GalaxyUnifyItem? otherItem = other.ListItems.Find(a => a.Index == item.Index);
            if (otherItem != null)
            {
              item.UpdateFrom(otherItem);
            }
          }
          for (int i = ListItems.Count; i < other.ListItems.Count; i++)
          {
            GalaxyUnifyItem item = new GalaxyUnifyItem();
            item.CopyFrom(other.ListItems[i]);
            ListItems.Add(item);
          }
          break;
      }
    }

    public void Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndObject)
        {
          return;
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          string propertyName = reader.GetString() ?? throw new JsonException("Property name is null");
          reader.Read();
          switch (propertyName)
          {
            case nameof(GalaxyUnifyItemAttribute.Index):
              Index = reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItemAttribute.Type):
              Type = (AttributeType)reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItemAttribute.State):
              State = (AttributeState)reader.GetInt32();
              break;
            default:
              Name = propertyName;
              switch (Type)
              {
                case AttributeType.String:
                  ValueString = reader.GetString() ?? string.Empty;
                  break;
                case AttributeType.Int:
                  ValueInt = reader.GetInt32();
                  break;
                case AttributeType.Double:
                  ValueDouble = reader.GetDouble();
                  break;
                case AttributeType.Bool:
                  ValueBool = reader.GetBoolean();
                  break;
                case AttributeType.Attribute:
                  ValueAttribute = JsonSerializer.Deserialize<GalaxyUnifyItemAttribute>(ref reader, options);
                  break;
                case AttributeType.Item:
                  ValueItem = JsonSerializer.Deserialize<GalaxyUnifyItem>(ref reader, options);
                  break;
                case AttributeType.ListAttributes:
                  ListAttributes = JsonSerializer.Deserialize<List<GalaxyUnifyItemAttribute>>(ref reader, options) ?? [];
                  break;
                case AttributeType.ListItems:
                  ListItems = JsonSerializer.Deserialize<List<GalaxyUnifyItem>>(ref reader, options) ?? [];
                  break;
              }
              break;
          }
        }
      }
      throw new JsonException("Unable to read GalaxyItemAttribute");
    }

    public void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      if (State == AttributeState.None || State == AttributeState.Set)
      {
        return;
      }
      writer.WriteStartObject();
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.Type), (int)Type);
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.State), (int)State);
      if (Index >= 0)
      {
        writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.Index), Index);
      }
      switch (Type)
      {
        case AttributeType.String:
          writer.WriteString(Name, ValueString);
          break;
        case AttributeType.Int:
          if (ValueInt != null)
          {
            writer.WriteNumber(Name, ValueInt ?? 0);
          }
          break;
        case AttributeType.Double:
          if (ValueDouble != null)
          {
            writer.WriteNumber(Name, ValueDouble ?? 0);
          }
          break;
        case AttributeType.Bool:
          if (ValueBool != null)
          {
            writer.WriteBoolean(Name, ValueBool ?? false);
          }
          break;
        case AttributeType.Attribute:
          writer.WritePropertyName(Name);
          JsonSerializer.Serialize(writer, ValueAttribute, options);
          break;
        case AttributeType.Item:
          writer.WritePropertyName(Name);
          JsonSerializer.Serialize(writer, ValueItem, options);
          break;
        case AttributeType.ListAttributes:
          writer.WritePropertyName(Name);
          JsonSerializer.Serialize(writer, ListAttributes, options);
          break;
        case AttributeType.ListItems:
          writer.WritePropertyName(Name);
          JsonSerializer.Serialize(writer, ListItems, options);
          break;
      }
      writer.WriteEndObject();
    }
  }

  public class GalaxyUnifyItemAttributeConverter : JsonConverter<GalaxyUnifyItemAttribute>
  {
    public override GalaxyUnifyItemAttribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var attribute = new GalaxyUnifyItemAttribute("", AttributeType.String);
      attribute.Read(ref reader, typeToConvert, options);
      return attribute;
    }

    public override void Write(Utf8JsonWriter writer, GalaxyUnifyItemAttribute value, JsonSerializerOptions options)
    {
      value.Write(writer, options);
    }
  }

  public class GalaxyUnifyItem : INotifyPropertyChanged
  {
    public int Index = -1;
    public List<GalaxyUnifyItemAttribute> Attributes = [];
    public string ItemId = "";

    public AttributeState State { get; set; } = AttributeState.None;
    protected Translation? TranslationObject = null;
    protected GalaxyReferencesHolder? GalaxyReferences;
    public static readonly Dictionary<string, Type> TypeMappings = new() { { "Item", typeof(GalaxyUnifyItem) } };

    public void Connect(Translation translation, GalaxyReferencesHolder galaxyReferences)
    {
      TranslationObject = translation;
      GalaxyReferences = galaxyReferences;
    }

    public GalaxyUnifyItemAttribute? PreSetAttribute(string name, AttributeType? type)
    {
      GalaxyUnifyItemAttribute? attribute = Attributes.Find(a => a.Name == name && (type == null || a.Type == type));
      if (attribute != null)
      {
        return attribute;
      }
      return null;
    }

    public void SetModified(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, null);
      if (attribute != null)
      {
        attribute.State = AttributeState.Modified;
      }
    }

    public void PostSetAttribute(GalaxyUnifyItemAttribute attribute, bool valueIsEqual)
    {
      attribute.State = attribute.State switch
      {
        AttributeState.None => AttributeState.Modified,
        AttributeState.Set => valueIsEqual ? AttributeState.Set : AttributeState.Modified,
        AttributeState.Unset => AttributeState.Modified,
        _ => attribute.State,
      };
    }

    public void Set(string name, int value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Int);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueInt == value;
        attribute.ValueInt = value;
        PostSetAttribute(attribute, isEqual);
        if (!isEqual)
        {
          OnPropertyChanged(name);
        }
      }
    }

    public void Set(string name, string value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.String);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueString == value;
        attribute.ValueString = value;
        PostSetAttribute(attribute, isEqual);
        if (!isEqual)
        {
          OnPropertyChanged(name);
        }
      }
    }

    public void Set(string name, double value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Double);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueDouble == value;
        attribute.ValueDouble = value;
        PostSetAttribute(attribute, isEqual);
        if (!isEqual)
        {
          OnPropertyChanged(name);
        }
      }
    }

    public void Set(string name, bool value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Bool);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueBool == value;
        attribute.ValueBool = value;
        PostSetAttribute(attribute, isEqual);
        if (!isEqual)
        {
          OnPropertyChanged(name);
        }
      }
    }

    public void Set(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Attribute);
      if (attribute != null) //ToDo - check if value is equal
      {
        attribute.ValueAttribute = value;
        PostSetAttribute(attribute, false);
        OnPropertyChanged(name);
      }
    }

    public void Set(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Item);
      if (attribute != null)
      {
        attribute.ValueItem = value;
        PostSetAttribute(attribute, false);
        OnPropertyChanged(name);
      }
    }

    public void Set(string name, List<GalaxyUnifyItemAttribute> value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListAttributes);
      if (attribute != null)
      {
        attribute.ListAttributes = value;
        PostSetAttribute(attribute, false);
        OnPropertyChanged(name);
      }
    }

    public void Set(string name, List<GalaxyUnifyItem> value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListItems);
      if (attribute != null)
      {
        attribute.ListItems = value;
        PostSetAttribute(attribute, false);
        OnPropertyChanged(name);
      }
    }

    public GalaxyUnifyItemAttribute? PreGetAttributeValue(string name, AttributeType type)
    {
      GalaxyUnifyItemAttribute? attribute = Attributes.Find(a => a.Name == name && a.Type == type);
      if (attribute != null && (attribute.State == AttributeState.Set || attribute.State == AttributeState.Modified))
      {
        return attribute;
      }
      return null;
    }

    public bool IsModified(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, null);
      if (attribute != null)
      {
        return attribute.State == AttributeState.Modified;
      }
      return false;
    }

    public bool isUnset(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, null);
      if (attribute != null)
      {
        return attribute.State == AttributeState.Unset;
      }
      return false;
    }

    public int? GetInt(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.Int);
      if (attribute != null)
      {
        return attribute.ValueInt;
      }
      return null;
    }

    public string? GetString(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.String);
      if (attribute != null)
      {
        return attribute.ValueString;
      }
      return null;
    }

    public double? GetDouble(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.Double);
      if (attribute != null)
      {
        return attribute.ValueDouble;
      }
      return null;
    }

    public bool? GetBool(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.Bool);
      if (attribute != null)
      {
        return attribute.ValueBool;
      }
      return null;
    }

    public GalaxyUnifyItemAttribute? GetAttribute(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.Attribute);
      if (attribute != null)
      {
        return attribute.ValueAttribute;
      }
      return null;
    }

    public GalaxyUnifyItem? GetItem(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.Item);
      if (attribute != null)
      {
        return attribute.ValueItem;
      }
      return null;
    }

    public List<GalaxyUnifyItemAttribute> GetListOfAttributes(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.ListAttributes);
      if (attribute != null)
      {
        return attribute.ListAttributes.Where(a => a.State == AttributeState.Set || a.State == AttributeState.Modified).ToList();
      }
      return [];
    }

    public List<GalaxyUnifyItem> GetListOfItems(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.ListItems);
      if (attribute != null)
      {
        return attribute.ListItems.Where(a => a.State == AttributeState.Set || a.State == AttributeState.Modified).ToList();
      }
      return [];
    }

    public int AddToList(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListAttributes);
      if (attribute != null)
      {
        attribute.ListAttributes.Add(value);
        value.Index = attribute.ListAttributes.Count - 1;
        OnPropertyChanged(name);
        return value.Index;
      }
      return -1;
    }

    public int AddToList(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListItems);
      if (attribute != null)
      {
        attribute.ListItems.Add(value);
        value.Index = attribute.ListItems.Count - 1;
        OnPropertyChanged(name);
        return value.Index;
      }
      return -1;
    }

    public void RemoveFromList(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListAttributes);
      if (attribute != null)
      {
        if (value.Index < attribute.ListAttributes.Count && value.Index >= 0)
        {
          GalaxyUnifyItemAttribute removed = attribute.ListAttributes[value.Index];
          removed.State = AttributeState.Unset;
          OnPropertyChanged(name);
        }
      }
    }

    public void RemoveFromList(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListItems);
      if (attribute != null)
      {
        if (value.Index < attribute.ListItems.Count && value.Index >= 0)
        {
          GalaxyUnifyItem removed = attribute.ListItems[value.Index];
          removed.State = AttributeState.Unset;
          OnPropertyChanged(name);
        }
      }
    }

    public void UpdateInList(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListAttributes);
      if (attribute != null)
      {
        if (value.Index < attribute.ListAttributes.Count && value.Index >= 0)
        {
          GalaxyUnifyItemAttribute updated = attribute.ListAttributes[value.Index];
          updated.UpdateFrom(value);
          OnPropertyChanged(name);
        }
      }
    }

    public void UpdateInList(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListItems);
      if (attribute != null)
      {
        if (value.Index < attribute.ListItems.Count && value.Index >= 0)
        {
          GalaxyUnifyItem updated = attribute.ListItems[value.Index];
          updated.UpdateFrom(value);
          OnPropertyChanged(name);
        }
      }
    }

    public void UpdateFrom(GalaxyUnifyItem other)
    {
      if (State == AttributeState.Set || State == AttributeState.None)
      {
        return;
      }
      State = other.State;
      ItemId = other.ItemId;
      Index = other.Index;
      foreach (
        GalaxyUnifyItemAttribute attribute in Attributes.Where(a => a.State == AttributeState.Unset || a.State == AttributeState.Modified)
      )
      {
        GalaxyUnifyItemAttribute? otherAttribute = other.PreSetAttribute(attribute.Name, attribute.Type);
        if (otherAttribute != null)
        {
          attribute.UpdateFrom(otherAttribute);
        }
      }
    }

    public void CopyFrom(GalaxyUnifyItem other)
    {
      State = other.State;
      ItemId = other.ItemId;
      Index = other.Index;
      foreach (GalaxyUnifyItemAttribute otherAttribute in other.Attributes)
      {
        GalaxyUnifyItemAttribute? attribute = PreSetAttribute(otherAttribute.Name, otherAttribute.Type);
        if (attribute == null)
        {
          attribute = new GalaxyUnifyItemAttribute(
            otherAttribute.Name,
            otherAttribute.Type,
            otherAttribute.IsMandatory,
            otherAttribute.Group
          );
          Attributes.Add(attribute);
        }
        attribute.UpdateFrom(otherAttribute);
      }
    }

    public void PostInit()
    {
      foreach (GalaxyUnifyItemAttribute attribute in Attributes)
      {
        attribute.PostInit();
      }
      State = AttributeState.Set;
    }

    public static GalaxyUnifyItem? ReadByType(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options,
      Dictionary<string, Type> typeMapping
    )
    {
      using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
      {
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("$type", out JsonElement typeElement))
        {
          string? typeName = typeElement.GetString();
          if (
            typeName != null
            && GalaxyUnifyItem.TypeMappings.TryGetValue(typeName, out Type? targetType)
            && targetType != null
            && targetType != typeToConvert
          )
          {
            GalaxyUnifyItem? result = JsonSerializer.Deserialize(root.GetRawText(), targetType, options) as GalaxyUnifyItem;
            return result;
          }
        }
        return null;
      }
    }

    public bool IsReady()
    {
      List<GalaxyUnifyItemAttribute> mandatoryAttributes = Attributes.Where(a => a.IsMandatory).ToList();
      List<string> groups = mandatoryAttributes.Select(a => a.Group).Distinct().ToList().Where(g => !string.IsNullOrEmpty(g)).ToList();
      foreach (string group in groups)
      {
        List<GalaxyUnifyItemAttribute> groupAttributes = mandatoryAttributes.Where(a => a.Group == group).ToList();
        if (!groupAttributes.Any(a => a.IsHasValue()))
        {
          return false;
        }
      }
      List<GalaxyUnifyItemAttribute> noGroupAttributes = mandatoryAttributes.Where(a => a.Group == "").ToList();
      if (noGroupAttributes.All(a => a.IsHasValue()))
      {
        return true;
      }
      return false;
    }

    public virtual void Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndObject)
        {
          return;
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          string propertyName = reader.GetString() ?? throw new JsonException("Property name is null");
          reader.Read();

          switch (propertyName)
          {
            case nameof(GalaxyUnifyItem.ItemId):
              ItemId = reader.GetString() ?? string.Empty;
              break;
            case nameof(GalaxyUnifyItem.Index):
              Index = reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItem.State):
              State = (AttributeState)reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItem.Attributes):
              List<object> attributes = JsonSerializer.Deserialize<List<object>>(ref reader, options) ?? new List<object>();
              foreach (object attribute in attributes)
              {
                string? attributeString = attribute.ToString();
                if (!string.IsNullOrEmpty(attributeString))
                {
                  GalaxyUnifyItemAttribute? itemAttribute = JsonSerializer.Deserialize<GalaxyUnifyItemAttribute>(attributeString, options);
                  if (itemAttribute != null)
                  {
                    GalaxyUnifyItemAttribute? attributeExisting = Attributes.Find(a => a.Name == itemAttribute.Name) ?? itemAttribute;
                    if (attributeExisting != null)
                    {
                      attributeExisting.UpdateFrom(itemAttribute);
                    }
                  }
                }
              }
              break;
          }
        }
      }
      throw new JsonException("Unable to read GalaxyItemInfo");
    }

    public virtual void Write(Utf8JsonWriter writer, JsonSerializerOptions options, string? type = null)
    {
      if (State == AttributeState.None || State == AttributeState.Set)
      {
        return;
      }
      writer.WriteStartObject();
      writer.WriteString("$type", type ?? "Item");
      if (!string.IsNullOrEmpty(ItemId))
      {
        writer.WriteString(nameof(GalaxyUnifyItem.ItemId), ItemId);
      }
      if (Index >= 0)
      {
        writer.WriteNumber(nameof(GalaxyUnifyItem.Index), Index);
      }
      writer.WriteNumber(nameof(GalaxyUnifyItem.State), (int)State);
      writer.WritePropertyName(nameof(GalaxyUnifyItem.Attributes));
      JsonSerializer.Serialize(writer, Attributes, options);
      writer.WriteEndObject();
    }

    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected virtual void OnPropertyChanged(string propertyName)
    {
      State = AttributeState.Modified;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class GalaxyUnifyItemJsonConverter : JsonConverter<GalaxyUnifyItem>
  {
    private static readonly Dictionary<string, Type> TypeMappings = new()
    {
      { "Moon", typeof(UnifyItemMoon) },
      { "Planet", typeof(UnifyItemPlanet) },
      { "Cluster", typeof(UnifyItemCluster) },
    };

    public override GalaxyUnifyItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      GalaxyUnifyItem? result = null;
      using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
      {
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("$type", out JsonElement typeElement))
        {
          string typeName = typeElement.GetString() ?? throw new JsonException("Type name is null");
          if (TypeMappings.TryGetValue(typeName, out Type? targetType) && targetType != null)
          {
            result = (GalaxyUnifyItem)JsonSerializer.Deserialize(root.GetRawText(), targetType, options)!;
          }
        }
      }
      if (result == null)
      {
        result = new GalaxyUnifyItem();
        result.Read(ref reader, typeToConvert, options);
      }
      return result;
    }

    public override void Write(Utf8JsonWriter writer, GalaxyUnifyItem value, JsonSerializerOptions options)
    {
      value.Write(writer, options);
    }
  }
}
