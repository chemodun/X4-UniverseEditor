using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Accessibility;
using Utilities.Logging;
using X4DataLoader;
using X4Map;

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
    Item,
    List,
    ListOfItems,
  }

  public class GalaxyUnifyItemAttribute
  {
    public AttributeType Type { get; set; } = AttributeType.String;
    public int Index { get; set; } = -1;
    public string Name { get; set; } = "";
    private string _valueString = "";
    private int _valueInt = 0;
    private double _valueDouble = 0;
    private bool _valueBool = false;
    public string ValueString { get; set; } = "";
    public int ValueInt { get; set; } = 0;
    public double ValueDouble { get; set; } = 0;
    public bool ValueBool { get; set; } = false;
    public GalaxyUnifyItemAttribute? ValueItem { get; set; } = null;
    public List<GalaxyUnifyItemAttribute> ValueList { get; set; } = [];
    public List<GalaxyUnifyItem> ValueListOfItems { get; set; } = [];
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
        case AttributeType.Item:
          ValueItem?.PostInit();
          break;
        case AttributeType.List:
          foreach (GalaxyUnifyItemAttribute item in ValueList)
          {
            item.PostInit();
          }
          break;
        case AttributeType.ListOfItems:
          foreach (GalaxyUnifyItem item in ValueListOfItems)
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
            case nameof(GalaxyUnifyItemAttribute.Name):
              attribute.Name = reader.GetString() ?? string.Empty;
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueString):
              attribute.ValueString = reader.GetString() ?? string.Empty;
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueInt):
              attribute.ValueInt = reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueDouble):
              attribute.ValueDouble = reader.GetDouble();
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueBool):
              attribute.ValueBool = reader.GetBoolean();
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueItem):
              attribute.ValueItem = JsonSerializer.Deserialize<GalaxyUnifyItemAttribute>(ref reader, options);
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueList):
              attribute.ValueList = JsonSerializer.Deserialize<List<GalaxyUnifyItemAttribute>>(ref reader, options) ?? [];
              break;
            case nameof(GalaxyUnifyItemAttribute.ValueListOfItems):
              attribute.ValueListOfItems =
                JsonSerializer.Deserialize<List<GalaxyUnifyItem>>(ref reader, options) ?? new List<GalaxyUnifyItem>();
              break;
            case nameof(GalaxyUnifyItemAttribute.State):
              attribute.State = (AttributeState)reader.GetInt32();
              break;
            case nameof(GalaxyUnifyItemAttribute.Type):
              attribute.Type = (AttributeType)reader.GetInt32();
              break;
          }
        }
      }
      throw new JsonException("Unable to read GalaxyItemAttribute");
    }

    public override void Write(Utf8JsonWriter writer, GalaxyUnifyItemAttribute value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.Type), (int)value.Type);
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.Index), value.Index);
      writer.WriteString(nameof(GalaxyUnifyItemAttribute.Name), value.Name);
      switch (value.Type)
      {
        case AttributeType.String:
          writer.WriteString(nameof(GalaxyUnifyItemAttribute.ValueString), value.ValueString);
          break;
        case AttributeType.Int:
          writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.ValueInt), value.ValueInt);
          break;
        case AttributeType.Double:
          writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.ValueDouble), value.ValueDouble);
          break;
        case AttributeType.Bool:
          writer.WriteBoolean(nameof(GalaxyUnifyItemAttribute.ValueBool), value.ValueBool);
          break;
        case AttributeType.Item:
          writer.WritePropertyName(nameof(GalaxyUnifyItemAttribute.ValueItem));
          JsonSerializer.Serialize(writer, value.ValueItem, options);
          break;
        case AttributeType.List:
          writer.WritePropertyName(nameof(GalaxyUnifyItemAttribute.ValueList));
          JsonSerializer.Serialize(writer, value.ValueList, options);
          break;
        case AttributeType.ListOfItems:
          writer.WritePropertyName(nameof(GalaxyUnifyItemAttribute.ValueListOfItems));
          JsonSerializer.Serialize(writer, value.ValueListOfItems, options);
          break;
      }
      writer.WriteNumber(nameof(GalaxyUnifyItemAttribute.State), (int)value.State);
      writer.WriteEndObject();
    }
  }

  public class GalaxyUnifyItem : INotifyPropertyChanged
  {
    public int Index = -1;
    public List<GalaxyUnifyItemAttribute> Attributes = [];

    public AttributeState State { get; set; } = AttributeState.None;

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

    public void SetInt(string name, int value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Int);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueInt == value;
        attribute.ValueInt = value;
        PostSetAttribute(attribute, isEqual);
        OnPropertyChanged(name);
      }
    }

    public void SetString(string name, string value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.String);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueString == value;
        attribute.ValueString = value;
        PostSetAttribute(attribute, isEqual);
        OnPropertyChanged(name);
      }
    }

    public void SetDouble(string name, double value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Double);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueDouble == value;
        attribute.ValueDouble = value;
        PostSetAttribute(attribute, isEqual);
        OnPropertyChanged(name);
      }
    }

    public void SetBool(string name, bool value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Bool);
      if (attribute != null)
      {
        bool isEqual = attribute.ValueBool == value;
        attribute.ValueBool = value;
        PostSetAttribute(attribute, isEqual);
        OnPropertyChanged(name);
      }
    }

    public void SetItem(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.Item);
      if (attribute != null)
      {
        attribute.ValueItem = value;
        PostSetAttribute(attribute, false);
        OnPropertyChanged(name);
      }
    }

    public void SetList(string name, List<GalaxyUnifyItemAttribute> value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.List);
      if (attribute != null)
      {
        attribute.ValueList = value;
        PostSetAttribute(attribute, false);
        OnPropertyChanged(name);
      }
    }

    public void SetListOfItems(string name, List<GalaxyUnifyItem> value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListOfItems);
      if (attribute != null)
      {
        attribute.ValueListOfItems = value;
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

    public GalaxyUnifyItemAttribute? GetItem(string name)
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
      GalaxyUnifyItemAttribute? attribute = PreGetAttributeValue(name, AttributeType.ListOfItems);
      if (attribute != null)
      {
        return attribute.ValueListOfItems.Where(a => a.State == AttributeState.Set || a.State == AttributeState.Modified).ToList();
      }
      return [];
    }

    public void AddToList(string name, GalaxyUnifyItemAttribute value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.List);
      if (attribute != null)
      {
        attribute.ValueList.Add(value);
        value.Index = attribute.ValueList.Count - 1;
        OnPropertyChanged(name);
      }
    }

    public void AddToListOfItems(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListOfItems);
      if (attribute != null)
      {
        attribute.ValueListOfItems.Add(value);
        value.Index = attribute.ValueListOfItems.Count - 1;
        OnPropertyChanged(name);
      }
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

    public void RemoveFromListOfItems(string name, GalaxyUnifyItem value)
    {
      GalaxyUnifyItemAttribute? attribute = PreSetAttribute(name, AttributeType.ListOfItems);
      if (attribute != null)
      {
        if (value.Index < attribute.ValueList.Count && value.Index >= 0)
        {
          GalaxyUnifyItem removed = attribute.ValueListOfItems[value.Index];
          removed.State = AttributeState.Unset;
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
  }

  public class GalaxyUnifyItemJsonConverter : JsonConverter<GalaxyUnifyItem>
  {
    public override GalaxyUnifyItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            case nameof(GalaxyUnifyItem.Attributes):
              item.Attributes =
                JsonSerializer.Deserialize<List<GalaxyUnifyItemAttribute>>(ref reader, options) ?? new List<GalaxyUnifyItemAttribute>();
              break;
          }
        }
      }
      throw new JsonException("Unable to read GalaxyItemInfo");
    }

    public override void Write(Utf8JsonWriter writer, GalaxyUnifyItem value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WritePropertyName(nameof(GalaxyUnifyItem.Attributes));
      JsonSerializer.Serialize(writer, value.Attributes, options);
      writer.WriteEndObject();
    }
  }
}
