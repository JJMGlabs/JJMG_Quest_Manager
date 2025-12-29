using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuestJournal.UI
{
    public class SettingsPageController
    {
        private const string SettingsFileName = "unity_db_settings.json";

        [Serializable]
        public class DbOptions
        {
            public string BasePath = "";
            public string DbName = "";
            public string CollectionName = "";
            public string RootObject = "";
        }

        private void ShowValidationPopup(System.Collections.Generic.List<string> errors)
        {
            if (root == null) return;

            // Find top-level visual root so overlay covers entire UI window
            VisualElement host = root;
            while (host.parent is VisualElement parent)
            {
                host = parent;
            }

            ValidationPopup.Show(host, errors);
        }

        [Serializable]
        public class AppSettings
        {
            public DbOptions QuestDb = new DbOptions();
            public DbOptions QuestlineDb = new DbOptions();
        }

        private AppSettings settings;
        private Button saveButton;
        private Label toastLabel;
        private VisualElement root;

        public SettingsPageController(VisualElement rootElement)
        {
            root = rootElement;
            Initialize();
        }

        private void Initialize()
        {
            // Query embedded QuestDb and QuestlineDb fields directly
            var questFolderField = root.Q<TextField>("QuestDb_FolderPathField");
            var questFileField = root.Q<TextField>("QuestDb_FileNameField");
            var questDelimiterField = root.Q<TextField>("QuestDb_DelimiterField");

            var questlineFolderField = root.Q<TextField>("QuestlineDb_FolderPathField");
            var questlineFileField = root.Q<TextField>("QuestlineDb_FileNameField");
            var questlineDelimiterField = root.Q<TextField>("QuestlineDb_DelimiterField");

            saveButton = root.Q<Button>("SaveAllSettingsButton");
            toastLabel = root.Q<Label>("SettingsToastLabel");

            LoadSettings();

            // Initialize UI fields from settings
            if (questFolderField != null) questFolderField.value = settings.QuestDb.BasePath + settings.QuestDb.DbName;
            if (questFileField != null) questFileField.value = settings.QuestDb.CollectionName?.TrimStart('\\', '/')?.Replace(".json", "");
            if (questDelimiterField != null) questDelimiterField.value = settings.QuestDb.RootObject;

            if (questlineFolderField != null) questlineFolderField.value = settings.QuestlineDb.BasePath + settings.QuestlineDb.DbName;
            if (questlineFileField != null) questlineFileField.value = settings.QuestlineDb.CollectionName?.TrimStart('\\', '/')?.Replace(".json", "");
            if (questlineDelimiterField != null) questlineDelimiterField.value = settings.QuestlineDb.RootObject;

            if (saveButton != null)
            {
                saveButton.clicked += OnSaveClicked;
            }
        }

        void LoadSettings()
        {
            string path = Path.Combine(Application.persistentDataPath, SettingsFileName);
            Debug.Log(path);
            if (File.Exists(path))
            {
                settings = JsonUtility.FromJson<AppSettings>(File.ReadAllText(path));
            }
            else
            {
                settings = new AppSettings();
            }
        }

        void PersistSettings()
        {
            string path = Path.Combine(Application.persistentDataPath, SettingsFileName);
            File.WriteAllText(path, JsonUtility.ToJson(settings, true));
        }

        private bool SectionValuesChanged(DbOptions opts, string folderValue, string fileValue, string delimValue)
        {
            string existingFolder = (opts?.BasePath ?? string.Empty) + (opts?.DbName ?? string.Empty);
            string existingFile = opts?.CollectionName?.TrimStart('\\')?.Replace(".json", "") ?? string.Empty;
            string existingDelim = opts?.RootObject ?? string.Empty;

            return !string.Equals(folderValue ?? string.Empty, existingFolder, StringComparison.Ordinal) ||
                   !string.Equals(fileValue ?? string.Empty, existingFile, StringComparison.Ordinal) ||
                   !string.Equals(delimValue ?? string.Empty, existingDelim, StringComparison.Ordinal);
        }

        private System.Collections.Generic.List<string> ValidateAndApplySection(string sectionName, TextField folder, TextField file, TextField delim, DbOptions target)
        {
            var errors = new System.Collections.Generic.List<string>();
            if (folder == null || file == null)
                return errors;

            bool invalidFolder = string.IsNullOrEmpty(folder.value) || !Directory.Exists(folder.value);
            string invalidFileNameChars = new string(Path.GetInvalidFileNameChars()) + '.';
            var invalidFileNameRegex = new System.Text.RegularExpressions.Regex("[" + System.Text.RegularExpressions.Regex.Escape(invalidFileNameChars) + "]");
            bool invalidFile = string.IsNullOrEmpty(file.value) || invalidFileNameRegex.IsMatch(file.value);

            if (invalidFolder)
                errors.Add($"Failed to save {sectionName} settings: FolderPath: invalid or does not exist");
            if (invalidFile)
                errors.Add($"Failed to save {sectionName} settings: FileName: invalid file name");

            if (errors.Count == 0)
            {
                var index = folder.value.LastIndexOf('\\');
                target.DbName = index >= 0 ? folder.value.Substring(index) : string.Empty;
                target.BasePath = index >= 0 ? folder.value.Remove(index) : folder.value;
                target.CollectionName = $"\\{file.value}.json";
                target.RootObject = delim != null ? delim.value : string.Empty;
            }

            return errors;
        }

        private void UpdateSettingsManagerForSection(DbOptions opts)
        {
            if (opts == null) return;
            if (!string.IsNullOrEmpty(opts.BasePath) && !string.IsNullOrEmpty(opts.CollectionName))
            {
                var file = opts.CollectionName.TrimStart('\\');
                SettingsManager.AddOrUpdate(new SettingsManager.SettingEntry(opts.BasePath + opts.DbName, file, opts.RootObject));
            }
        }

        void OnSaveClicked()
        {
            // Collect UI fields
            var qFolder = root.Q<TextField>("QuestDb_FolderPathField");
            var qFile = root.Q<TextField>("QuestDb_FileNameField");
            var qDelim = root.Q<TextField>("QuestDb_DelimiterField");

            var qlFolder = root.Q<TextField>("QuestlineDb_FolderPathField");
            var qlFile = root.Q<TextField>("QuestlineDb_FileNameField");
            var qlDelim = root.Q<TextField>("QuestlineDb_DelimiterField");

            bool questChanged = SectionValuesChanged(settings.QuestDb, qFolder?.value, qFile?.value, qDelim?.value);
            bool qlChanged = SectionValuesChanged(settings.QuestlineDb, qlFolder?.value, qlFile?.value, qlDelim?.value);

            if (!questChanged && !qlChanged)
            {
                ShowNoChangesToast();
                return;
            }

            var errors = new System.Collections.Generic.List<string>();
            if (questChanged)
            {
                errors.AddRange(ValidateAndApplySection("QuestDb", qFolder, qFile, qDelim, settings.QuestDb));
            }

            if (qlChanged)
            {
                errors.AddRange(ValidateAndApplySection("QuestlineDb", qlFolder, qlFile, qlDelim, settings.QuestlineDb));
            }

            if (errors.Count > 0)
            {
                ShowValidationPopup(errors);
                return;
            }

            PersistSettings();
            ShowSavedToast();

            try
            {
                if (questChanged) UpdateSettingsManagerForSection(settings.QuestDb);
                if (qlChanged) UpdateSettingsManagerForSection(settings.QuestlineDb);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsPageController: failed to persist to SettingsManager: {ex}");
            }
        }

        private void ShowNoChangesToast()
        {
            if (toastLabel == null) return;
            try { toastLabel.style.color = new UnityEngine.UIElements.StyleColor(Color.gray); } catch { toastLabel.style.color = Color.gray; }
            toastLabel.text = "No changes to save.";
        }

        private void ShowSavedToast()
        {
            if (toastLabel == null) return;
            try { toastLabel.style.color = new UnityEngine.UIElements.StyleColor(Color.green); } catch { toastLabel.style.color = Color.green; }
            toastLabel.text = "Settings saved successfully.";
        }
    }
}
