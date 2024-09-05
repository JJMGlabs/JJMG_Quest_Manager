using QuestManagerSharedResources.Model;
using QuestProgressionManager.Managers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QuestProgressionManager.Managers
{
    /// <summary>
    /// Measures QuestLines relative to a single quest. 
    /// Note In future this should be ran each time quests are loaded, and cache or store a list of ids to save effort 
    /// </summary>
    public class QuestlineManager : IQuestlineManager
    {
        private readonly QuestProgressionManagerClient _progressionClient;
        public QuestlineManager(QuestProgressionManagerClient progressionClient)
        {
            _progressionClient = progressionClient;
        }

        public List<Quest> GetNextQuestInQuestline(Quest quest)
        {
            var outcomes = quest.QuestOutcomes.Where(o => o.isQuestlineOutcome()).Select(o => o.getQuestOutcome());

            var result = new List<Quest>();
            foreach (var item in outcomes)
            {
                var nextQuest = _progressionClient.GetQuestByID(item);
                result.Add(nextQuest);
            }
            return result;
        }

        public List<Quest> GetNextQuestInQuestline(string questId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetNextQuestInQuestline(quest);
        }

        public List<Quest> GetPreviousQuestInQuestline(Quest quest)
        {
            var allQuests = _progressionClient.GetAllQuests();

            return allQuests.FindAll(q => q.QuestOutcomes.Any(o => o.isQuestlineOutcome() && o.getQuestOutcome() == quest.Id));
        }

        public List<Quest> GetPreviousQuestInQuestline(string questId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetPreviousQuestInQuestline(quest);
        }

        public List<List<Quest>> GetPreviousQuestsInQuestline(Quest quest)
        {
            var previousQuests = new List<List<Quest>>();
            var priorQuestset = GetPreviousQuestInQuestline(quest);

            if (priorQuestset != null && priorQuestset.Count > 0)
            {
                previousQuests.Add(priorQuestset);
                GetPreviousQuestsInQuestline(previousQuests, priorQuestset);
            }

            previousQuests.Reverse();
            return previousQuests;
        }

        private void GetPreviousQuestsInQuestline(List<List<Quest>> previousQuests, List<Quest> quests)
        {
            var prevFromList = new List<Quest>();

            foreach (var q in quests)
            {
                var prevQuests = GetPreviousQuestInQuestline(q);
                if (prevQuests != null && prevQuests.Count > 0)
                {
                    prevFromList.AddRange(prevQuests);
                }
            }

            if (prevFromList.Count > 0)
            {
                previousQuests.Add(prevFromList);
                GetPreviousQuestsInQuestline(previousQuests, prevFromList);
            }
        }
    

        public List<List<Quest>> GetPreviousQuestsInQuestline(string questId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetPreviousQuestsInQuestline(quest);
        }

        public List<List<Quest>> GetQuestline(Quest quest)
        {
            var priorQuests = GetPreviousQuestsInQuestline(quest);
            var RemainingQuests = GetRemainingQuestsInQuestline(quest);
            var jaggedquest = new List<List<Quest>>() { new List<Quest>() { quest } };

            // seems more efficiant than concat based on the answer here https://stackoverflow.com/questions/4488054/merge-two-or-more-lists-into-one-in-c-sharp-net
            var questline = new List<List<Quest>>(priorQuests.Count + RemainingQuests.Count + jaggedquest.Count);

            questline.AddRange(priorQuests);
            questline.AddRange(jaggedquest);
            questline.AddRange(RemainingQuests);

            return questline;
        }

        public List<List<Quest>> GetQuestline(string questId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetQuestline(quest);
        }
        public List<List<Quest>> GetRemainingQuestsInQuestline(Quest quest)
        {
            var remainingQuests = new List<List<Quest>>();
            var nextQuests = GetNextQuestInQuestline(quest);

            if (nextQuests != null && nextQuests.Count > 0)
            {
                remainingQuests.Add(nextQuests);
                GetRemainingQuestsInQuestline(remainingQuests, nextQuests);
            }

            return remainingQuests;
        }

        private void GetRemainingQuestsInQuestline(List<List<Quest>> remainingQuests, List<Quest> quests)
        {
            var remainingFromList = new List<Quest>();

            foreach (var q in quests)
            {
                var nextQuestset = GetNextQuestInQuestline(q);
                if (nextQuestset != null && nextQuestset.Count > 0)
                {
                    remainingFromList.AddRange(nextQuestset);
                }
            }

            if (remainingFromList.Count > 0)
            {
                remainingQuests.Add(remainingFromList);
                GetRemainingQuestsInQuestline(remainingQuests, remainingFromList);
            }
        }


        public List<List<Quest>> GetRemainingQuestsInQuestline(string questId)
        {
            var quest = _progressionClient.GetQuestByID(questId);
            return GetRemainingQuestsInQuestline(quest);
        }
    }
}
