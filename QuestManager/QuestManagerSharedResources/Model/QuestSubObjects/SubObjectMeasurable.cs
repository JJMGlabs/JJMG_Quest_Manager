using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;

namespace QuestManagerSharedResources.QuestSubObjects
{
    public class SubObjectMeasurable : QuestSubObject
    {
        //The object name or key being measured
        public string Measurement { get; set; }
        public string TargetValue { get; set; }
        public string ProgressValue { get; set; }
        public SubObjectComparator Comparator { get; set; }
    }
}
