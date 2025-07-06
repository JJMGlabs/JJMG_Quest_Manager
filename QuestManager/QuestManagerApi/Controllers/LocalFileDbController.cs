using Newtonsoft.Json;
using QuestManagerSharedResources.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuestManagerClientApi.Controllers
{
    public class LocalFileDbController
    {
        public static List<T> GetFallbackCollectionFromDatabase<T>()
        {
            return GetCollectionFromDatabase<T>(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "questDb");
        }

        public static List<T> GetCollectionFromDatabase<T>(string dbConnectionPath, string dbCollectionName,string delimiter = "")
        {
            string basePath = $"{dbConnectionPath}/{dbCollectionName}.json";
            var readFile = File.ReadAllText(basePath);

            if (!string.IsNullOrEmpty(delimiter))
            {
                var jsonData = ControllerHelper.RetreiveDelimitedData(delimiter, readFile);
                return JsonConvert.DeserializeObject<List<T>>(jsonData) ?? new List<T>();
            }
            else
            {
                return JsonConvert.DeserializeObject<List<T>>(readFile) ?? new List<T>();
            }
        }
    }
}
