using System.Collections.Generic;

namespace QuestManagerSharedResources.Model
{
    /// <summary>
    /// Stores the name and description of a questline. The questline graph itself is derived from quest outcomes — this class holds only the identifying metadata.
    /// </summary>
    public class QuestlineMetadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string,string> Metadata { get; set; }
    }
}
