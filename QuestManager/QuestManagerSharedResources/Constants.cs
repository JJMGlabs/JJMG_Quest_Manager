using System;
using System.Collections.Generic;
using System.Text;

namespace QuestManagerSharedResources
{
    public static class Constants
    {
        public static class Prefix
        {
            public static string QuestPrefix = "qst";
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
            //used to mark a quest id for a prerequisite
            public static string PrerequisiteQuestKey = "Quest" + Prefix.QuestSubObjectIdPrefix.QuestPrerequisitePrefix;
        }
    }
}
