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
        private readonly IQuestDbConnection _questDbConnection;
        private readonly IOptionsMonitor<QuestLineDbConnectionOptions> _optionsMonitor;
        private List<QuestlineMetadata> _questlines;
        FileBasedDbConnectionUtility _db;

        private QuestLineDbConnectionOptions CurrentOptions => _optionsMonitor.CurrentValue;

        private string CurrentFullPath
        {
            get
            {
                var options = CurrentOptions;
                if (string.IsNullOrEmpty(options.BasePath))
                    options.BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

                return options.BasePath + options.DbName + options.CollectionName;
            }
        }

        public QuestlineDbConnection(IOptionsMonitor<QuestLineDbConnectionOptions> dbConnectionOptions, IQuestDbConnection questDbConnection)
        {
            _optionsMonitor = dbConnectionOptions;
            _optionsMonitor.OnChange(_ => ClearCachedQuestlines());

            _db = new FileBasedDbConnectionUtility(Constants.UtilityValues.WaitHandleForQuestLineDb);
            _questDbConnection = questDbConnection;
        }

        private QuestlineDbConnection(IOptions<QuestLineDbConnectionOptions> dbConnectionOptions, IQuestDbConnection questDbConnection)
            : this(new StaticOptionsMonitor<QuestLineDbConnectionOptions>(dbConnectionOptions.Value), questDbConnection)
        {
        }

        private void ClearCachedQuestlines()
        {
            _questlines = null;
        }

        public ResponseStatus SaveDbChanges()
        {
            SaveDbBackup();

            _db.WriteListDataToFile(CurrentFullPath, GetQuestlines(), CurrentOptions.RootObject);

            return new ResponseStatus(true, "Successfully wrote quest data to data store");
        }

        public ResponseStatus SaveDbBackup()
        {
            _db.BackupDB(CurrentFullPath, CurrentOptions.RootObject);

            return new ResponseStatus(true, "Successfully backed up quest data");
        }

        public List<QuestlineMetadata> GetQuestlines()
        {
            if (_questlines == null)
            {
                List<QuestlineMetadata> questLines = new List<QuestlineMetadata>();
                questLines = _db.GetListOfDataFromFile<QuestlineMetadata>(CurrentFullPath, CurrentOptions.RootObject);
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

        private sealed class StaticOptionsMonitor<TOptions> : IOptionsMonitor<TOptions> where TOptions : class, new()
        {
            private readonly TOptions _value;

            public StaticOptionsMonitor(TOptions value)
            {
                _value = value;
            }

            public TOptions CurrentValue => _value;

            public TOptions Get(string name) => _value;

            public IDisposable OnChange(Action<TOptions, string> listener) => NullDisposable.Instance;

            private sealed class NullDisposable : IDisposable
            {
                public static readonly NullDisposable Instance = new NullDisposable();
                public void Dispose() { }
            }
        }
    }
}
