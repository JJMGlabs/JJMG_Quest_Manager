namespace SharedLibrary
{
    public static class Constants
    {
        public static string QuestDbConfigurationSection = "QuestDb";
        public static string QuestLineDbConfigurationSection = "QuestlineDb";
        /// <summary>
        /// Routes are duplicated at the head of each page
        /// </summary>
        public static class Routes
        {
            public static string QuestList = "";
            public static string QuestForm = "questForm";
            public static string Settings = "settings";
            public static string QuestLines = "questLines";
            public static string QuestLineForm = "questLineForm";
        }
    }
}
