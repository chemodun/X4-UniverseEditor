using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Utilities.Logging;

namespace X4Unpack
{
  public class CatEntry
  {
    public required string FilePath { get; set; }
    public long FileSize { get; set; }
    public long FileOffset { get; set; }
    public DateTime FileDate { get; set; }
    public required string FileHash { get; set; }
    public required string DatFilePath { get; set; }
  }

  public class ContentExtractor
  {
    protected string _folderPath;
    protected readonly Dictionary<string, CatEntry> _catalog;
    public int FileCount => _catalog.Count;
    private Regex catEntryRegex = new(@"^(.+?)\s(\d+)\s(\d+)\s([0-9a-fA-F]{32})$");

    public ContentExtractor(string folderPath, string pattern = "*.cat", bool excludeSignatures = true)
    {
      _folderPath = folderPath;
      _catalog = new Dictionary<string, CatEntry>();
      InitializeCatalog(pattern, excludeSignatures);
    }

    protected virtual void InitializeCatalog(string pattern = "*.cat", bool excludeSignatures = true)
    {
      List<string> catFiles = Directory.GetFiles(_folderPath, pattern).ToList();
      if (excludeSignatures)
      {
        catFiles = catFiles.Where(f => !f.EndsWith("_sig.cat")).ToList();
      }
      catFiles.Sort();
      foreach (var catFilePath in catFiles)
      {
        string datFilePath = Path.ChangeExtension(catFilePath, ".dat");
        if (File.Exists(datFilePath))
        {
          ParseCatFile(catFilePath, datFilePath);
        }
      }
    }

    private void ParseCatFile(string catFilePath, string datFilePath)
    {
      long offset = 0;
      foreach (var line in File.ReadLines(catFilePath))
      {
        if (string.IsNullOrWhiteSpace(line))
        {
          continue;
        }
        Match? match = catEntryRegex.Match(line);
        if (!match.Success || match.Groups.Count < 5 || !match.Groups.Values.All(g => g.Success))
        {
          Log.Warn($"Warning: Invalid line in catalog file: {line}");
          continue;
        }
        long fileSize = long.TryParse(match.Groups[2].Value, out long sizeValue) ? sizeValue : 0;
        long unixTime = long.TryParse(match.Groups[3].Value, out long timeValue) ? timeValue : 0;
        string filePath = match.Groups[1].Value;
        _catalog[filePath] = new CatEntry
        {
          FilePath = filePath,
          FileSize = fileSize,
          FileOffset = offset,
          FileDate = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime,
          FileHash = match.Groups[4].Value,
          DatFilePath = datFilePath,
        };
        offset += fileSize;
      }
    }

    public void ExtractFile(string filePath, string outputDirectory, bool overwrite = false, bool skipHashCheck = false)
    {
      var entry = _catalog.FirstOrDefault(e => e.Value.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)).Value;
      if (entry != null)
      {
        ExtractEntry(entry, outputDirectory, overwrite, skipHashCheck);
      }
      else
      {
        Log.Warn($"File {filePath} not found in catalog.");
      }
    }

    public bool FolderExists(string folderPath)
    {
      return _catalog.Any(e => e.Key.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase));
    }

    public List<CatEntry> GetFolderEntries(string folderPath)
    {
      return _catalog.Where(e => e.Key.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase)).Select(e => e.Value).ToList();
    }

    public void ExtractFolder(string folderPath, string outputDirectory, bool overwrite = false, bool skipHashCheck = false)
    {
      var entries = GetFolderEntries(folderPath);
      foreach (var entry in entries)
      {
        ExtractEntry(entry, outputDirectory, overwrite, skipHashCheck);
      }
    }

    public List<CatEntry> GetFilesByMask(string mask)
    {
      var regexPattern = "^" + Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
      var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

      return _catalog.Where(e => regex.IsMatch(e.Key)).Select(e => e.Value).ToList();
    }

    public void ExtractFilesByMask(string mask, string outputDirectory, bool overwrite = false, bool skipHashCheck = false)
    {
      var entries = GetFilesByMask(mask);
      foreach (var entry in entries)
      {
        ExtractEntry(entry, outputDirectory, overwrite, skipHashCheck);
      }
    }

    public static byte[] GetEntryData(CatEntry entry)
    {
      using var datFileStream = new FileStream(entry.DatFilePath, FileMode.Open, FileAccess.Read);
      datFileStream.Seek(entry.FileOffset, SeekOrigin.Begin);

      byte[] buffer = new byte[entry.FileSize];
      datFileStream.Read(buffer, 0, buffer.Length);
      // Remove BOM if present
      if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
      {
        buffer = buffer.Skip(3).ToArray();
      }
      return buffer;
    }

    public virtual void ExtractEntry(CatEntry entry, string outputDirectory, bool overwrite = false, bool skipHashCheck = false)
    {
      string outputFilePath = Path.Combine(outputDirectory, entry.FilePath);
      if (File.Exists(outputFilePath) && !overwrite)
      {
        Log.Warn($"File {entry.FilePath} already exists in output directory. Skipping extraction.");
        return;
      }

      byte[] buffer = GetEntryData(entry);

      if (!skipHashCheck)
      {
        string extractedFileHash = CalculateMD5Hash(buffer);
        if (extractedFileHash != entry.FileHash)
        {
          Log.Warn($"Warning: Hash mismatch for file {entry.FilePath}. Skipping extraction.");
          return;
        }
      }
      var directoryPath = Path.GetDirectoryName(outputFilePath);
      if (directoryPath != null)
      {
        Directory.CreateDirectory(directoryPath);
      }
      File.WriteAllBytes(outputFilePath, buffer);
      File.SetLastWriteTime(outputFilePath, entry.FileDate);
    }

    private static string CalculateMD5Hash(byte[] data)
    {
      byte[] hashBytes = MD5.HashData(data);
      return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
  }
}
