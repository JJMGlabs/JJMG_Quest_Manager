
namespace Assets.QuestJournalApplication.QuestJournal
{
    public static class Constants
    {
        public static class QuestDb
        {
            public const string QuestDbJsonFile = "QuestDb";
        }
        //TODO: revisit as potential configuration values
        public static class DbBindingSettings
        {
            //Frequent rebuild was presenting a noticeable impact on performance, coroutine restricts this to value of this timer
            public const float RebuildTimerSeconds = 2;
            public const bool RebuildUpdate = true;
        }
    }

}
