using Microsoft.Extensions.Options;
using QuestManager.Configuration;
using QuestManager.Managers.Interfaces;
using QuestManager.Utility;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.QuestSubObjects;
using QuestManagerSharedResources.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuestManager.Managers
{
    public class QuestlineDbConnection : IQuestlineDbConnection, IQuestlineQuestRelationshipConnection
    {
        private readonly string fullPath;
        private readonly IQuestDbConnection _questDbConnection;
        private List<QuestlineMetadata> _questlines;
        QuestLineDbConnectionOptions _options;
        FileBasedDbConnectionUtility _db;

        public QuestlineDbConnection(IOptions<QuestLineDbConnectionOptions> dbConnectionOptions, IQuestDbConnection questDbConnection)
        {
            _options = dbConnectionOptions.Value;

            //if we dont have a json file location we use app settings
            if (string.IsNullOrEmpty(_options.BasePath))
                _options.BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

            fullPath = _options.BasePath + _options.DbName + _options.CollectionName;
            _db = new FileBasedDbConnectionUtility(Constants.UtilityValues.WaitHandleForQuestLineDb);
            _questDbConnection = questDbConnection;
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

        public ResponseStatus CreateQuestLine(QuestlineMetadata questline)
        {
            GetQuestlines();

            questline.Id = DbIdHelper.GenerateQuestId(_questlines.Count, Constants.Prefix.QuestlinePrefix);

            _questlines.Add(questline);

            OverwriteAllQuestLines(_questlines);

            return new ResponseStatus(true, "Successfully created and added questline.");
        }

        public ResponseStatus WriteQuestLines(List<QuestlineMetadata> questlines)
        {
            //We can overwrite questlines with null or empty data....but if we have data it absolutely must have an ID
            if (questlines != null)
                if (questlines.Any(q => string.IsNullOrEmpty(q.Id)))
                    throw new ArgumentException("Invalid questline data, one or more questlines have a null or empty ID.");

            OverwriteAllQuestLines(questlines);
            SaveDbChanges();
            return new ResponseStatus(true, "Successfully Wrote Questline data.");
        }
        public List<List<Quest>> GetQuestline(string questlineId)
        {
            var allQuests = _questDbConnection.GetAllQuests()
                .Where(q => q.QuestOutcomes != null && q.QuestOutcomes.Any(o => o.isQuestlineOutcome() && o.GetQuestlineId() == questlineId))
                .ToList();

            if(allQuests == null || !allQuests.Any())
                return new List<List<Quest>>();

            List<List<Quest>> orderedQuestlinePaths = new List<List<Quest>>();

            // 1. split allQuests into groups based on (A) no outcomes point to them, (B) All alternates to form paths
            List<Quest> originQuests = allQuests.Where(q => !allQuests.Any(otherQuest => otherQuest.QuestOutcomes
                    .Any(o => o.GetQuestIdFromOutcome() == q.Id)))  // Root quests are not outcomes of other quests
                .ToList();

            var pathQuests = allQuests.Where(q => !originQuests.Contains(q)).ToDictionary(q => q.Id);
            // 2. We can freely traverse all nodeas from each group A node to find all paths
            foreach (var originQuest in originQuests)
                BuildQuestlinePath(questlineId, new List<Quest>() { originQuest }, pathQuests, orderedQuestlinePaths);

            return orderedQuestlinePaths;
        }

        private List<List<Quest>> BuildQuestlinePath(string questlineId, List<Quest> currentPath, Dictionary<string, Quest> availableQuests, List<List<Quest>> orderedQuestlinePaths)
        {
            if (availableQuests == null || !availableQuests.Any())
            {
                orderedQuestlinePaths.Add(currentPath);
                return orderedQuestlinePaths;
            }

            var outcomes = currentPath.Last().QuestOutcomes.Where(o => o.isQuestlineOutcome() && o.GetQuestlineId() == questlineId);

           foreach (var outcome in outcomes)
            {
                var questID = outcome.GetQuestIdFromOutcome();
                if (outcomes.Count() == 1)
                {
                    if (string.IsNullOrEmpty(questID))
                    {
                        orderedQuestlinePaths.Add(currentPath);
                        break;
                    }
                    currentPath.Add(availableQuests[questID]);
                    BuildQuestlinePath(questlineId, currentPath, availableQuests, orderedQuestlinePaths);
                }
                else
                {
                    //in the first path we can simply return no need to start a new recursion
                    if (outcome == outcomes.First())
                    {
                        if (string.IsNullOrEmpty(questID) || !availableQuests.ContainsKey(questID))
                        {
                            orderedQuestlinePaths.Add(currentPath);
                            break;
                        }
                        orderedQuestlinePaths.Add(new List<Quest>(currentPath) { availableQuests[questID] });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(questID) || !availableQuests.ContainsKey(questID))
                        {
                            orderedQuestlinePaths.Add(currentPath);
                            break;
                        }

                        BuildQuestlinePath(questlineId, new List<Quest>(currentPath) { availableQuests[questID] }, availableQuests, orderedQuestlinePaths);
                    }
                }
            }
            return orderedQuestlinePaths;
        }
    }
}
