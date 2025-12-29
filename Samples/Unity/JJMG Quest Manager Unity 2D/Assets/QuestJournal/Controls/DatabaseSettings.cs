using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QuestJournal.Controls
{
    public class DatabaseSettings : MonoBehaviour
    {
        [Header("Database Settings")]
        public string FolderPath;
        public string FileName;
        public string Delimiter;

        [NonSerialized]
        public bool InvalidFolderPath;
        [NonSerialized]
        public bool InvalidFileName;
        [NonSerialized]
        public bool InvalidSaveAttempt;

        public string StoredFileName => $"\\{FileName}.json";
        public string FullFolderPath => FolderPath + StoredFileName;
        public bool ValidInput => !InvalidFolderPath && !InvalidFileName;
        public bool ValidationFailed => !ValidInput || !RequiredDataPresent();

        public void Initialize(string basePath, string dbName, string collectionName, string rootObject)
        {
            FolderPath = basePath + dbName;
            if (!string.IsNullOrEmpty(collectionName))
            {
                FileName = collectionName.TrimStart('\\', '/').Replace(".json", "");
            }
            Delimiter = rootObject;
        }

        public void ValidateFolderPath(string value)
        {
            InvalidSaveAttempt = false;
            InvalidFolderPath = string.IsNullOrEmpty(value) || !Directory.Exists(value);
            FolderPath = value;
        }

        public void ValidateFileName(string value)
        {
            InvalidSaveAttempt = false;
            string invalidFileNameChars = new string(Path.GetInvalidFileNameChars()) + '.';
            Regex invalidFileNameRegex = new Regex("[" + Regex.Escape(invalidFileNameChars) + "]");
            InvalidFileName = string.IsNullOrEmpty(value) || invalidFileNameRegex.IsMatch(value);
            FileName = value;
        }

        public bool RequiredDataPresent()
        {
            if (!string.IsNullOrEmpty(FolderPath) && !string.IsNullOrEmpty(FileName))
                return true;
            InvalidSaveAttempt = true;
            return false;
        }

        public void SaveSettings(Action<string, string, string, string> onSave)
        {
            if (ValidationFailed)
                return;
            int index = FolderPath.LastIndexOf('\\');
            string dbName = FolderPath.Substring(index);
            string basePath = FolderPath.Remove(index);
            string collectionName = StoredFileName;
            string rootObject = Delimiter;
            onSave?.Invoke(basePath, dbName, collectionName, rootObject);
        }
    }
}
