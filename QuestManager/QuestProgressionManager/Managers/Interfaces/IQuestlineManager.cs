using QuestManagerSharedResources.Model;
using System.Collections.Generic;

namespace QuestProgressionManager.Managers.Interfaces
{
    /// <summary>
    /// Questlines are managed as a list containing lists of quests which can lead into one another. Each list of quests can be considered a 
    /// </summary>
    public interface IQuestlineManager
    {
        List<List<Quest>> GetQuestline(string questlineId);
        List<Quest> GetNextQuestInQuestline(Quest quest, string questlineId);
        List<Quest> GetNextQuestInQuestline(string questId, string questlineId);
        List<Quest> GetPreviousQuestInQuestline(Quest quest, string questlineId, bool hasBeenCompleted);
        List<Quest> GetPreviousQuestInQuestline(string questId, string questlineId, bool hasBeenCompleted);
        List<List<Quest>> GetRemainingQuestsInQuestline(Quest quest, string questlineId);
        List<List<Quest>> GetRemainingQuestsInQuestline(string questId, string questlineId);
        List<List<Quest>> GetPreviousQuestsInQuestline(Quest quest, string questlineId, bool hasBeenCompleted);
        List<List<Quest>> GetPreviousQuestsInQuestline(string questId, string questlineId, bool hasBeenCompleted);        
    }
}
