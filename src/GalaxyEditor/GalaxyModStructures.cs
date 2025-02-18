using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Utilities.Logging;
using X4DataLoader;
using X4Map;

namespace GalaxyEditor
{
  public class ExtensionInfo(string name, string id, int version, bool required) : INotifyPropertyChanged
  {
    private string _name = name;
    public string Name
    {
      get => _name;
      set
      {
        _name = value;
        NotifyPropertyChanged(nameof(Name));
      }
    }
    private string _id = id;
    public string Id
    {
      get => _id;
      set
      {
        _id = value;
        NotifyPropertyChanged(nameof(Id));
      }
    }
    private int _version = version;
    public int Version
    {
      get => _version;
      set
      {
        _version = value;
        NotifyPropertyChanged(nameof(Version));
      }
    }
    private bool _required = required;
    public bool Required
    {
      get => _required;
      set
      {
        _required = value;
        NotifyPropertyChanged(nameof(Required));
      }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void NotifyPropertyChanged(string propertyName)
    {
      OnPropertyChanged(propertyName);
    }
  }

  public class ExtensionInfoJsonConverter : JsonConverter<ExtensionInfo>
  {
    public override ExtensionInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      string name = string.Empty;
      string id = string.Empty;
      int version = 0;
      bool required = false;

      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndObject)
        {
          return new ExtensionInfo(name ?? string.Empty, id ?? string.Empty, version, required);
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          string propertyName = reader.GetString() ?? string.Empty;
          reader.Read();

          switch (propertyName)
          {
            case nameof(ExtensionInfo.Name):
              name = reader.GetString() ?? string.Empty;
              break;
            case nameof(ExtensionInfo.Id):
              id = reader.GetString() ?? string.Empty;
              break;
            case nameof(ExtensionInfo.Version):
              version = reader.GetInt32();
              break;
            case nameof(ExtensionInfo.Required):
              required = reader.GetBoolean();
              break;
          }
        }
      }

      throw new JsonException("Invalid JSON for ExtensionInfo");
    }

    public override void Write(Utf8JsonWriter writer, ExtensionInfo value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WriteString(nameof(ExtensionInfo.Name), value.Name);
      writer.WriteString(nameof(ExtensionInfo.Id), value.Id);
      writer.WriteNumber(nameof(ExtensionInfo.Version), value.Version);
      writer.WriteBoolean(nameof(ExtensionInfo.Required), value.Required);
      writer.WriteEndObject();
    }
  }

  public class ExtensionsInfoList(IEnumerable<ExtensionInfo> collection) : ObservableCollection<ExtensionInfo>(collection)
  {
    public void AddExtension(ExtensionInfo extension)
    {
      this.Add(extension);
    }

    public void RemoveExtension(ExtensionInfo extension)
    {
      this.Remove(extension);
    }

    public string ToJson()
    {
      return System.Text.Json.JsonSerializer.Serialize(this);
    }
  }

  public class ExtensionsInfoListJsonConverter : JsonConverter<ExtensionsInfoList>
  {
    public override ExtensionsInfoList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var extensions = JsonSerializer.Deserialize<List<ExtensionInfo>>(ref reader, options);
      return new ExtensionsInfoList(extensions ?? []);
    }

    public override void Write(Utf8JsonWriter writer, ExtensionsInfoList value, JsonSerializerOptions options)
    {
      JsonSerializer.Serialize(writer, value, options);
    }
  }

  public class MapInfoJsonConverter : JsonConverter<MapInfo>
  {
    public override MapInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      int minCol = 0,
        maxCol = 0,
        minRow = 0,
        maxRow = 0;

      while (reader.Read())
      {
        if (reader.TokenType == JsonTokenType.EndObject)
        {
          return new MapInfo(minCol, maxCol, minRow, maxRow);
        }

        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          string propertyName = reader.GetString() ?? string.Empty;
          reader.Read();

          switch (propertyName)
          {
            case nameof(MapInfo.ColumnMin):
              minCol = reader.GetInt32();
              break;
            case nameof(MapInfo.ColumnMax):
              maxCol = reader.GetInt32();
              break;
            case nameof(MapInfo.RowMin):
              minRow = reader.GetInt32();
              break;
            case nameof(MapInfo.RowMax):
              maxRow = reader.GetInt32();
              break;
          }
        }
      }

      throw new JsonException("Invalid JSON for MapInfo");
    }

    public override void Write(Utf8JsonWriter writer, MapInfo value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WriteNumber(nameof(MapInfo.ColumnMin), value.ColumnMin);
      writer.WriteNumber(nameof(MapInfo.ColumnMax), value.ColumnMax);
      writer.WriteNumber(nameof(MapInfo.RowMin), value.RowMin);
      writer.WriteNumber(nameof(MapInfo.RowMax), value.RowMax);
      writer.WriteEndObject();
    }
  }
}
