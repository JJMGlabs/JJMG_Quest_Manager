using QuestManagerSharedResources.Model;
using System.Collections.Generic;

namespace QuestProgressionManager.Managers.Interfaces
{
    /// <summary>
    /// Navigates the questline graph using player progression data. Each questline is returned as a list of paths, where each path is one possible sequence of quests from start to end.
    /// </summary>
    public interface IQuestlineManager
    {
        /// <summary>
        /// Returns the full questline as a list of paths. Each inner list is one possible sequence of quests from start to end.
        /// </summary>
        List<List<Quest>> GetQuestline(string questlineId);

        /// <summary>
        /// Returns the quests that follow the given quest in the questline. Returns multiple quests if the questline branches at this point.
        /// </summary>
        List<Quest> GetNextQuestInQuestline(Quest quest, string questlineId);

        /// <summary>
        /// Returns the quests that follow the given quest ID in the questline.
        /// </summary>
        List<Quest> GetNextQuestInQuestline(string questId, string questlineId);

        /// <summary>
        /// Returns the quests that precede the given quest in the questline, filtered by completion state.
        /// </summary>
        List<Quest> GetPreviousQuestInQuestline(Quest quest, string questlineId, bool hasBeenCompleted);

        /// <summary>
        /// Returns the quests that precede the given quest ID in the questline, filtered by completion state.
        /// </summary>
        List<Quest> GetPreviousQuestInQuestline(string questId, string questlineId, bool hasBeenCompleted);

        /// <summary>
        /// Returns all quest paths that have not yet been followed from the given quest's position onward.
        /// </summary>
        List<List<Quest>> GetRemainingQuestsInQuestline(Quest quest, string questlineId);

        /// <summary>
        /// Returns all quest paths that have not yet been followed from the given quest ID's position onward.
        /// </summary>
        List<List<Quest>> GetRemainingQuestsInQuestline(string questId, string questlineId);

        /// <summary>
        /// Returns all quest paths that precede the given quest in the questline, filtered by completion state.
        /// </summary>
        List<List<Quest>> GetPreviousQuestsInQuestline(Quest quest, string questlineId, bool hasBeenCompleted);

        /// <summary>
        /// Returns all quest paths that precede the given quest ID in the questline, filtered by completion state.
        /// </summary>
        List<List<Quest>> GetPreviousQuestsInQuestline(string questId, string questlineId, bool hasBeenCompleted);
    }
}
