using Microsoft.Extensions.Options;
using QuestManager.Configuration;
using QuestManager.Managers.Interfaces;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.Utility;
using System;
using System.Collections.Generic;

namespace QuestManager.Managers
{
    public class QuestlineDbConnection : IQuestlineDbConnection
    {
        private readonly string fullPath;
        private List<QuestlineMetadata> _questlines;
        QuestLineDbConnectionOptions _options;
        FileBasedDbConnectionUtility _db;

        public QuestlineDbConnection(IOptions<QuestLineDbConnectionOptions> dbConnectionOptions)
        {
            _options = dbConnectionOptions.Value;

            //if we dont have a json file location we use app settings
            if (string.IsNullOrEmpty(_options.BasePath))
                _options.BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

            fullPath = _options.BasePath + _options.DbName + _options.CollectionName;
            _db = new FileBasedDbConnectionUtility(Constants.UtilityValues.WaitHandleForQuestLineDb);
        }

        public ResponseStatus SaveDbChanges()
        {
            SaveDbBackup();

            _db.WriteListDataToFile(fullPath, GetQuestlines(), _options.RootObject);

            return new ResponseStatus(true, "Successfully wrote quest data to data store");
        }

        public ResponseStatus SaveDbBackup()
        {
            _db.BackupDB(fullPath, _options.RootObject);

            return new ResponseStatus(true, "Successfully backed up quest data");
        }

        public List<QuestlineMetadata> GetQuestlines()
        {
            if (_questlines == null)
            {
                List<QuestlineMetadata> questLines = new List<QuestlineMetadata>();
                questLines = _db.GetListOfDataFromFile<QuestlineMetadata>(fullPath, _options.RootObject);
                _questlines = questLines != null ? questLines : new List<QuestlineMetadata>();
            }

            return _questlines;
        }

        public List<QuestlineMetadata> OverwriteAllQuestLines(List<QuestlineMetadata> questlines)
        {
            if (questlines == null)
                throw new Exception("Invalid questline data, unable to overwrite");
            _questlines = questlines;
            return questlines;
        }

        public ResponseStatus WriteQuestLines(List<QuestlineMetadata> questlines)
        {
            OverwriteAllQuestLines(questlines);
            SaveDbChanges();
            return new ResponseStatus(true, "Successfully Wrote Questline data.");
        }
    }
}
