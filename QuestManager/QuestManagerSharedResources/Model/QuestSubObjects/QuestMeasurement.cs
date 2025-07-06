using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.Model.Utility;
using System.Collections.Generic;

namespace QuestManagerSharedResources.QuestSubObjects
{
    /// <summary>
    /// Measurements are used to measure quest progression, each quest needs at least one set as a completion prerequisite to be possible.
    /// </summary>
    public class QuestMeasurement : SubObjectMeasurable
    {
        public bool MeasurementReached { get; set; }
        public bool MeasurementFailed { get; set; }
        public bool QuestCompletionRequirement { get; set; }
        public List<string> TriggeredGameEventIDs { get; set; }

        public virtual void Measure(string progressvalue)
        {
            if (Comparator == SubObjectComparator.NA)
                return;

            ProgressValue = progressvalue;
            MeasurementReached = QuestSubObjectComparisonUtility.PerformComparison(ProgressValue, Comparator, TargetValue);
        }

        internal void SetForRepeat()
        {
            MeasurementReached = false;
            MeasurementFailed = false;
        }
    }
}
