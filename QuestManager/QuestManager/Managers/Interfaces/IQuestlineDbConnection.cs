using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using System.Collections.Generic;

namespace QuestManager.Managers.Interfaces
{
    public interface IQuestlineDbConnection
    {
        /// <summary>
        /// Retrieves all questline metadata records from the database.
        /// </summary>
        List<QuestlineMetadata> GetQuestlines();

        /// <summary>
        /// Adds a new questline metadata record to the database.
        /// </summary>
        ResponseStatus CreateQuestLine(QuestlineMetadata questline);

        /// <summary>
        /// Replaces all questline metadata records with the provided list.
        /// </summary>
        ResponseStatus WriteQuestLines(List<QuestlineMetadata> questlines);

        /// <summary>
        /// Serializes and writes the current questline metadata to the database.
        /// </summary>
        ResponseStatus SaveDbChanges();
    }

    public interface IQuestlineQuestRelationshipConnection
    {
        /// <summary>
        /// Builds and returns the questline as a list of paths. Each inner list is one possible sequence of quests through the questline graph.
        /// </summary>
        List<List<Quest>> GetQuestline(string questlineId);
    }
}
