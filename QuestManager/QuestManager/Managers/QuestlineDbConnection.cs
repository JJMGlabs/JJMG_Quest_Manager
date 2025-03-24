using Microsoft.Extensions.Options;
using QuestManager.Configuration;
using QuestManager.Managers.Interfaces;
using QuestManager.Utility;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.Utility;
using System;
using System.Collections.Generic;
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

            var mappedQuests = allQuests.ToDictionary(q => q.Id);

            List<List<Quest>> orderedQuestlinePaths = new List<List<Quest>>();

            // Start at root quests
            var rootQuests = allQuests
                .Where(q => !allQuests.Any(otherQuest => otherQuest.QuestOutcomes
                    .Any(o => o.GetQuestIdFromOutcome() == q.Id)))  // Root quests are not outcomes of other quests
                .ToList();

            foreach (var rootQuest in rootQuests)
                BuildQuestlinePath(rootQuest, new List<Quest>(), mappedQuests, orderedQuestlinePaths);

            return orderedQuestlinePaths;
        }

        private static void BuildQuestlinePath(Quest currentQuest, List<Quest> currentPath, Dictionary<string, Quest> mappedQuests, List<List<Quest>> orderedQuestlinePaths)
        {
            HashSet<string> visitedQuests = new HashSet<string>();

            if (visitedQuests.Contains(currentQuest.Id))
                return;

            currentPath.Add(currentQuest);
            visitedQuests.Add(currentQuest.Id);

            var nextQuestIds = currentQuest.QuestOutcomes
                .Select(o => o.GetQuestIdFromOutcome())
                .Where(nextQuestId => !string.IsNullOrEmpty(nextQuestId))
                .ToList();

            if (!nextQuestIds.Any())
            {
                orderedQuestlinePaths.Add(new List<Quest>(currentPath));
            }
            else
            {
                foreach (var nextQuestId in nextQuestIds)
                    if (mappedQuests.ContainsKey(nextQuestId))
                        BuildQuestlinePath(mappedQuests[nextQuestId], new List<Quest>(currentPath), mappedQuests, orderedQuestlinePaths);
            }
        }
    }
}
