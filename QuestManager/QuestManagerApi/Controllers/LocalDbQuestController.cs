using Newtonsoft.Json;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuestManagerApi.Controllers
{
    /// <summary>
    /// Typically called from the progression manager to give read only access to the main database of quest data, butcan be used to show what quests are available without reading from player progress.
    /// </summary>
    public static class LocalDbQuestController
    {
        public static List<Quest> GetAllQuestsFromDatabase()
        {
           return GetAllQuestsFromDatabase(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"questDb");
        }
        public static List<Quest> GetAllQuestsFromDatabase(string dbConnectionPath,string dbCollectionName)
        {
            string basePath = $"{dbConnectionPath}/{dbCollectionName}.json";
            var readFile = File.ReadAllText(basePath);
            return JsonConvert.DeserializeObject<List<Quest>>(readFile);
        }
    }
}
