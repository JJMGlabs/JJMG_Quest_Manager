using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.QuestSubObjects;
using QuestProgressionManager.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuestManager.Managers.Tests
{
    [TestClass()]
    public class QuestProgressionManagerClientTests
    {
        private static string _tempDir;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "JJMGTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            var currentQuest = new Quest { Id = "qst1test1", Name = "Current Quest" };
            currentQuest.State = QuestState.CURRENT;
            currentQuest.AddSubObject(new QuestMeasurement { Name = "Measurement", QuestCompletionRequirement = true });

            var completeQuest = new Quest { Id = "qst1test2", Name = "Complete Quest", Repeatable = true };
            completeQuest.State = QuestState.COMPLETE;
            completeQuest.AddSubObject(new QuestMeasurement { Name = "Measurement" });
            completeQuest.AddSubObject(new QuestOutcome { Name = "Outcome", DeliveryMetadata = new Dictionary<string, string>() });

            File.WriteAllText(
                Path.Combine(_tempDir, "questDb.json"),
                JsonConvert.SerializeObject(new List<Quest> { currentQuest, completeQuest }));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Directory.Delete(_tempDir, true);
        }

        private static QuestProgressionManagerClient ArrangeClient() =>
            new QuestProgressionManagerClient(_tempDir, _tempDir, "questDb");

        [TestMethod()]
        public void QuestProgressionManagerClientTest()
        {
            try
            {
                var sut = ArrangeClient();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void StartTest()
        {
            var sut = ArrangeClient();
            Assert.AreEqual(2, sut.GetAllQuests().Count);
        }

        [TestMethod()]
        public void GetQuestByIDTest()
        {
            var sut = ArrangeClient();
            var result = sut.GetQuestByID("qst1test1");
            Assert.IsNotNull(result);
            Assert.AreEqual("Current Quest", result.Name);
        }

        [TestMethod()]
        public void GetAllCurrentQuestsTest()
        {
            var sut = ArrangeClient();
            var result = sut.GetAllCurrentQuests();
            Assert.IsTrue(result.All(q => q.State == QuestState.CURRENT));
        }

        [TestMethod()]
        public void GetAllCompleteQuestsTest()
        {
            var sut = ArrangeClient();
            var result = sut.GetAllCompleteQuests();
            Assert.IsTrue(result.All(q => q.State == QuestState.COMPLETE));
        }

        [TestMethod()]
        public void GetAllActiveQuestsTest()
        {
            var sut = ArrangeClient();
            var result = sut.GetAllActiveQuests();
            Assert.IsInstanceOfType(result, typeof(List<Quest>));
        }

        [TestMethod()]
        public void GetAllFailedQuestsTest()
        {
            var sut = ArrangeClient();
            var result = sut.GetAllFailedQuests();
            Assert.IsInstanceOfType(result, typeof(List<Quest>));
        }

        [TestMethod()]
        public void AcceptQuestOutcomesTest()
        {
            var sut = ArrangeClient();
            var outcomes = sut.AcceptQuestOutcomes();
            Assert.IsTrue(outcomes.Count > 0);
            Assert.IsTrue(sut.GetAllCompleteQuests().All(q => q.QuestOutcomes.All(o => o.Accepted)));
        }

        [TestMethod()]
        public void RepeatQuestTest()
        {
            var sut = ArrangeClient();
            sut.RepeatQuest("qst1test2");
            Assert.AreEqual(QuestState.CURRENT, sut.GetQuestByID("qst1test2").State);
        }

        [TestMethod()]
        public void SavePlayerQuestDataTest()
        {
            var sut = ArrangeClient();
            var result = sut.SavePlayerQuestData();
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var sut = ArrangeClient();
            try
            {
                sut.Update(new List<SubObjectUpdate>());
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
}
