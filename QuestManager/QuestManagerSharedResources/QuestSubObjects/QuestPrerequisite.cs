﻿using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.Model.Utility;
using System.Collections.Generic;

namespace QuestManagerSharedResources.QuestSubObjects
{
    /// <summary>
    //A Prerequisite is something that is needed to make a quest available to a user
    //Remember when setting Prerequisites, all need to be met or canceled to Unlock the quest
    //A Quest should be unlocked via the requirement of a previous quest rather than as an outcome
    //Prerequisite are updated by the game loop
    /// <summary>
    public class QuestPrerequisite : SubObjectMeasurable
    {
        public bool isPrerequisiteMet { get; set; }
        //Should this prerequisite be factored into player progression
        public bool isPrerequisiteCanceled { get; set; }
        //If the quest is revealed to a player when this requisite is met(including other prerequisites)
        public bool RevealsQuest { get; set; }
        //to target the completion of a quest as the prerequisite use Constants.ReservedMeasurementKeys.PrerequisiteQuestKey as the key and id as the value
        public Dictionary<string, string> Metadata { get; set; }

        public virtual void UpdatePrerequisite(string prerequisiteMetadataUpdate)
        {
            if (Comparator == SubObjectComparator.NA)
                return;

            ProgressValue = prerequisiteMetadataUpdate;
            isPrerequisiteMet = QuestSubObjectComparisonUtility.PerformComparison(ProgressValue, Comparator, TargetValue);
        }

        public virtual void UpdatePrerequisite(bool prerequisiteMet) => isPrerequisiteMet = prerequisiteMet;
        public virtual void CancelPrerequisite(bool cancelPrerequisite) => isPrerequisiteCanceled = cancelPrerequisite;
    }
}
