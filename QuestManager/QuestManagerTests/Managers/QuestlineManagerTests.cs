using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.QuestSubObjects;
using QuestProgressionManager.Managers;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuestManager.Managers.Tests
{
    [TestClass()]
    public class QuestlineManagerTests
    {
        private static string _tempDir;
        private const string QuestlineId = "ql1";
        private const string QuestAId = "qst1test1";
        private const string QuestBId = "qst1test2";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "JJMGTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            // Quest A → Quest B via questline outcome
            var questA = new Quest { Id = QuestAId, Name = "Quest A" };
            questA.State = QuestState.CURRENT;
            questA.AddSubObject(new QuestOutcome
            {
                Name = "Next",
                DeliveryMetadata = new Dictionary<string, string>
                {
                    { "questLineId", QuestlineId },
                    { "QuestO", QuestBId }
                }
            });

            var questB = new Quest { Id = QuestBId, Name = "Quest B" };
            questB.State = QuestState.CURRENT;
            questB.AddSubObject(new QuestOutcome
            {
                Name = "Terminal",
                DeliveryMetadata = new Dictionary<string, string> { { "questLineId", QuestlineId } }
            });

            File.WriteAllText(
                Path.Combine(_tempDir, "questDb.json"),
                JsonConvert.SerializeObject(new List<Quest> { questA, questB }));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Directory.Delete(_tempDir, true);
        }

        private static QuestProgressionManagerClient ArrangeClient() =>
            new QuestProgressionManagerClient(_tempDir, _tempDir, "questDb");

        private static QuestlineManager ArrangeManager(QuestProgressionManagerClient client) =>
            new QuestlineManager(client);

        [TestMethod()]
        public void GetQuestlineTest()
        {
            var client = ArrangeClient();
            var sut = ArrangeManager(client);
            var result = sut.GetQuestline(QuestlineId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Count);
        }

        [TestMethod()]
        public void GetNextQuestInQuestlineTest()
        {
            var client = ArrangeClient();
            var sut = ArrangeManager(client);
            var questA = client.GetQuestByID(QuestAId);
            var result = sut.GetNextQuestInQuestline(questA, QuestlineId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(QuestBId, result[0].Id);
        }

        [TestMethod()]
        public void GetPreviousQuestInQuestlineTest()
        {
            var client = ArrangeClient();
            var sut = ArrangeManager(client);
            var questB = client.GetQuestByID(QuestBId);
            var result = sut.GetPreviousQuestInQuestline(questB, QuestlineId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(QuestAId, result[0].Id);
        }

        [TestMethod()]
        public void GetRemainingQuestsInQuestlineTest()
        {
            var client = ArrangeClient();
            var sut = ArrangeManager(client);
            var questA = client.GetQuestByID(QuestAId);
            var result = sut.GetRemainingQuestsInQuestline(questA, QuestlineId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(QuestBId, result[0][0].Id);
        }

        [TestMethod()]
        public void GetPreviousQuestsInQuestlineTest()
        {
            var client = ArrangeClient();
            var sut = ArrangeManager(client);
            var questB = client.GetQuestByID(QuestBId);
            var result = sut.GetPreviousQuestsInQuestline(questB, QuestlineId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(QuestAId, result[0][0].Id);
        }
    }
}
