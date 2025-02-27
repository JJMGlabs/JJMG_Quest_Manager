using System.Collections.Generic;

namespace QuestManagerSharedResources.Model
{
    public class QuestlineMetadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string,string> Metadata { get; set; }
    }
}
