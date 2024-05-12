﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QuestManager.Configuration;
using QuestManager.Utility;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace QuestManager.Managers
{
    /// <summary>
    /// This class is responsible for connecting to a JSON file that holds quest data. 
    /// </summary>
    public class QuestDbConnection : IQuestDbConnection
    {
        private readonly string fullPath;
        private List<Quest> _allQuests;
        EventWaitHandle _waitHandle;
        DbConnectionOptions _options;

        public QuestDbConnection(IOptions<DbConnectionOptions> dbConnectionOptions)
        {
            _options = dbConnectionOptions.Value;

            //if we dont have a json file location we use app settings
            if (string.IsNullOrEmpty(_options.BasePath))
                _options.BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

            fullPath = _options.BasePath + _options.DbName + _options.CollectionName;
             _waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "questManagerWaitFileAccess");
        }

        public ResponseStatus SaveQuestDbChanges()
        {
            var questsToSave = JsonConvert.SerializeObject(GetAllQuests(), new JsonSerializerSettings
            {
                ContractResolver = new JsonReadOnlyPropertiesResolver()
            });

            SaveDbBackup();

            _waitHandle.WaitOne();
            File.WriteAllText(fullPath, questsToSave);
            _waitHandle.Set();

            return new ResponseStatus(true, "Successfully wrote quest data to data store");
        }

        public ResponseStatus SaveDbBackup()
        {
            _waitHandle.WaitOne();
            if (!File.Exists(fullPath))
            {
                _waitHandle.Set();
                //fine for a json db but not expected in hosted db
                return new ResponseStatus(true, "No data to backup");
            }

            File.Copy(fullPath, fullPath.Replace(".json","Old.json"), true);
            _waitHandle.Set();

            return new ResponseStatus(true, "Successfully backed up quest data");
        }

        public List<Quest> GetAllQuests()
        {
            if (_allQuests == null)
            {
                _waitHandle.WaitOne();
                if (!File.Exists(fullPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    var createdDb = File.Create(fullPath);
                    createdDb.Close();
                }

                var readFile = File.ReadAllText(fullPath);
                var quests = JsonConvert.DeserializeObject<List<Quest>>(readFile);
                _waitHandle.Set();
                _allQuests = quests != null ? quests : new List<Quest>();
            }

            return _allQuests;
        }

        public List<Quest> OverwriteAllQuests(List<Quest> quests)
        {
            if (quests == null)
                throw new Exception("Invalid quest data passed, unable to overwrite");
            _allQuests = quests;
            return quests;
        }

        public Quest GetQuest(string id)
        {
            var quests = GetAllQuests();
            return quests.First(q => q.Id == id);
        }

        public Quest UpdateQuest(Quest quest)
        {
            if (quest == null)
                throw new Exception("Failure to update quest data as no data is present");
            if (string.IsNullOrEmpty(quest.Id))
                throw new Exception("Failure to update quest data as no quest Id is present to match");

            var quests = GetAllQuests();

            if (quests.Any(q => q.Id == quest.Id))
                quests.First(q => q.Id == quest.Id).OverwriteData(quest);
            else
                throw new Exception("Failure to update quest data quest Id does not match any in the database");

            return quest;
        }

        public List<Quest> UpdateQuests(List<Quest> quests)
        {
            List<Quest> responses = new List<Quest>();
            foreach (var quest in quests)
                responses.Add(UpdateQuest(quest));

            return responses;
        }

        public Quest CreateQuest(Quest quest)
        {
            quest.Id = DbIdHelper.GenerateQuestId(GetAllQuests().Count, Constants.Prefix.QuestPrefix);

            if (string.IsNullOrEmpty(quest.Id))
                throw new Exception("Quest was created without an Id, something has went wrong!");

            GetAllQuests().Add(quest);
            return quest;
        }

        public List<Quest> CreateQuests(List<Quest> quests)
        {
            if (quests == null || quests.Count == 0)
                throw new Exception("A call was made to create quests but not data was provided");
            return quests.ConvertAll(q => CreateQuest(q));
        }

        public ResponseStatus DeleteQuest(string QuestID)
        {
            if (GetAllQuests().RemoveAll(q => q.Id == QuestID) > 0)
                return new ResponseStatus(true, "Deleted successfully");

            return new ResponseStatus(false, "No quests match that Id");
        }

        public List<ResponseStatus> DeleteQuests(List<Quest> quests)
        {
            if (quests == null || quests.Count == 0)
                return new List<ResponseStatus>() { new ResponseStatus(false, "Failure to delete any quest data, none was found") };

            List<ResponseStatus> responses = new List<ResponseStatus>();
            foreach (var quest in quests)
            {
                try
                {
                    responses.Add(DeleteQuest(quest.Id));
                }
                catch (Exception ex)
                {
                    responses.Add(new ResponseStatus(false, $"An exception occurred while deleting quest {quest.Id} error: {ex.Message}"));
                }
            }

            return responses;
        }

        public List<ResponseStatus> DeleteQuests(string[] questIds)
        {
            if (questIds == null || questIds.Length == 0)
                return new List<ResponseStatus>() { new ResponseStatus(false, "Failure to delete any quest data, none was found") };

            List<ResponseStatus> responses = new List<ResponseStatus>();
            for (int i = 0; i < questIds.Length; i++)
            {
                try
                {
                    responses.Add(DeleteQuest(questIds[i]));
                }
                catch (Exception ex)
                {
                    responses.Add(new ResponseStatus(false, $"An exception occurred while deleting quest {questIds[i]} error: {ex.Message}"));
                }
            }

            return responses;
        }
    }
}
