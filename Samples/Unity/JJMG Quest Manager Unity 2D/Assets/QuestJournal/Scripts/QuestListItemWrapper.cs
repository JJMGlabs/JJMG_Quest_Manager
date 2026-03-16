using QuestManagerSharedResources.Model;

namespace QuestJournal.UI
{
    public class QuestListItemWrapper
    {
        public bool IsHeader { get; private set; }
        public string HeaderTitle { get; private set; }
        public string HeaderId { get; private set; }
        public Quest Quest { get; private set; }

        public static QuestListItemWrapper Header(string id, string title) => new QuestListItemWrapper { IsHeader = true, HeaderId = id, HeaderTitle = title };
        public static QuestListItemWrapper ForQuest(Quest q) => new QuestListItemWrapper { IsHeader = false, Quest = q };
    }
}
