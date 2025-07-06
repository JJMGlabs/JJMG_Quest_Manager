using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Utility;
using System.Collections.Generic;

namespace QuestManager.Managers.Interfaces
{
    public interface IQuestlineDbConnection
    {
        List<QuestlineMetadata> GetQuestlines();
        ResponseStatus CreateQuestLine(QuestlineMetadata questline);
        ResponseStatus WriteQuestLines(List<QuestlineMetadata> questlines);
        ResponseStatus SaveDbChanges();
    }

    public interface IQuestlineQuestRelationshipConnection
    {
        List<List<Quest>> GetQuestline(string questlineId);
    }
}
