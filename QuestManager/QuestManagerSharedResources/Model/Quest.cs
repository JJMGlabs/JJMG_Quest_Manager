using QuestManager.Utility;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.QuestSubObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestManagerSharedResources.Model
{
    /// <summary>
    /// Quest is central to the manager, each quest is independent of the next
    /// Questlines are managed by the questline manager based on questline outcomes
    /// </summary>
    public class Quest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //A record of the state of the quest from a progression pov
        public QuestState State { get; set; }
        //The importance of the quest in terms of progression
        public QuestPriority QuestPriority { get; set; }
        //Can this quest be seen in the UI
        public bool PlayerVisible { get; set; }
        //Is a player activly tracking proggression for this quest
        public bool Tracked { get; set; }
        //Can this quest track repeats of itself
        public bool Repeatable { get; set; }

        public List<QuestMeasurement> QuestMeasurements { get { return _questMeasurements; } }
        private List<QuestMeasurement> _questMeasurements;
        public List<QuestOutcome> QuestOutcomes { get { return _questOutcomes; } }
        private List<QuestOutcome> _questOutcomes;
        public List<QuestPrerequisite> QuestPrerequisites { get { return _questPrerequisites != null ? _questPrerequisites : new List<QuestPrerequisite>(); } }
        private List<QuestPrerequisite> _questPrerequisites;
        private List<Quest> repeats;

        public Quest()
        {
        }

        public void OverwriteData(Quest overwriteData)
        {
            Name = overwriteData.Name;
            Description = overwriteData.Description;
            State = overwriteData.State;
            QuestPriority = overwriteData.QuestPriority;
            PlayerVisible = overwriteData.PlayerVisible;
            _questMeasurements = overwriteData.QuestMeasurements;
            _questOutcomes = overwriteData.QuestOutcomes;
            _questPrerequisites = overwriteData.QuestPrerequisites;
        }

        public void SetQuestSubObjectIds<T>(List<T> questSubObjects, string idPrefix) where T : QuestSubObject
        {
            foreach (var item in questSubObjects)
            {
                if (string.IsNullOrEmpty(item.Id))//dont reset id if one exists
                    item.Id = DbIdHelper.GenerateSubObjectID(Id, idPrefix);
            }
        }

        public void BeginRepeat()
        {
            if (!Repeatable && State != QuestState.COMPLETE)
                return;

            if (repeats == null)
                repeats = new List<Quest>();

            repeats.Add(new Quest()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                State = this.State,
                QuestPriority = this.QuestPriority,
                PlayerVisible = this.PlayerVisible,
                _questMeasurements = this.QuestMeasurements,
                _questOutcomes = this.QuestOutcomes,
                _questPrerequisites = this.QuestPrerequisites
            });

            _questMeasurements.ForEach(m => m.SetForRepeat());

            foreach (var outcome in _questOutcomes)
                if (outcome.RepeatOutcome)
                    outcome.Accepted = false;

            this.State = QuestState.CURRENT;
        }

        public List<Quest> GetRepeatData()
        {
            return repeats;
        }

        public List<QuestOutcome> AcceptQuestOutcomes()
        {
            if(State != QuestState.COMPLETE)
                return new List<QuestOutcome>();

            var outputOutcomes = _questOutcomes.Where(o => !o.Accepted).ToList();

            foreach (var item in _questOutcomes)
                item.Accepted = true;

            return outputOutcomes;
        }

        public void OverwriteNonClientModifiedData(Quest overwriteData)
        {
            Name = overwriteData.Name;
            Description = overwriteData.Description;
            _questMeasurements = OverwriteSubObjectList(overwriteData.QuestMeasurements);
            _questOutcomes = OverwriteSubObjectList(overwriteData.QuestOutcomes);
            _questPrerequisites = OverwriteSubObjectList(overwriteData.QuestPrerequisites);
        }

        private List<T> OverwriteSubObjectList<T>(List<T> overwriteData) where T : QuestSubObject
        {
            List<T> overwriteSubObjects = new List<T>();
            foreach (var item in overwriteData)
            {
                var existing = GetSubObjectList<T>().Find(qm => item.Id == qm.Id);
                if (existing == null)
                    overwriteSubObjects.Add(item);
                else
                    overwriteSubObjects.Add(existing);
            }
            SetQuestSubObjectIds(overwriteSubObjects, GetSubObjectIdPrefix<T>());
            return overwriteSubObjects;
        }

        public void AddSubObject<T>(T subObject) where T : QuestSubObject
        {
            List<T> subObjectList = GetSubObjectList<T>();
            if (subObjectList == null)
            {
                subObjectList = new List<T>();
                SetSubObjectList(subObjectList);
            }

            subObject.Id = DbIdHelper.GenerateSubObjectID(Id, GetSubObjectIdPrefix<T>());

            subObjectList.Add(subObject);
        }

        public bool FindAndReplaceSubObject<T>(T subObject) where T : QuestSubObject
        {
            List<T> subObjectList = GetSubObjectList<T>();
            if (subObjectList == null)
                return false;

            var index = subObjectList.FindIndex(p => p.Id == subObject.Id);
            if (index < 0)
                return false;

            subObjectList[index] = subObject;

            return true;
        }

        public bool RemoveSubObject<T>(string subObjectId) where T : QuestSubObject
        {
            if (string.IsNullOrEmpty(subObjectId))
                return false;

            List<T> subObjectList = GetSubObjectList<T>();
            if (subObjectList == null)
                return false;

            if (subObjectList.RemoveAll(p => p.Id == subObjectId) > 0)
                return true;

            return false;
        }

        private List<T> GetSubObjectList<T>() where T : QuestSubObject
        {
            if (typeof(T) == typeof(QuestPrerequisite))
                return _questPrerequisites as List<T>;
            else if (typeof(T) == typeof(QuestMeasurement))
                return _questMeasurements as List<T>;
            else if (typeof(T) == typeof(QuestOutcome))
                return _questOutcomes as List<T>;

            return null;
        }

        private void SetSubObjectList<T>(List<T> subObjectList) where T : QuestSubObject
        {
            if (typeof(T) == typeof(QuestPrerequisite))
                _questPrerequisites = subObjectList as List<QuestPrerequisite>;
            else if (typeof(T) == typeof(QuestMeasurement))
                _questMeasurements = subObjectList as List<QuestMeasurement>;
            else if (typeof(T) == typeof(QuestOutcome))
                _questOutcomes = subObjectList as List<QuestOutcome>;
        }

        private string GetSubObjectIdPrefix<T>() where T : QuestSubObject
        {
            if (typeof(T) == typeof(QuestPrerequisite))
                return Constants.Prefix.QuestSubObjectIdPrefix.QuestPrerequisitePrefix;
            else if (typeof(T) == typeof(QuestMeasurement))
                return Constants.Prefix.QuestSubObjectIdPrefix.QuestMeasurementPrefix;
            else if (typeof(T) == typeof(QuestOutcome))
                return Constants.Prefix.QuestSubObjectIdPrefix.QuestOutcomePrefix;

            return string.Empty;
        }
    }
}
