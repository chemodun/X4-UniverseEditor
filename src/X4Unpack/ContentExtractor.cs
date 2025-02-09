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
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public long FileOffset { get; set; }
    public string FileHash { get; set; }
    public string DatFilePath { get; set; }
  }

  public class ContentExtractor
  {
    private readonly string _folderPath;
    private readonly List<CatEntry> _catalog;

    public ContentExtractor(string folderPath)
    {
      _folderPath = folderPath;
      _catalog = new List<CatEntry>();
      InitializeCatalog();
    }

    private void InitializeCatalog()
    {
      var catFiles = Directory.GetFiles(_folderPath, "*.cat");

      foreach (var catFilePath in catFiles)
      {
        string datFilePath = Path.ChangeExtension(catFilePath, ".dat");
        if (File.Exists(datFilePath))
        {
          var catEntries = ParseCatFile(catFilePath, datFilePath);
          _catalog.AddRange(catEntries);
        }
      }
    }

    private List<CatEntry> ParseCatFile(string catFilePath, string datFilePath)
    {
      var catEntries = new List<CatEntry>();
      long offset = 0;
      foreach (var line in File.ReadLines(catFilePath))
      {
        var parts = line.Split(' ');
        if (parts.Length == 4)
        {
          catEntries.Add(
            new CatEntry
            {
              FilePath = parts[0],
              FileSize = long.Parse(parts[1]),
              FileOffset = offset,
              FileHash = parts[3],
              DatFilePath = datFilePath,
            }
          );
          offset += long.Parse(parts[1]);
        }
      }

      return catEntries;
    }

    public void ExtractFile(string filePath, string outputDirectory)
    {
      var entry = _catalog.FirstOrDefault(e => e.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
      if (entry != null)
      {
        ExtractEntry(entry, outputDirectory);
      }
      else
      {
        Log.Warn($"File {filePath} not found in catalog.");
      }
    }

    public void ExtractFolder(string folderPath, string outputDirectory)
    {
      var entries = _catalog.Where(e => e.FilePath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase)).ToList();
      foreach (var entry in entries)
      {
        ExtractEntry(entry, outputDirectory);
      }
    }

    public void ExtractFilesByMask(string mask, string outputDirectory)
    {
      var regexPattern = "^" + Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
      var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

      var entries = _catalog.Where(e => regex.IsMatch(e.FilePath)).ToList();
      foreach (var entry in entries)
      {
        ExtractEntry(entry, outputDirectory);
      }
    }

    private void ExtractEntry(CatEntry entry, string outputDirectory)
    {
      using (var datFileStream = new FileStream(entry.DatFilePath, FileMode.Open, FileAccess.Read))
      {
        datFileStream.Seek(entry.FileOffset, SeekOrigin.Begin);

        byte[] buffer = new byte[entry.FileSize];
        datFileStream.Read(buffer, 0, buffer.Length);

        string outputFilePath = Path.Combine(outputDirectory, entry.FilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
        File.WriteAllBytes(outputFilePath, buffer);

        string extractedFileHash = CalculateMD5Hash(buffer);
        if (extractedFileHash != entry.FileHash)
        {
          Log.Warn($"Warning: Hash mismatch for file {entry.FilePath}");
        }
      }
    }

    private string CalculateMD5Hash(byte[] data)
    {
      using (var md5 = MD5.Create())
      {
        byte[] hashBytes = md5.ComputeHash(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
      }
    }
  }
}
