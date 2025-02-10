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
    public required string FileHash { get; set; }
    public required string DatFilePath { get; set; }
  }

  public class ContentExtractor
  {
    private readonly string _folderPath;
    private readonly Dictionary<string, CatEntry> _catalog;

    public ContentExtractor(string folderPath, string pattern = "*.cat", bool excludeSignatures = true)
    {
      _folderPath = folderPath;
      _catalog = new Dictionary<string, CatEntry>();
      InitializeCatalog(pattern, excludeSignatures);
    }

    private void InitializeCatalog(string pattern = "*.cat", bool excludeSignatures = true)
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
        var parts = line.Split(' ');
        if (parts.Length >= 4)
        {
          if (parts.Length > 4)
          {
            Log.Warn($"Warning: Unexpected number of parts in line: {line}");
            string[] newParts =
            {
              string.Join(" ", parts[0..(parts.Length - 3)]),
              parts[parts.Length - 3],
              parts[parts.Length - 2],
              parts[parts.Length - 1],
            };
            parts = newParts;
          }
          long fileSize = long.Parse(parts[1]);
          string filePath = parts[0];
          _catalog[filePath] = new CatEntry
          {
            FilePath = filePath,
            FileSize = fileSize,
            FileOffset = offset,
            FileHash = parts[3],
            DatFilePath = datFilePath,
          };
          offset += fileSize;
        }
      }
    }

    public void ExtractFile(string filePath, string outputDirectory)
    {
      var entry = _catalog.FirstOrDefault(e => e.Value.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)).Value;
      if (entry != null)
      {
        ExtractEntry(entry, outputDirectory);
      }
      else
      {
        Log.Warn($"File {filePath} not found in catalog.");
      }
    }

    public List<CatEntry> GetFolderEntries(string folderPath)
    {
      return _catalog.Where(e => e.Key.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase)).Select(e => e.Value).ToList();
    }

    public void ExtractFolder(string folderPath, string outputDirectory)
    {
      var entries = GetFolderEntries(folderPath);
      foreach (var entry in entries)
      {
        ExtractEntry(entry, outputDirectory);
      }
    }

    public List<CatEntry> GetFilesByMask(string mask)
    {
      var regexPattern = "^" + Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
      var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

      return _catalog.Where(e => regex.IsMatch(e.Key)).Select(e => e.Value).ToList();
    }

    public void ExtractFilesByMask(string mask, string outputDirectory)
    {
      var entries = GetFilesByMask(mask);
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
        var directoryPath = Path.GetDirectoryName(outputFilePath);
        if (directoryPath != null)
        {
          Directory.CreateDirectory(directoryPath);
        }
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
