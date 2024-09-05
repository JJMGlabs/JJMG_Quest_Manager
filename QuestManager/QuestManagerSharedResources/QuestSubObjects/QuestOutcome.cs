using Newtonsoft.Json;
using QuestManagerSharedResources.Model;
using System.Collections.Generic;

namespace QuestManagerSharedResources.QuestSubObjects
{
    /// <summary>
    /// Outcomes are the direct result of a quest to be activated when all measurements or external game events are satisfied.
    /// A Quest should be unlocked via the requirement of a previous quest rather than as an outcome
    /// </summary>
    /// 

    public class QuestOutcome : QuestSubObject
    {
        //It is assumed all dependencies (if any) are required for an outcome to occur
        public List<string> MeasurementDependancyIds { get; set; }        
        public List<string> GameEventDependancyIds { get; set; }

        //To make a quest deliverable as an outcome use Constants.ReservedMeasurementKeys.PrerequisiteQuestKey as the key and id as the value
        //Metadata to be used to handle delivery of the outcome in the app
        public Dictionary<string,string> DeliveryMetadata { get; set; }
        //Has the quest been accepted by the app
        public bool Accepted { get; set; }
        public bool RepeatOutcome { get; set; }
        public bool isQuestlineOutcome()
        {
            if (DeliveryMetadata == null || DeliveryMetadata.Count == 0)
                return false;

            if (DeliveryMetadata.TryGetValue(Constants.ReservedMeasurementKeys.IsQuestlineKey, out string isQuestlineValue))
                if (bool.TryParse(isQuestlineValue, out bool isQuestline))
                    return isQuestline;

            return false;
        }

        public string getQuestOutcome()
        {
            if (DeliveryMetadata == null || DeliveryMetadata.Count == 0)
                return string.Empty;

            if (DeliveryMetadata.TryGetValue(Constants.ReservedMeasurementKeys.OutcomeQuestKey, out string questOutcome))
                return questOutcome;

            return string.Empty;
        }
    }
}
