using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Simple settings manager that persists an array of database setting entries
/// to a JSON file under the Unity project (relative to Assets/ via Application.dataPath).
/// Each entry contains FolderPath, FileName and Delimiter.
/// </summary>
public static class SettingsManager
{
    // Path relative to the Assets folder. Example: "Configuration/JournalSettings.json"
    static string _unityProjectAbsolutePath = "Configuration/JournalSettings.json";

    [Serializable]
    public class SettingEntry
    {
        public string FolderPath;
        public string FileName;
        public string Delimiter;

        public SettingEntry() { }

        public SettingEntry(string folderPath, string fileName, string delimiter)
        {
            FolderPath = folderPath;
            FileName = fileName;
            Delimiter = delimiter;
        }
    }

    [Serializable]
    private class SettingsFile
    {
        public List<SettingEntry> Entries = new List<SettingEntry>();
    }

    private static string FullPath
    {
        get
        {
            // Application.dataPath points to the Assets folder
            return Path.Combine(Application.dataPath, _unityProjectAbsolutePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }

    /// <summary>
    /// Load all settings from the JSON file. Returns empty list if none found or on error.
    /// </summary>
    public static List<SettingEntry> Load()
    {
        try
        {
            var path = FullPath;
            if (!File.Exists(path))
            {
                Debug.Log($"SettingsManager.Load: settings file not found at '{path}'");
                return new List<SettingEntry>();
            }

            var json = File.ReadAllText(path);
            var container = JsonUtility.FromJson<SettingsFile>(json);
            if (container == null || container.Entries == null)
            {
                Debug.LogWarning($"SettingsManager.Load: settings file parsed but contained no entries ({path})");
                return new List<SettingEntry>();
            }

            Debug.Log($"SettingsManager.Load: loaded {container.Entries.Count} entries from '{path}'");
            return container.Entries;
        }
        catch (Exception ex)
        {
            Debug.LogError($"SettingsManager.Load: failed to read settings file ({FullPath}): {ex}");
            return new List<SettingEntry>();
        }
    }

    /// <summary>
    /// Persist the provided entries to disk (overwrites file).
    /// Ensures directory exists.
    /// </summary>
    public static void Save(List<SettingEntry> entries)
    {
        try
        {
            var path = FullPath;
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Debug.Log($"SettingsManager.Save: creating directory '{dir}'");
                Directory.CreateDirectory(dir);
            }

            var container = new SettingsFile { Entries = entries ?? new List<SettingEntry>() };
            var json = JsonUtility.ToJson(container, true);
            File.WriteAllText(path, json);
            Debug.Log($"SettingsManager.Save: wrote {container.Entries.Count} entries to '{path}'");
        }
        catch (Exception ex)
        {
            Debug.LogError($"SettingsManager.Save: failed to write settings file ({FullPath}): {ex}");
        }
    }

    /// <summary>
    /// Add or update an entry (matching by FolderPath + FileName). Returns true if added/updated.
    /// </summary>
    public static bool AddOrUpdate(SettingEntry entry)
    {
        if (entry == null) return false;
        var list = Load();
        var existing = list.Find(e => string.Equals(e.FolderPath, entry.FolderPath, StringComparison.OrdinalIgnoreCase)
                                     && string.Equals(e.FileName, entry.FileName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Delimiter = entry.Delimiter;
            Debug.Log($"SettingsManager.AddOrUpdate: updated entry Folder='{entry.FolderPath}' File='{entry.FileName}' Delimiter='{entry.Delimiter}'");
        }
        else
        {
            list.Add(entry);
            Debug.Log($"SettingsManager.AddOrUpdate: added entry Folder='{entry.FolderPath}' File='{entry.FileName}' Delimiter='{entry.Delimiter}'");
        }

        Save(list);
        return true;
    }

    /// <summary>
    /// Remove an entry by folderPath and fileName. Returns true if removed.
    /// </summary>
    public static bool Remove(string folderPath, string fileName)
    {
        var list = Load();
        var removed = list.RemoveAll(e => string.Equals(e.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)
                                       && string.Equals(e.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            Save(list);
            Debug.Log($"SettingsManager.Remove: removed {removed} entries matching Folder='{folderPath}' File='{fileName}'");
            return true;
        }
        Debug.Log($"SettingsManager.Remove: no entries matched Folder='{folderPath}' File='{fileName}'");
        return false;
    }

    /// <summary>
    /// Get a single entry or null.
    /// </summary>
    public static SettingEntry Get(string folderPath, string fileName)
    {
        var list = Load();
        var found = list.Find(e => string.Equals(e.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(e.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        if (found != null)
            Debug.Log($"SettingsManager.Get: found entry Folder='{folderPath}' File='{fileName}'");
        else
            Debug.Log($"SettingsManager.Get: no entry found for Folder='{folderPath}' File='{fileName}'");
        return found;
    }
}
