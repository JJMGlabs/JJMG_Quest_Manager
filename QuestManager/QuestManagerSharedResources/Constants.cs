using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QuestManagerSharedResources
{
    public static class Constants
    {
        public static class Prefix
        {
            public static string QuestPrefix = "qst";
            public static string QuestlinePrefix = "qln";
            public static class QuestSubObjectIdPrefix
            {
                public static string QuestMeasurementPrefix = "M";
                public static string QuestOutcomePrefix = "O";
                public static string QuestPrerequisitePrefix = "P";
            }
            //Keys used for measurement only by the quest manager(not the game)

        }
        public static class ReservedMeasurementKeys
        {
            //used to mark a quest id for an outcome
            public static string OutcomeQuestKey = "Quest" + Prefix.QuestSubObjectIdPrefix.QuestOutcomePrefix;
            //used to mark an outcome with a quest id as a questline rather than just a quest received
            public static string QuestlineIdKey = "questLineId";
        }
        public static class UtilityValues
        {
            public static string WaitHandleForQuestDb = "JJMGquestDbWaitHandle";
            public static string WaitHandleForQuestLineDb = "JJMGquestlineDbWaitHandle";
        }
    }
}
