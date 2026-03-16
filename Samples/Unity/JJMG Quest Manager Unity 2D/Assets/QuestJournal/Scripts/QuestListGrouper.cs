using QuestManagerSharedResources.Model;
using QuestProgressionManager.Managers;
using System.Collections.Generic;
using System.Linq;

namespace QuestJournal.UI
{
    public class QuestListGrouper
    {
        private readonly List<QuestlineMetadata> _questlines;
        private readonly QuestlineManager _questlineManager;
        private readonly Dictionary<string, bool> _questlineExpanded;

        public QuestListGrouper(List<QuestlineMetadata> questlines, QuestlineManager questlineManager, Dictionary<string, bool> questlineExpanded)
        {
            _questlines = questlines;
            _questlineManager = questlineManager;
            _questlineExpanded = questlineExpanded;
        }

        public List<QuestListItemWrapper> BuildGroupedList(List<Quest> allQuests)
        {
            var list = new List<QuestListItemWrapper>();
            var questsInQuestlines = new HashSet<string>();

            foreach (var ql in _questlines)
            {
                var paths = _questlineManager.GetQuestline(ql.Id) ?? new List<List<Quest>>();
                var expanded = GetExpandedState(ql.Id, paths);
                _questlineExpanded[ql.Id] = expanded;

                list.Add(QuestListItemWrapper.Header(ql.Id, ql.Name));

                if (expanded)
                {
                    AddQuestsFromPaths(list, paths, questsInQuestlines);
                }
            }

            AddUngroupedQuests(list, allQuests, questsInQuestlines);
            return list;
        }

        private bool GetExpandedState(string id, List<List<Quest>> paths)
        {
            if (_questlineExpanded.TryGetValue(id, out var expanded))
                return expanded;

            // Default-expand if questline has quests
            var hasQuests = paths.Any(p => p != null && p.Count > 0);
            return hasQuests;
        }

        private void AddQuestsFromPaths(List<QuestListItemWrapper> list, List<List<Quest>> paths, HashSet<string> questsInQuestlines)
        {
            var added = new HashSet<string>();
            foreach (var path in paths)
            {
                foreach (var q in path)
                {
                    if (q != null && !added.Contains(q.Id))
                    {
                        list.Add(QuestListItemWrapper.ForQuest(q));
                        added.Add(q.Id);
                        questsInQuestlines.Add(q.Id);
                    }
                }
            }
        }

        private void AddUngroupedQuests(List<QuestListItemWrapper> list, List<Quest> allQuests, HashSet<string> questsInQuestlines)
        {
            var otherQuests = allQuests.Where(q => !questsInQuestlines.Contains(q.Id)).ToList();
            if (otherQuests.Any())
            {
                list.Add(QuestListItemWrapper.Header("other", "Other Quests"));
                foreach (var q in otherQuests)
                    list.Add(QuestListItemWrapper.ForQuest(q));
            }
        }
    }
}
