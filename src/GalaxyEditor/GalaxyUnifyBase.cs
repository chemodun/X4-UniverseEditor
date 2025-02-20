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
    List,
  }

  public class GalaxyUnifyItemAttribute
  {
    public AttributeType Type { get; set; } = AttributeType.String;
    public int Index { get; set; } = -1;
    public string Name { get; set; } = "";
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
    public List<GalaxyUnifyItemAttribute> ValueList { get; set; } = [];
    public AttributeState State { get; set; } = AttributeState.None;

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
        case AttributeType.List:
          foreach (GalaxyUnifyItemAttribute item in ValueList)
          {
            item.PostInit();
          }
          break;
      }
      State = AttributeState.Set;
    }
  }

  public class GalaxyUnifyItemAttributeConverter : JsonConverter<GalaxyUnifyItemAttribute>
  {
    public override GalaxyUnifyItemAttribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var attribute = new GalaxyUnifyItemAttribute();
      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndObject)
        {
          return attribute;
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          string propertyName = reader.GetString() ?? throw new JsonException("Property name is null");
          reader.Read();
          switch (propertyName)
          {
            case nameof(GalaxyUnifyItemAttribute.Index):
              attribute.Index = reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItemAttribute.Type):
              attribute.Type = (AttributeType)reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItemAttribute.State):
              attribute.State = (AttributeState)reader.GetInt32();
              break;
            default:
              attribute.Name = propertyName;
              switch (attribute.Type)
              {
                case AttributeType.String:
                  attribute.ValueString = reader.GetString() ?? string.Empty;
                  break;
                case AttributeType.Int:
                  attribute.ValueInt = reader.GetInt32();
                  break;
                case AttributeType.Double:
                  attribute.ValueDouble = reader.GetDouble();
                  break;
                case AttributeType.Bool:
                  attribute.ValueBool = reader.GetBoolean();
                  break;
                case AttributeType.Attribute:
                  attribute.ValueAttribute = JsonSerializer.Deserialize<GalaxyUnifyItemAttribute>(ref reader, options);
                  break;
                case AttributeType.Item:
                  attribute.ValueItem = JsonSerializer.Deserialize<GalaxyUnifyItem>(ref reader, options);
                  break;
                case AttributeType.List:
                  attribute.ValueList = JsonSerializer.Deserialize<List<GalaxyUnifyItemAttribute>>(ref reader, options) ?? [];
                  break;
              }
              break;
          }
        }
      }
      throw new JsonException("Unable to read GalaxyItemAttribute");
    }

    public override void Write(Utf8JsonWriter writer, GalaxyUnifyItemAttribute value, JsonSerializerOptions options)
    {
      if (value.State == AttributeState.None || value.State == AttributeState.Set)
      {
        return;
      }
      writer.WriteStartObject();
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.Type), (int)value.Type);
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.State), (int)value.State);
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.Index), value.Index);
      switch (value.Type)
      {
        case AttributeType.String:
          writer.WriteString(value.Name, value.ValueString);
          break;
        case AttributeType.Int:
          if (value.ValueInt != null)
          {
            writer.WriteNumber(value.Name, value.ValueInt ?? 0);
          }
          break;
        case AttributeType.Double:
          if (value.ValueDouble != null)
          {
            writer.WriteNumber(value.Name, value.ValueDouble ?? 0);
          }
          break;
        case AttributeType.Bool:
          if (value.ValueBool != null)
          {
            writer.WriteBoolean(value.Name, value.ValueBool ?? false);
          }
          break;
        case AttributeType.Attribute:
          writer.WritePropertyName(value.Name);
          JsonSerializer.Serialize(writer, value.ValueAttribute, options);
          break;
        case AttributeType.Item:
          writer.WritePropertyName(value.Name);
          JsonSerializer.Serialize(writer, value.ValueItem, options);
          break;
        case AttributeType.List:
          writer.WritePropertyName(value.Name);
          JsonSerializer.Serialize(writer, value.ValueList, options);
          break;
      }
      writer.WriteEndObject();
    }
  }

  public class GalaxyUnifyItem : INotifyPropertyChanged
  {
    public int Index = -1;
    public List<GalaxyUnifyItemAttribute> Attributes = [];

    public AttributeState State { get; set; } = AttributeState.None;
    protected Translation? TranslationObject = null;
    protected GalaxyReferencesHolder? GalaxyReferences;

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

    public void PostSetAttribute(GalaxyUnifyItemAttribute attribute, bool valueIsEqual)
    {
      attribute.State = attribute.State switch
      {
        AttributeState.None => AttributeState.Set,
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
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.List);
      if (attribute != null)
      {
        attribute.ValueList = value;
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

    public List<GalaxyUnifyItemAttribute> GetList(string name)
    {
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.List);
      if (attribute != null)
      {
        return attribute.ValueList.Where(a => a.State == AttributeState.Set || a.State == AttributeState.Modified).ToList();
      }
      return [];
    }

    public List<GalaxyUnifyItem> GetListOfItems(string name)
    {
      List<GalaxyUnifyItemAttribute> list = GetList(name);
      if (list.Count > 0)
      {
        return list.Select(a => a.ValueItem).Where(a => a != null).Cast<GalaxyUnifyItem>().ToList();
      }
      return [];
    }

    public int AddToList(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.List);
      if (attribute != null)
      {
        attribute.ValueList.Add(value);
        value.Index = attribute.ValueList.Count - 1;
        OnPropertyChanged(name);
        return value.Index;
      }
      return -1;
    }

    public int AddToList(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute newItem = new() { Type = AttributeType.Item, ValueItem = value };
      value.Index = AddToList(name, newItem);
      return value.Index;
    }

    public void RemoveFromList(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.List);
      if (attribute != null)
      {
        if (value.Index < attribute.ValueList.Count && value.Index >= 0)
        {
          GalaxyUnifyItemAttribute removed = attribute.ValueList[value.Index];
          removed.State = AttributeState.Unset;
          OnPropertyChanged(name);
        }
      }
    }

    public void RemoveFromList(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.List);
      if (attribute != null)
      {
        if (value.Index < attribute.ValueList.Count && value.Index >= 0)
        {
          GalaxyUnifyItemAttribute removed = attribute.ValueList[value.Index];
          removed.State = AttributeState.Unset;
          value.State = AttributeState.Unset;
          OnPropertyChanged(name);
        }
        OnPropertyChanged(name);
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected virtual void OnPropertyChanged(string propertyName)
    {
      State = AttributeState.Modified;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void PostInit()
    {
      foreach (GalaxyUnifyItemAttribute attribute in Attributes)
      {
        attribute.PostInit();
      }
      State = AttributeState.Set;
    }

    public static GalaxyUnifyItem JsonRead(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var item = new GalaxyUnifyItem();
      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndObject)
        {
          return item;
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          string propertyName = reader.GetString() ?? throw new JsonException("Property name is null");
          reader.Read();

          switch (propertyName)
          {
            case nameof(GalaxyUnifyItem.Index):
              item.Index = reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItem.State):
              item.State = (AttributeState)reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItem.Attributes):
              item.Attributes =
                JsonSerializer.Deserialize<List<GalaxyUnifyItemAttribute>>(ref reader, options) ?? new List<GalaxyUnifyItemAttribute>();
              break;
          }
        }
      }
      throw new JsonException("Unable to read GalaxyItemInfo");
    }

    public static void JsonWrite(Utf8JsonWriter writer, GalaxyUnifyItem value, JsonSerializerOptions options)
    {
      if (value.State == AttributeState.None || value.State == AttributeState.Set)
      {
        return;
      }
      writer.WriteStartObject();
      writer.WriteNumber(nameof(GalaxyUnifyItem.State), (int)value.State);
      writer.WriteNumber(nameof(GalaxyUnifyItem.Index), value.Index);
      writer.WritePropertyName(nameof(GalaxyUnifyItem.Attributes));
      JsonSerializer.Serialize(writer, value.Attributes, options);
      writer.WriteEndObject();
    }
  }

  public class GalaxyUnifyItemJsonConverter : JsonConverter<GalaxyUnifyItem>
  {
    public override GalaxyUnifyItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return GalaxyUnifyItem.JsonRead(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, GalaxyUnifyItem value, JsonSerializerOptions options)
    {
      GalaxyUnifyItem.JsonWrite(writer, value, options);
    }
  }
}
