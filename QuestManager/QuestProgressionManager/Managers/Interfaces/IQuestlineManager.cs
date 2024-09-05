using QuestManagerSharedResources.Model;
using System.Collections.Generic;

namespace QuestProgressionManager.Managers.Interfaces
{
    /// <summary>
    /// Questlines are managed as a list containing lists of quests which can lead into one another. Each list of quests can be considered a 
    /// </summary>
    public interface IQuestlineManager
    {
        List<List<Quest>> GetQuestline(Quest quest);
        List<List<Quest>> GetQuestline(string questId);
        List<Quest> GetNextQuestInQuestline(Quest quest);
        List<Quest> GetNextQuestInQuestline(string questId);
        List<Quest> GetPreviousQuestInQuestline(Quest quest);
        List<Quest> GetPreviousQuestInQuestline(string questId);
        List<List<Quest>> GetRemainingQuestsInQuestline(Quest quest);
        List<List<Quest>> GetRemainingQuestsInQuestline(string questId);
        List<List<Quest>> GetPreviousQuestsInQuestline(Quest quest);
        List<List<Quest>> GetPreviousQuestsInQuestline(string questId);
    }
}
