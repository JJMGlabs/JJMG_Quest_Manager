using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using System.Collections.Generic;

namespace QuestManager.Managers.Interfaces
{
    public interface IQuestDbConnection
    {
        /// <summary>
        /// Serializes and writes the current quest data to the Database (JSON file or server).
        /// </summary>
        ResponseStatus SaveQuestDbChanges();

        /// <summary>
        /// Creates a new quest with a generated ID and adds it to the database.
        /// </summary>
        Quest CreateQuest(Quest quest);

        /// <summary>
        /// Creates multiple quests with generated IDs and adds them to the database.
        /// </summary>
        List<Quest> CreateQuests(List<Quest> quests);

        /// <summary>
        /// Retrieves all quests from the database. Creates the database entry if it does not exist.
        /// </summary>
        List<Quest> GetAllQuests();

        /// <summary>
        /// Retrieves a specific quest by its ID.
        /// </summary>
        Quest GetQuest(string id);

        /// <summary>
        /// Replaces all quests in the database with the provided list.
        /// </summary>
        List<Quest> OverwriteAllQuests(List<Quest> quests);

        /// <summary>
        /// Updates a single quest in the database.
        /// </summary>
        Quest UpdateQuest(Quest quest);

        /// <summary>
        /// Updates a list of quests in the database.
        /// </summary>
        List<Quest> UpdateQuests(List<Quest> quests);

        /// <summary>
        /// Deletes a specific quest by its ID.
        /// </summary>
        ResponseStatus DeleteQuest(string QuestID);

        /// <summary>
        /// Deletes a list of quests from the database.
        /// </summary>
        List<ResponseStatus> DeleteQuests(List<Quest> quests);
    }
}
