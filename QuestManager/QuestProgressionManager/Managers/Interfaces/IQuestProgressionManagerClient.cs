using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.Model.Utility;
using QuestManagerSharedResources.QuestSubObjects;
using System.Collections.Generic;

namespace QuestProgressionManager.Managers.Interfaces
{
    public interface IQuestProgressionManagerClient
    {
        /// <summary>
        /// When true, player quest data is saved automatically after each update.
        /// </summary>
        bool AutoSave { get; set; }

        /// <summary>
        /// Returns all outcomes for completed quests and marks them as accepted so they are not returned again.
        /// </summary>
        List<QuestOutcome> AcceptQuestOutcomes();

        /// <summary>
        /// Retrieves all quests in the ACTIVE state.
        /// </summary>
        List<Quest> GetAllActiveQuests();

        /// <summary>
        /// Retrieves all quests in the COMPLETE state.
        /// </summary>
        List<Quest> GetAllCompleteQuests();

        /// <summary>
        /// Retrieves all quests in the CURRENT state.
        /// </summary>
        List<Quest> GetAllCurrentQuests();

        /// <summary>
        /// Retrieves all quests in the FAILED state.
        /// </summary>
        List<Quest> GetAllFailedQuests();

        /// <summary>
        /// Retrieves all quests currently held in the player progression data.
        /// </summary>
        List<Quest> GetAllQuests();

        /// <summary>
        /// Retrieves a quest by its ID.
        /// </summary>
        Quest GetQuestByID(string id);

        /// <summary>
        /// Retrieves all quests matching the specified state.
        /// </summary>
        List<Quest> GetQuestsByState(QuestState state);

        /// <summary>
        /// Reads player quest data from the save file.
        /// </summary>
        List<Quest> ReadPlayerQuestDataFromSaveFile();

        /// <summary>
        /// Resets a completed quest's measurements and returns it to CURRENT for a new run.
        /// </summary>
        void RepeatQuest(string questId);

        /// <summary>
        /// Saves player quest data to the save file.
        /// </summary>
        ResponseStatus SavePlayerQuestData();

        /// <summary>
        /// Reads player quest data from the save file, merges it with quests from the database, and saves the result.
        /// </summary>
        void Start();

        /// <summary>
        /// Measures quest progression based on the provided updates and evaluates prerequisites.
        /// </summary>
        void Update(List<SubObjectUpdate> measurementUpdates);

        /// <summary>
        /// Updates quest prerequisites based on the provided value. Targets all quests unless a quest ID is specified.
        /// </summary>
        void UpdateQuestPrerequisite(string prerequisiteUpdate, string id = "");
    }
}
