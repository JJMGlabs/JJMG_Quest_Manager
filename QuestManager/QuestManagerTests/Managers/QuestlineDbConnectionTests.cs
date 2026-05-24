using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using QuestManager.Configuration;
using QuestManager.Managers;
using QuestManagerSharedResources.Model;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.QuestSubObjects;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuestManager.Managers.Tests
{
    [TestClass()]
    public class QuestlineDbConnectionTests
    {
        private static string _tempDir;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "JJMGTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            // Seed quest DB with two quests forming a questline: A → B
            var questA = new Quest { Id = "qst1test1", Name = "Quest A" };
            questA.State = QuestState.CURRENT;
            questA.AddSubObject(new QuestOutcome
            {
                Name = "Next",
                DeliveryMetadata = new Dictionary<string, string>
                {
                    { "questLineId", "ql1" },
                    { "QuestO", "qst1test2" }
                }
            });

            var questB = new Quest { Id = "qst1test2", Name = "Quest B" };
            questB.State = QuestState.CURRENT;
            questB.AddSubObject(new QuestOutcome
            {
                Name = "Terminal",
                DeliveryMetadata = new Dictionary<string, string> { { "questLineId", "ql1" } }
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

        [TestInitialize]
        public void TestInitialize()
        {
            File.WriteAllText(Path.Combine(_tempDir, "questlineDb.json"), "[]");
        }

        private static QuestlineDbConnection ArrangeConnection()
        {
            var questlineOptions = new QuestLineDbConnectionOptions
            {
                BasePath = _tempDir,
                CollectionName = @"\questlineDb.json"
            };
            var questOptions = new QuestDbConnectionOptions
            {
                BasePath = _tempDir,
                CollectionName = @"\questDb.json"
            };

            var questDbConnection = new QuestDbConnection(new TestOptionsMonitor<QuestDbConnectionOptions>(questOptions));
            return new QuestlineDbConnection(new TestOptionsMonitor<QuestLineDbConnectionOptions>(questlineOptions), questDbConnection);
        }

        [TestMethod()]
        public void QuestlineDbConnectionTest()
        {
            try
            {
                var sut = ArrangeConnection();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void GetQuestlinesTest()
        {
            var sut = ArrangeConnection();
            var result = sut.GetQuestlines();
            Assert.IsInstanceOfType(result, typeof(List<QuestlineMetadata>));
        }

        [TestMethod()]
        public void CreateQuestLineTest()
        {
            var sut = ArrangeConnection();
            var result = sut.CreateQuestLine(new QuestlineMetadata { Name = "Test Questline", Description = "Test" });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, sut.GetQuestlines().Count);
            Assert.IsFalse(string.IsNullOrEmpty(sut.GetQuestlines()[0].Id));
        }

        [TestMethod()]
        public void WriteQuestLinesTest()
        {
            var sut = ArrangeConnection();
            sut.CreateQuestLine(new QuestlineMetadata { Name = "Initial", Description = "Test" });
            var questlines = sut.GetQuestlines();
            questlines[0].Name = "Written";
            var result = sut.WriteQuestLines(questlines);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Written", sut.GetQuestlines()[0].Name);
        }

        [TestMethod()]
        public void SaveDbChangesTest()
        {
            var sut = ArrangeConnection();
            sut.CreateQuestLine(new QuestlineMetadata { Name = "Saved Questline", Description = "Test" });
            var result = sut.SaveDbChanges();
            Assert.IsTrue(result.IsSuccess);

            var sut2 = ArrangeConnection();
            Assert.AreEqual(1, sut2.GetQuestlines().Count);
        }

        [TestMethod()]
        public void GetQuestlineTest()
        {
            var sut = ArrangeConnection();
            var result = sut.GetQuestline("ql1");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Count);
        }
    }
}
