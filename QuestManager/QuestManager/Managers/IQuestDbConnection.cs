using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using System.Collections.Generic;

namespace QuestManager.Managers
{
    public interface IQuestDbConnection
    {
        ResponseStatus SaveQuestDbChanges();
        Quest CreateQuest(Quest quest);
        List<Quest> CreateQuests(List<Quest> quests);
        List<Quest> GetAllQuests();
        Quest GetQuest(string id);
        List<Quest> OverwriteAllQuests(List<Quest> quests);
        Quest UpdateQuest(Quest quest);
        List<Quest> UpdateQuests(List<Quest> quests);
        ResponseStatus DeleteQuest(string QuestID);
        List<ResponseStatus> DeleteQuests(List<Quest> quests);
    }
}