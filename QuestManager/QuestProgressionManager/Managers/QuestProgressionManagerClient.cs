using Newtonsoft.Json;
using QuestManager.Utility;
using QuestManagerApi.Controllers;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.QuestSubObjects;
using QuestProgressionManager.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace QuestProgressionManager.Managers
{
    public class QuestProgressionManagerClient : IQuestProgressionManagerClient
    {
        private string basePath;
        private List<Quest> _playerQuestData;
        EventWaitHandle _waitHandle;

        private string _sourcDbConnectionPath = null;
        private string _sourceDbCollectionName = null;
        private string _questProgressionFilePath;

        public bool AutoSave { get; set; }

        public QuestProgressionManagerClient()
        {
            Initialise();
        }

        public QuestProgressionManagerClient(string questProgressionFilePath, string sourceDbConnectionPath, string sourceDbCollectionName) : base()
        {
            _sourcDbConnectionPath = sourceDbConnectionPath;
            _sourceDbCollectionName = sourceDbCollectionName;
            _questProgressionFilePath = questProgressionFilePath;
            Initialise();
        }

        private void Initialise()
        {
            if (string.IsNullOrEmpty(_questProgressionFilePath))
                _questProgressionFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            basePath = $"{_questProgressionFilePath}/questProgression.json";
            _waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "questManagerWaitFileAccess");
            Start();
            ReadAllAvailableQuestsFromDb();
        }

        #region Application loop methods        
        public void Start()
        {
            ReadPlayerQuestDataFromSaveFile();
            var dbQuests = ReadAllAvailableQuestsFromDb();
            DbQuestToQuestProgressionMerge(dbQuests);
            SavePlayerQuestDataToSaveFile(_playerQuestData);
        }
        public void Update(List<SubObjectUpdate> measurementUpdates)
        {
            MeasureQuestProgression(measurementUpdates);
            UpdateQuestPrerequisites();
        }

        public List<QuestOutcome> AcceptQuestOutcomes()
        {
            var quests = GetAllCompleteQuests();
            var outputOutcomes = new List<QuestOutcome>();
            foreach (var quest in quests)
            {
                List<QuestOutcome> outcomes = quest.AcceptQuestOutcomes();
                outputOutcomes.AddRange(outcomes);
                outcomes.Where(o => o.DeliveryMetadata.ContainsKey(Constants.ReservedMeasurementKeys.OutcomeQuestKey)).ToList().ForEach(qo => UpdateInternalOutcome(qo));
            }
            return outputOutcomes;
        }

        public void UpdateQuestPrerequisite(string prerequisiteUpdate, string id = "")
        {
            if (!string.IsNullOrEmpty(id))
            {
                var quest = ReadPlayerQuestDataFromSaveFile().Find(q => q.Id == DbIdHelper.ReadQuestIDFromSubObjectId(id));
                var prerequisite = quest?.QuestPrerequisites.Find(r => r.Id == id);
                prerequisite?.UpdatePrerequisite(prerequisiteUpdate);
                return;
            }
            foreach (var quest in ReadPlayerQuestDataFromSaveFile())
            {
                if (quest.State == QuestState.ACTIVE)
                    quest.QuestPrerequisites.ForEach(q => q.UpdatePrerequisite(prerequisiteUpdate));
            }
        }
        #endregion

        public Quest GetQuestByID(string id)
        {
            return _playerQuestData.Find(q => q.Id == id);
        }

        public List<Quest> GetAllQuests() => _playerQuestData;

        public List<Quest> GetAllActiveQuests() => GetQuestsByState(QuestState.ACTIVE);

        public List<Quest> GetAllCompleteQuests() => GetQuestsByState(QuestState.COMPLETE);

        public List<Quest> GetAllCurrentQuests() => GetQuestsByState(QuestState.CURRENT);

        public List<Quest> GetAllFailedQuests() => GetQuestsByState(QuestState.FAILED);

        public List<Quest> GetQuestsByState(QuestState state) => _playerQuestData.Where(q => q.State == state).ToList();

        private void UpdateQuestPrerequisites()
        {
            foreach (var quest in GetAllCurrentQuests())
            {
                if (quest.State == QuestState.ACTIVE)
                {

                    if (quest.QuestPrerequisites.TrueForAll(qp => qp.isPrerequisiteMet || qp.isPrerequisiteCanceled))
                    {
                        quest.State = QuestState.CURRENT;

                        if (!quest.PlayerVisible)
                            if (quest.QuestPrerequisites.Any(qp => qp.RevealsQuest && !qp.isPrerequisiteCanceled))
                                quest.PlayerVisible = true;
                    }
                }
            }
        }

        private void UpdateInternalOutcome(QuestOutcome outcome)
        {
            var targetQuest = outcome.DeliveryMetadata[Constants.ReservedMeasurementKeys.OutcomeQuestKey];

            if (string.IsNullOrEmpty(targetQuest))
                return;

            var quest = GetQuestByID(targetQuest);

            if (quest == null) return;

            if (quest.State == QuestState.COMPLETE)
                outcome.Accepted = true;

            if (quest.State != QuestState.CURRENT || quest.State != QuestState.ACTIVE)
                quest.State = QuestState.ACTIVE;
        }

        private void MeasureQuestProgression(List<SubObjectUpdate> measurementUpdates)
        {
            foreach (var quest in ReadPlayerQuestDataFromSaveFile().Where(q => q.State == QuestState.ACTIVE || q.State == QuestState.CURRENT))
            {
                bool allMeasuresMet = quest.QuestMeasurements
                    .Where(qm => qm.QuestCompletionRequirement)
                    .All(qm =>
                    {
                        var progressionValue = measurementUpdates.Find(u => u.QuestID == quest.Id && u.MeasurementId == qm.Id)?.UpdatedValue;
                        if (!string.IsNullOrEmpty(progressionValue))
                            qm.Measure(progressionValue);

                        if (qm.MeasurementFailed)
                        {
                            quest.State = QuestState.FAILED;
                            return false;
                        }
                        return qm.MeasurementReached;
                    });

                if (allMeasuresMet && quest.State != QuestState.FAILED)
                    quest.State = QuestState.COMPLETE;
            }
        }

        public void RepeatQuest(string questId)
        {
            var quest = GetAllCompleteQuests().Find(q => q.Id == questId);
            quest.BeginRepeat();
        }

        public List<Quest> ReadPlayerQuestDataFromSaveFile()
        {
            _playerQuestData = new List<Quest>();


            if (_playerQuestData == null)
            {
                _waitHandle.WaitOne();
                if (File.Exists(basePath))
                {
                    var readFile = File.ReadAllText(basePath);
                    var quests = JsonConvert.DeserializeObject<List<Quest>>(readFile);
                    if (quests != null)
                        _playerQuestData = quests;
                }
                _waitHandle.Set();
            }

            return _playerQuestData;
        }

        public ResponseStatus SavePlayerQuestData() => SavePlayerQuestDataToSaveFile(_playerQuestData);

        private ResponseStatus SavePlayerQuestDataToSaveFile(List<Quest> playerQuestData)
        {
            var questsToSave = JsonConvert.SerializeObject(playerQuestData);
            _waitHandle.WaitOne();

            if (!File.Exists(basePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(basePath));
                var createdProgressionData = File.Create(basePath);
                createdProgressionData.Close();
            }

            File.WriteAllText(basePath, questsToSave);
            _waitHandle.Set();
            return new ResponseStatus(true, "Successfully saved player progression");
        }

        private ResponseStatus SaveBackupPlayerData(string jsonQuestData, string path)
        {
            _waitHandle.WaitOne();
            if (!File.Exists(basePath))
            {
                _waitHandle.Set();
                //fine for a json db but not expected in hosted db
                return new ResponseStatus(true, "No progress exists to back up");
            }

            File.Copy(basePath, basePath.Replace(".json", "Old.json"), true);
            _waitHandle.Set();
            return new ResponseStatus(true, "Successfully backed up player progress");
        }

        private List<Quest> ReadAllAvailableQuestsFromDb()
        {
            List<Quest> dbQuests;
            if (string.IsNullOrEmpty(_sourcDbConnectionPath) && string.IsNullOrEmpty(_sourceDbCollectionName))
                dbQuests = LocalDbQuestController.GetAllQuestsFromDatabase();
            else
                dbQuests = LocalDbQuestController.GetAllQuestsFromDatabase(_sourcDbConnectionPath, _sourceDbCollectionName);

            if (dbQuests == null || dbQuests.Count == 0)
                throw new Exception("Failed to retrieve any quests from database");

            return dbQuests;
        }

        private void DbQuestToQuestProgressionMerge(List<Quest> dbQuests)
        {
            if (dbQuests.Count == 0)
                return;

            var mergeResult = new List<Quest>();
            foreach (var item in dbQuests)
            {
                var existingQuest = _playerQuestData.Find(qm => item.Id == qm.Id);

                if (existingQuest != null)
                {
                    existingQuest.OverwriteNonClientModifiedData(item);
                    mergeResult.Add(existingQuest);
                }
                else
                {
                    mergeResult.Add(item);
                }
            }
            _playerQuestData = mergeResult;
        }
    }
}
