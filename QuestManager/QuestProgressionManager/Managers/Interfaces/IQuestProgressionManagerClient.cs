using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.QuestSubObjects;
using System.Collections.Generic;

namespace QuestProgressionManager.Managers.Interfaces
{
    public interface IQuestProgressionManagerClient
    {
        bool AutoSave { get; set; }

        List<QuestOutcome> AcceptQuestOutcomes();
        List<Quest> GetAllActiveQuests();
        List<Quest> GetAllCompleteQuests();
        List<Quest> GetAllCurrentQuests();
        List<Quest> GetAllFailedQuests();
        List<Quest> GetAllQuests();
        Quest GetQuestByID(string id);
        List<Quest> GetQuestsByState(QuestState state);
        List<Quest> ReadPlayerQuestDataFromSaveFile();
        void RepeatQuest(string questId);
        ResponseStatus SavePlayerQuestData();
        void Start();
        void Update(List<SubObjectUpdate> measurementUpdates);
        void UpdateQuestPrerequisite(string prerequisiteUpdate, string id = "");
    }
}