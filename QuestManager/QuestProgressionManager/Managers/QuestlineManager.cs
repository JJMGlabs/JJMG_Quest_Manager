using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestProgressionManager.Managers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QuestProgressionManager.Managers
{
    /// <summary>
    /// Measures QuestLines 
    /// Questline Metadata is a requirment to make a quest outcome part of a questline
    /// </summary>
    public class QuestlineManager : IQuestlineManager
    {
        private readonly QuestProgressionManagerClient _progressionClient;
        public QuestlineManager(QuestProgressionManagerClient progressionClient)
        {
            _progressionClient = progressionClient;
        }

        public List<Quest> GetNextQuestInQuestline(Quest quest, string questlineId)
        {
            var result = new List<Quest>();
            if (quest == null || string.IsNullOrEmpty(questlineId))
                return result;

            var outcomes = quest.QuestOutcomes.Where(o => o.isQuestlineOutcome() && o.GetQuestlineId() == questlineId);

            foreach (var item in outcomes)
            {                
                var nextQuest = _progressionClient.GetQuestByID(item.GetQuestIdFromOutcome());

                if(nextQuest != null && nextQuest.QuestOutcomes.Any(o => o.GetQuestlineId() == questlineId))
                result.Add(nextQuest);
            }
            return result;
        }

        public List<Quest> GetNextQuestInQuestline(string questId, string questlineId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetNextQuestInQuestline(quest,questlineId);
        }

        public List<Quest> GetPreviousQuestInQuestline(Quest quest, string questlineId, bool hasBeenCompleted = false)
        {
            var allQuests = hasBeenCompleted ? _progressionClient.GetAllCompleteQuests() : _progressionClient.GetAllQuests();

            return allQuests.FindAll(q => q.QuestOutcomes.Any(o => o.isQuestlineOutcome() && o.GetQuestIdFromOutcome() == quest.Id && o.GetQuestlineId() == questlineId));
        }

        public List<Quest> GetPreviousQuestInQuestline(string questId, string questlineId, bool hasBeenCompleted = false)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetPreviousQuestInQuestline(quest,questlineId,hasBeenCompleted);
        }

        public List<List<Quest>> GetPreviousQuestsInQuestline(Quest quest, string questlineId, bool hasBeenCompleted = false)
        {
            var previousQuests = new List<List<Quest>>();

            if (quest == null || string.IsNullOrEmpty(questlineId))
                return previousQuests;


            var priorQuestset = GetPreviousQuestInQuestline(quest,questlineId,hasBeenCompleted);

            if (priorQuestset != null && priorQuestset.Count > 0)
            {
                previousQuests.Add(priorQuestset);
                GetPreviousQuestsInQuestline(previousQuests, priorQuestset,questlineId,hasBeenCompleted);
            }

            previousQuests.Reverse();
            return previousQuests;
        }

        private void GetPreviousQuestsInQuestline(List<List<Quest>> previousQuests, List<Quest> quests, string questlineId, bool hasBeenCompleted = false)
        {
            var prevFromList = new List<Quest>();

            foreach (var q in quests)
            {
                var prevQuests = GetPreviousQuestInQuestline(q,questlineId,hasBeenCompleted);
                if (prevQuests != null && prevQuests.Count > 0)
                {
                    prevFromList.AddRange(prevQuests);
                }
            }

            if (prevFromList.Count > 0)
            {
                previousQuests.Add(prevFromList);
                GetPreviousQuestsInQuestline(previousQuests, prevFromList,questlineId,hasBeenCompleted);
            }
        }
    

        public List<List<Quest>> GetPreviousQuestsInQuestline(string questId, string questlineId, bool hasBeenCompleted = false)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetPreviousQuestsInQuestline(quest, questlineId, hasBeenCompleted);
        }

        public List<List<Quest>> GetQuestline(string questlineId)
        {
            var allQuests = _progressionClient.GetAllQuests()
                .Where(q => q.QuestOutcomes.Any(o => o.isQuestlineOutcome() && o.GetQuestlineId() == questlineId))
                .ToList();

            var mappedQuests = allQuests.ToDictionary(q => q.Id);

            List<List<Quest>> orderedQuestlinePaths = new List<List<Quest>>();

            // Start at root quests
            var rootQuests = allQuests
                .Where(q => !allQuests.Any(otherQuest => otherQuest.QuestOutcomes
                    .Any(o => o.GetQuestIdFromOutcome() == q.Id)))  // Root quests are not outcomes of other quests
                .ToList();

            foreach (var rootQuest in rootQuests)
                BuildQuestlinePath(rootQuest, new List<Quest>(), mappedQuests, orderedQuestlinePaths);

            return orderedQuestlinePaths;
        }

        private static void BuildQuestlinePath(Quest currentQuest, List<Quest> currentPath, Dictionary<string, Quest> mappedQuests, List<List<Quest>> orderedQuestlinePaths)
        {
            HashSet<string> visitedQuests = new HashSet<string>();

            if (visitedQuests.Contains(currentQuest.Id))
                return;

            currentPath.Add(currentQuest);
            visitedQuests.Add(currentQuest.Id);

            var nextQuestIds = currentQuest.QuestOutcomes
                .Select(o => o.GetQuestIdFromOutcome())
                .Where(nextQuestId => !string.IsNullOrEmpty(nextQuestId))
                .ToList();

            if (!nextQuestIds.Any())
            {
                orderedQuestlinePaths.Add(new List<Quest>(currentPath));
            }
            else
            {
                foreach (var nextQuestId in nextQuestIds)
                    if (mappedQuests.ContainsKey(nextQuestId))
                        BuildQuestlinePath(mappedQuests[nextQuestId], new List<Quest>(currentPath), mappedQuests, orderedQuestlinePaths);
            }
        }

        public List<List<Quest>> GetRemainingQuestsInQuestline(Quest quest, string questlineId)
        {
            var remainingQuests = new List<List<Quest>>();
            var nextQuests = GetNextQuestInQuestline(quest, questlineId);

            if (nextQuests != null && nextQuests.Count > 0)
            {
                remainingQuests.Add(nextQuests);
                GetRemainingQuestsInQuestline(remainingQuests, nextQuests, questlineId);
            }

            return remainingQuests;
        }

        private void GetRemainingQuestsInQuestline(List<List<Quest>> remainingQuests, List<Quest> quests, string questlineId)
        {
            var remainingFromList = new List<Quest>();

            foreach (var q in quests)
            {
                var nextQuestset = GetNextQuestInQuestline(q,questlineId);
                if (nextQuestset != null && nextQuestset.Count > 0)
                {
                    remainingFromList.AddRange(nextQuestset);
                }
            }

            if (remainingFromList.Count > 0)
            {
                remainingQuests.Add(remainingFromList);
                GetRemainingQuestsInQuestline(remainingQuests, remainingFromList, questlineId);
            }
        }


        public List<List<Quest>> GetRemainingQuestsInQuestline(string questId,string questlineId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetRemainingQuestsInQuestline(quest, questlineId);
        }
    }
}
