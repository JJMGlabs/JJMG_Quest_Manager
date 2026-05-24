using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.QuestSubObjects;
using System.Collections.Generic;
using System.Linq;

namespace QuestManagerSharedResources.Model.Tests
{
    [TestClass()]
    public class QuestTests
    {
        private static Quest ArrangeQuest() => new Quest() { Id = "qst1test1", Name = "Test Quest" };

        [TestMethod()]
        public void AddSubObjectTest()
        {
            var sut = ArrangeQuest();
            sut.AddSubObject(new QuestMeasurement() { Name = "Test Measurement" });
            Assert.IsTrue(sut.QuestMeasurements.Count == 1);
        }

        [TestMethod()]
        public void AddSubObjectTest_Outcome()
        {
            var sut = ArrangeQuest();
            sut.AddSubObject(new QuestOutcome() { Name = "Test Outcome", DeliveryMetadata = new Dictionary<string, string>() });
            Assert.IsTrue(sut.QuestOutcomes.Count == 1);
        }

        [TestMethod()]
        public void AddSubObjectTest_Prerequisite()
        {
            var sut = ArrangeQuest();
            sut.AddSubObject(new QuestPrerequisite() { Name = "Test Prerequisite" });
            Assert.IsTrue(sut.QuestPrerequisites.Count == 1);
        }

        [TestMethod()]
        public void RemoveSubObjectTest()
        {
            var sut = ArrangeQuest();
            sut.AddSubObject(new QuestMeasurement() { Name = "Test Measurement" });
            string id = sut.QuestMeasurements[0].Id;
            var result = sut.RemoveSubObject<QuestMeasurement>(id);
            Assert.IsTrue(result);
            Assert.IsTrue(sut.QuestMeasurements.Count == 0);
        }

        [TestMethod()]
        public void FindAndReplaceSubObjectTest()
        {
            var sut = ArrangeQuest();
            sut.AddSubObject(new QuestMeasurement() { Name = "Original" });
            var measurement = sut.QuestMeasurements[0];
            measurement.Name = "Updated";
            var result = sut.FindAndReplaceSubObject(measurement);
            Assert.IsTrue(result);
            Assert.AreEqual("Updated", sut.QuestMeasurements[0].Name);
        }

        [TestMethod()]
        public void BeginRepeatTest()
        {
            var sut = ArrangeQuest();
            sut.State = QuestState.COMPLETE;
            sut.Repeatable = true;
            sut.AddSubObject(new QuestMeasurement() { Name = "Measurement", MeasurementReached = true, QuestCompletionRequirement = true });
            sut.AddSubObject(new QuestOutcome() { Name = "Outcome", DeliveryMetadata = new Dictionary<string, string>(), RepeatOutcome = true });
            sut.BeginRepeat();
            Assert.AreEqual(QuestState.CURRENT, sut.State);
            Assert.IsTrue(!sut.QuestMeasurements[0].MeasurementReached);
        }

        [TestMethod()]
        public void BeginRepeatTest_StoresRepeatHistory()
        {
            var sut = ArrangeQuest();
            sut.State = QuestState.COMPLETE;
            sut.Repeatable = true;
            sut.AddSubObject(new QuestMeasurement() { Name = "Measurement" });
            sut.AddSubObject(new QuestOutcome() { Name = "Outcome", DeliveryMetadata = new Dictionary<string, string>() });
            sut.BeginRepeat();
            Assert.IsTrue(sut.GetRepeatData().Count == 1);
        }

        [TestMethod()]
        public void AcceptQuestOutcomesTest()
        {
            var sut = ArrangeQuest();
            sut.State = QuestState.COMPLETE;
            sut.AddSubObject(new QuestOutcome() { Name = "Test Outcome", DeliveryMetadata = new Dictionary<string, string>() });
            var outcomes = sut.AcceptQuestOutcomes();
            Assert.IsTrue(outcomes.Count == 1);
            Assert.IsTrue(sut.QuestOutcomes[0].Accepted);
        }

        [TestMethod()]
        public void AcceptQuestOutcomesTest_AlreadyAccepted()
        {
            var sut = ArrangeQuest();
            sut.State = QuestState.COMPLETE;
            sut.AddSubObject(new QuestOutcome() { Name = "Test Outcome", Accepted = true, DeliveryMetadata = new Dictionary<string, string>() });
            var outcomes = sut.AcceptQuestOutcomes();
            Assert.IsTrue(outcomes.Count == 0);
        }

        [TestMethod()]
        public void OverwriteNonClientModifiedDataTest()
        {
            var sut = ArrangeQuest();
            sut.State = QuestState.CURRENT;
            sut.AddSubObject(new QuestMeasurement() { Name = "Measurement" });
            sut.AddSubObject(new QuestOutcome() { Name = "Outcome", DeliveryMetadata = new Dictionary<string, string>() });
            sut.AddSubObject(new QuestPrerequisite() { Name = "Prerequisite" });

            var updatedQuest = new Quest() { Id = "qst1test1", Name = "Updated Name" };
            updatedQuest.AddSubObject(new QuestMeasurement() { Name = "Updated Measurement" });
            updatedQuest.AddSubObject(new QuestOutcome() { Name = "Updated Outcome", DeliveryMetadata = new Dictionary<string, string>() });
            updatedQuest.AddSubObject(new QuestPrerequisite() { Name = "Updated Prerequisite" });

            sut.OverwriteNonClientModifiedData(updatedQuest);

            Assert.AreEqual("Updated Name", sut.Name);
            Assert.AreEqual(QuestState.CURRENT, sut.State); // State is not overwritten
        }

        [TestMethod()]
        public void SetQuestSubObjectIdsTest()
        {
            var sut = ArrangeQuest();
            var measurements = new List<QuestMeasurement>()
            {
                new QuestMeasurement() { Name = "M1" },
                new QuestMeasurement() { Name = "M2" }
            };
            sut.SetQuestSubObjectIds(measurements, "M");
            Assert.IsTrue(measurements.All(m => !string.IsNullOrEmpty(m.Id)));
        }
    }
}
