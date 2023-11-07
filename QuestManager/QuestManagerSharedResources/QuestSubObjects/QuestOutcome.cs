using Newtonsoft.Json;
using QuestManagerSharedResources.Model;
using System.Collections.Generic;

namespace QuestManagerSharedResources.QuestSubObjects
{
    /// <summary>
    /// Outcomes are the direct result of a quest to be activated when all measurements or external game events are satisfied.    
    /// </summary>
    /// 

    public class QuestOutcome : QuestSubObject
    {
        //It is assumed all dependencies (if any) are required for an outcome to occur
        public List<string> MeasurementDependancyIds { get; set; }        
        public List<string> GameEventDependancyIds { get; set; }

        //Metadata to be used to handle delivery of the outcome in the app
        public Dictionary<string,string> DeliveryMetadata { get; set; }
        //Has the quest been accepted by the app
        public bool Accepted { get; set; }
        public bool RepeatOutcome { get; set; }
    }
}
