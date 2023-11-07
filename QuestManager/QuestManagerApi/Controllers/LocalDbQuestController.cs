using Newtonsoft.Json;
using QuestManagerSharedResources;
using QuestManagerSharedResources.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuestManagerApi.Controllers
{
    public static class LocalDbQuestController
    {
        public static List<Quest> GetAllQuestsFromDatabase()
        {
            string basePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/questDb.json";
            var readFile = File.ReadAllText(basePath);
            return JsonConvert.DeserializeObject<List<Quest>>(readFile);
        }
    }
}
